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
        public async Task<IActionResult> Login(Users model)
        {
            MySqlContext db = new MySqlContext();
            ResponseModel objresult = new ResponseModel();
            LoginResponse login = new LoginResponse();
            JwtManager jwt = new JwtManager(_config);

            try
            {
                var results = (from x in db.Users
                                    where x.Email == model.Email && x.Password == model.Password
                                    select x).FirstOrDefault();

                if (results == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false, token = "" };
                    objresult.HttpResponse = new { code = 401, message = "Contraseña o correo electronico invalido" };

                    return BadRequest(objresult);
                }

                login.Email = results.Email;
                login.FullName = results.FirstName + " " + results.LastName;

                var codigo = (from x in db.Roles
                              where x.RoleCode == results.RoleCode
                              select x.Description).FirstOrDefault();

                login.Role = codigo;

                objresult.FreedomResponse = new { serviceResponse = true, Data = login, token = jwt.GenerateCode(login.Email, login.Role, login.FullName) };
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