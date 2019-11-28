using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomMarketingApi.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using static FreedomMarketingApi.Models.DatabaseModel;
using static FreedomMarketingApi.Models.DataModel;

namespace FreedomMarketingApi.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration configuration)
        {
            _config = configuration;
        }

        [Route("wslogin")]
        [HttpPost]
        public async Task<IActionResult> Login(Usuarios model)
        {
            MySqlContext db = new MySqlContext();
            ResponseModel objresult = new ResponseModel();
            LoginResponse login = new LoginResponse();
            JwtManager jwt = new JwtManager(_config);

            try
            {
                Usuarios results = (from x in db.Usuarios
                                    where x.CorreoElectronico == model.CorreoElectronico && x.Contraseña == model.Contraseña
                                    select x).FirstOrDefault();

                if (results == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false, token = "" };
                    objresult.HttpResponse = new { code = 401, message = "Contraseña o correo electronico invalido" };

                    return BadRequest(objresult);
                }

                login.CorreoElectronico = results.CorreoElectronico;
                login.NombreCompleto = results.PrimerNombre + " " + results.PrimerApellido;

                var codigo = (from x in db.Roles
                              where x.CodigoRol == results.CodigoRol
                              select x.Descripcion).FirstOrDefault();

                login.Rol = codigo;

                objresult.FreedomResponse = new { serviceResponse = true, Data = login, token = jwt.GenerateCode(login.CorreoElectronico, login.Rol, login.NombreCompleto) };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, Roles = "" };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }
    }
}