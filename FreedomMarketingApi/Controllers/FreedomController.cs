using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FreedomMarketingApi.Helpers;
using FreedomMarketingApi.Models;
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
    [ApiController]
    public class FreedomController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _token;

        public FreedomController(IConfiguration configuration)
        {
            _config = configuration;
        }

        #region Roles

        [Authorize]
        [Route("wscrearrol")]
        [HttpPost]
        public async Task<IActionResult> AgregarRoles(Roles model)
        {
            ResponseModel objresult = new ResponseModel();
            try
            {
                MySqlContext db = new MySqlContext();
                DataManagement management = new DataManagement();

                model.RoleCode = Guid.NewGuid().ToString();
                model.CreateDate = management.LocalDateTime();
                model.LastModifiedDate = null;

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

        [Authorize]
        [Route("wsmodificarrol")]
        [HttpPost]
        public async Task<IActionResult> ModificarRoles(Roles model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();
                DataManagement management = new DataManagement();

                Roles results = (from x in db.Roles
                                 where x.idRoles == model.idRoles
                                 select x).FirstOrDefault();

                if (!String.IsNullOrEmpty(model.Description))
                    results.Description = model.Description;

                results.LastModifiedDate = management.LocalDateTime();

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

        [Authorize]
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

        [Authorize]
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
                DataManagement management = new DataManagement();
                LoginResponse newResponse = new LoginResponse();
                JwtManager jwt = new JwtManager(_config);

                var validateUser = (from x in db.Users
                                    where x.Email == model.Email || x.Identification == model.Identification
                                    select x).FirstOrDefault();

                if(validateUser != null)
                {
                    objresult.FreedomResponse = new { serviceResponse = false, User = "", token = "" };
                    objresult.HttpResponse = new { code = 400, message = "El correo/identificacion ingresado, ya esta siendo utilizado" };

                    return BadRequest(objresult);
                }

                model.UserCode = management.CreateReferenceCode();
                model.CreateDate = management.LocalDateTime();
                model.Points = 0;
                model.ReferenceCode = model.ReferenceCode;
                model.Status = false;
                model.MassiveMail = false;
                model.RoleCode = "eb3bd4c7-c621-432a-8066-3be8314b7fc2";
                model.PaymentId = null;
                model.LastModifiedDate = null;

                db.Users.Add(model);
                db.SaveChanges();

                var codigo = (from x in db.Roles
                              where x.RoleCode == model.RoleCode
                              select x.Description).FirstOrDefault();

                newResponse.Email = model.Email;
                newResponse.Role = codigo;
                newResponse.FullName = model.FirstName + " " + model.LastName;
                newResponse.Status = model.Status;

                objresult.FreedomResponse = new { serviceResponse = true, User = newResponse, token = jwt.GenerateCode(newResponse.Email, newResponse.Role, newResponse.FullName) };
                objresult.HttpResponse = new { code = 200, message = "Ok" };

                return Ok(objresult);
            }
            catch (Exception e)
            {
                objresult.FreedomResponse = new { serviceResponse = false, User = "", token = "" };
                objresult.HttpResponse = new { code = 400, message = e.Message };

                return BadRequest(objresult);
            }
        }

        [Authorize]
        [Route("wsmodificarusuario")]
        [HttpPost]
        public async Task<IActionResult> ModificarUsuario(Users model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();
                DataManagement management = new DataManagement();

                var results = (from x in db.Users
                                    where x.idUsers == model.idUsers
                                    select x).FirstOrDefault();

                if (!String.IsNullOrEmpty(model.FirstName))
                    results.FirstName = model.FirstName;

                if (!String.IsNullOrEmpty(model.LastName))
                    results.LastName = model.LastName;

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

                if (!String.IsNullOrEmpty(model.Password))
                    results.Password = model.Password;

                if (!String.IsNullOrEmpty(model.MassiveMail.ToString()))
                    results.MassiveMail = model.MassiveMail;

                results.LastModifiedDate = management.LocalDateTime();

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

        [Authorize]
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

        [Authorize]
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

        #region Payments

        [Authorize]
        [Route("wscrearpago")]
        [HttpPost]
        public async Task<IActionResult> CrearPago(PaymentModel model)
        {
            ResponseModel objresult = new ResponseModel();

            try
            {
                MySqlContext db = new MySqlContext();
                DataManagement management = new DataManagement();
                Payment payment = new Payment();

                var date = DateTime.Parse(model.PaymentDate).ToString("MM/dd/yyyy h:mm tt");

                payment.PaymentDate = date;
                payment.CreateDate = management.LocalDateTime();
                payment.PaymentSlip = model.PaymentSlip;

                db.Payment.Add(payment);
                db.SaveChanges();

                var data = (from x in db.Users
                             where x.Identification == model.Identification
                             select x).FirstOrDefault();

                data.PaymentId = payment.PaymentId.ToString();

                if (data != null)
                {
                    if(data.UserCode == data.ReferenceCode) 
                        data.Points = data.Points + 1;
                }

                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                objresult.FreedomResponse = new { serviceResponse = true, Payment = payment };
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

        #endregion

    }
}