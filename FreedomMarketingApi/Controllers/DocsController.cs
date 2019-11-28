using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FreedomMarketingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocsController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Index()
        {
            var environment = "Development";
            var isDevelopment = true;
            ViewBag.isDevelopment = isDevelopment;
            ViewBag.env = environment;
            return View();
        }     
    }
}