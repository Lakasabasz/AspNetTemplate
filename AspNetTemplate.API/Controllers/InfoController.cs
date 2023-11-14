using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace AspNetTemplate.API.Controllers;

[ApiController, Route("info")]
public class InfoController : Controller
{
    [HttpGet("version")]
    public IActionResult Index()
    {
        return Ok(new
        {
            Assembly = Assembly.GetExecutingAssembly().GetName().Version,
            File = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
            Product = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion
        });
    }
}