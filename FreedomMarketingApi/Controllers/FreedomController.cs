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

                model.RoleCode = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.UtcNow.AddHours(-5).ToString();

                db.Roles.Add(model);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true, Role = model };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, Role = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

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

                if (!String.IsNullOrEmpty(model.Description))
                    results.Description = model.Description;

                db.Entry(results).State = EntityState.Modified;
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true, Role = results };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, Role = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

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
                objresult.HttpResponse = new { code = 400, message = e.Message };

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
                               where !String.IsNullOrEmpty(rol) ? x.Description.Contains(rol) : x.Description.Contains("")
                               select x).ToList();

                objresult.FreedomResponse = new { serviceResponse = true, Roles = results };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, Roles = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

                return BadRequest(objresult);
            }
        }

        #endregion

        #region Usuarios

        [Route("wscrearusuario")]
        [HttpPost]
        public async Task<IActionResult> AgregarUsuario(Users model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                model.UserCode = Guid.NewGuid().ToString();
                model.CreateDate = DateTime.UtcNow.AddHours(-5).ToString();
                model.Points = 0;
                model.ReferenceCode = "CR" + Guid.NewGuid().ToString();
                model.Status = true;

                var codigo = (from x in db.Roles
                              where x.Description == model.RoleCode
                              select x.RoleCode).FirstOrDefault();

                if (codigo == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false };
                    objresult.HttpResponse = new { code = 400, message = "El rol ingresado no existe en la base de datos" };

                    return BadRequest(objresult);
                }

                model.RoleCode = codigo;

                db.Users.Add(model);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true, User = model};
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false };
                objresult.HttpResponse = new { code = 400, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Route("wsmodificarusuario")]
        [HttpPost]
        public async Task<IActionResult> ModificarUsuario(Users model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();

                var results = (from x in db.Users
                                    where x.idUsers == model.idUsers
                                    select x).FirstOrDefault();

                if (!String.IsNullOrEmpty(model.FirstName))
                    results.FirstName = model.FirstName;

                if (!String.IsNullOrEmpty(model.SecondName))
                    results.SecondName = model.SecondName;

                if (!String.IsNullOrEmpty(model.FirstLastName))
                    results.FirstLastName = model.FirstLastName;

                if (!String.IsNullOrEmpty(model.SecondLastName))
                    results.SecondLastName = model.SecondLastName;

                if (!String.IsNullOrEmpty(model.Identification))
                    results.Identification = model.Identification;

                if (!String.IsNullOrEmpty(model.Email))
                    results.Email = model.Email;

                if (!String.IsNullOrEmpty(model.Telephone))
                    results.Telephone = model.Telephone;

                if (!String.IsNullOrEmpty(model.Address))
                    results.Address = model.Address;

                if (!String.IsNullOrEmpty(model.Country))
                    results.Country = model.Country;

                if (!String.IsNullOrEmpty(model.ReferenceCode))
                    results.ReferenceCode = model.ReferenceCode;

                if (!String.IsNullOrEmpty(model.Points.ToString()))
                    results.Points = model.Points;

                if (!String.IsNullOrEmpty(model.RoleCode))
                {
                    var codigo = (from x in db.Roles
                                  where x.Description == model.RoleCode
                                  select x.RoleCode).FirstOrDefault();

                    if (codigo == null)
                    {
                        objresult.FreedomResponse = new { serviceResponse = false };
                        objresult.HttpResponse = new { code = 400, message = "El rol ingresado no existe en la base de datos" };

                        return BadRequest(objresult);
                    }

                    model.RoleCode = codigo;
                    results.RoleCode = model.RoleCode;
                }

                if (!String.IsNullOrEmpty(model.Status.ToString()))
                    results.Status = model.Status;

                if (!String.IsNullOrEmpty(model.PaymentAccount))
                    results.PaymentAccount = model.PaymentAccount;

                if (!String.IsNullOrEmpty(model.Password))
                    results.Password = model.Password;

                if (!String.IsNullOrEmpty(model.MassiveMail.ToString()))
                    results.MassiveMail = model.MassiveMail;

                db.Entry(results).State = EntityState.Modified;
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true, User = results };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, User = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

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

                var results = (from x in db.Users
                               where x.idUsers == id
                               select x).FirstOrDefault();

                if (results == null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false };
                    objresult.HttpResponse = new { code = 404, message = "El usuario no existe en la base de datos" };

                    return NotFound(objresult);
                }

                db.Users.Remove(results);
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, User = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

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

                var results = (from x in db.Users
                               where !String.IsNullOrEmpty(usuario) ? x.Email.Contains(usuario) : x.Email.Contains("")
                               select x).ToList();

                objresult.FreedomResponse = new { serviceResponse = true, User = results };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, User = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

                return BadRequest(objresult);
            }
        }

        #endregion

    }
}