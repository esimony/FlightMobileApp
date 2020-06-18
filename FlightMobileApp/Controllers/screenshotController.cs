using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class screenshotController : ControllerBase
    {
        private IConfiguration configuration;
        public screenshotController(IConfiguration config)
        {
            configuration = config;
        }
        // GET: /screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Create the request.
/*            string ip = config.GetSection("SimulatorInfo").GetSection("IP").Value;
            int port = Int32.Parse(config.GetSection("SimulatorInfo").GetSection("HttpPort").Value);
            string strurl = string.Format(ip + ":" + port + "/screenshot");*/
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("https://upload.wikimedia.org/wikipedia/commons/c/c9/Moon.jpg");
            myRequest.Timeout = 10000;
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            MemoryStream ms = new MemoryStream();
            myResponse.GetResponseStream().CopyTo(ms);
            byte[] data = ms.ToArray();

            return File(data, "image/png");
            //Byte[] image = await commandManager.GetScreenShot(configuration);
            //if (image == null)
            //{
            //    return BadRequest();
            //
            //return Ok(image);
        }
    }
}
