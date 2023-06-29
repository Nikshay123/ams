using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebApp.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        [Route("api/[controller]")]
        public ActionResult GetVersionNumber()
        {
            return Ok($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
        }
    }
}