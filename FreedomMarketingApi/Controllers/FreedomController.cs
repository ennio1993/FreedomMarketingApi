using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FreedomMarketingApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static FreedomMarketingApi.Models.DatabaseModel;
using static FreedomMarketingApi.Models.DataModel;

namespace FreedomMarketingApi.Controllers
{
    //[Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FreedomController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _token;

        public FreedomController(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _config = configuration;
            _httpContext = accessor;

            var accessToken = _httpContext.HttpContext.Request.Headers["Authorization"].ToString();

            if (accessToken != null)
            {
                if (accessToken.StartsWith("Bearer ") || accessToken.Contains("Bearer "))
                {
                    _token = accessToken.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid Token");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid Token");
            }
        }    

        #region Roles

        [Route("wscrearrol")]
        [HttpPost]
        public async Task<IActionResult> AgregarRoles(Roles model)
        {
            ResponseModel objresult = new ResponseModel();
            try
            {
                MySqlContext db = new MySqlContext();

                model.CodigoRol = Guid.NewGuid().ToString();
                model.FechaCreacion = DateTime.UtcNow.AddHours(-5).ToString();

                db.Roles.Add(model);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wsmodificarrol")]
        [HttpPost]
        public async Task<IActionResult> ModificarRoles(Roles model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                Roles results = (from x in db.Roles
                                 where x.idRoles == model.idRoles
                                 select x).FirstOrDefault();

                if (!String.IsNullOrEmpty(model.Descripcion))
                    results.Descripcion = model.Descripcion;

                db.Entry(results).State = EntityState.Modified;
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wseliminarrol")]
        [HttpDelete]
        public async Task<IActionResult> EliminarRoles(int? rol = null)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                Roles results = (from x in db.Roles
                                 where x.idRoles == rol
                                 select x).FirstOrDefault();

                if (results == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false };
                    objresult.HttpResponse = new { code = 400, message = "El rol no existe en la base de datos" };

                    return BadRequest(objresult);
                }

                db.Roles.Remove(results);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wslistarrol")]
        [HttpGet]
        public async Task<IActionResult> ListarRoles(string rol = null)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                var results = (from x in db.Roles
                               where !String.IsNullOrEmpty(rol) ? x.Descripcion.Contains(rol) : x.Descripcion.Contains("")
                               select x).ToList();

                objresult.FreedomResponse = new { serviceResponse = true, Roles = results };
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

        #endregion

        #region Usuarios

        [Route("wscrearusuario")]
        [HttpPost]
        public async Task<IActionResult> AgregarUsuario(Usuarios model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                model.CodigoUsuario = Guid.NewGuid().ToString();
                model.FechaCreacion = DateTime.UtcNow.AddHours(-5).ToString();
                model.Puntos = 0;
                model.CodigoReferencia = "CR" + Guid.NewGuid().ToString();
                model.Estado = true;

                var codigo = (from x in db.Roles
                              where x.Descripcion == model.CodigoRol
                              select x.CodigoRol).FirstOrDefault();

                if (codigo == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false };
                    objresult.HttpResponse = new { code = 400, message = "El rol ingresado no existe en la base de datos" };

                    return BadRequest(objresult);
                }

                model.CodigoRol = codigo;

                db.Usuarios.Add(model);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wsmodificarusuario")]
        [HttpPost]
        public async Task<IActionResult> ModificarUsuario(Usuarios model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                Usuarios results = (from x in db.Usuarios
                                    where x.idUsuarios == model.idUsuarios
                                    select x).FirstOrDefault();

                if (!String.IsNullOrEmpty(model.PrimerNombre))
                    results.PrimerNombre = model.PrimerNombre;

                if (!String.IsNullOrEmpty(model.SegundoNombre))
                    results.SegundoNombre = model.SegundoNombre;

                if (!String.IsNullOrEmpty(model.PrimerApellido))
                    results.PrimerApellido = model.PrimerApellido;

                if (!String.IsNullOrEmpty(model.SegundoApellido))
                    results.SegundoApellido = model.SegundoApellido;

                if (!String.IsNullOrEmpty(model.Identificacion))
                    results.Identificacion = model.Identificacion;

                if (!String.IsNullOrEmpty(model.CorreoElectronico))
                    results.CorreoElectronico = model.CorreoElectronico;

                if (!String.IsNullOrEmpty(model.Telefono))
                    results.Telefono = model.Telefono;

                if (!String.IsNullOrEmpty(model.Direccion))
                    results.Direccion = model.Direccion;

                if (!String.IsNullOrEmpty(model.Pais))
                    results.Pais = model.Pais;

                if (!String.IsNullOrEmpty(model.CodigoReferencia))
                    results.CodigoReferencia = model.CodigoReferencia;

                if (!String.IsNullOrEmpty(model.Puntos.ToString()))
                    results.Puntos = model.Puntos;

                if (!String.IsNullOrEmpty(model.CodigoRol))
                {
                    var codigo = (from x in db.Roles
                                  where x.Descripcion == model.CodigoRol
                                  select x.CodigoRol).FirstOrDefault();

                    if (codigo == null)
                    {
                        objresult.FreedomResponse = new { serviceResponse = false };
                        objresult.HttpResponse = new { code = 400, message = "El rol ingresado no existe en la base de datos" };

                        return BadRequest(objresult);
                    }

                    model.CodigoRol = codigo;
                    results.CodigoRol = model.CodigoRol;
                }

                if (!String.IsNullOrEmpty(model.Estado.ToString()))
                    results.Estado = model.Estado;

                if (!String.IsNullOrEmpty(model.CuentadePago))
                    results.CuentadePago = model.CuentadePago;

                if (!String.IsNullOrEmpty(model.Contraseña))
                    results.Contraseña = model.Contraseña;

                if (!String.IsNullOrEmpty(model.CorreoMasivo.ToString()))
                    results.CorreoMasivo = model.CorreoMasivo;

                db.Entry(results).State = EntityState.Modified;
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wseliminarusuario")]
        [HttpDelete]
        public async Task<IActionResult> EliminarUsuario(int? id = null)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                var results = (from x in db.Usuarios
                               where x.idUsuarios == id
                               select x).FirstOrDefault();

                if (results == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false };
                    objresult.HttpResponse = new { code = 404, message = "El usuario no existe en la base de datos" };

                    return NotFound(objresult);
                }

                db.Usuarios.Remove(results);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 404, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wslistarusuarios")]
        [HttpGet]
        public async Task<IActionResult> ListarUsuarios(string usuario = null)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                var results = (from x in db.Usuarios
                               where !String.IsNullOrEmpty(usuario) ? x.CorreoElectronico.Contains(usuario) : x.CorreoElectronico.Contains("")
                               select x).ToList();

                objresult.FreedomResponse = new { serviceResponse = true, Roles = results };
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

        #endregion

    }
}