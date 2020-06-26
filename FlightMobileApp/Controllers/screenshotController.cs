using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FlightMobileApp.Client;
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
        // ask noaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        private IConfiguration configuration;
        private MyTelnetClient telenet;
        //private static Mutex mut = new Mutex();
        public screenshotController(IConfiguration configuration,MyTelnetClient telenet)
        {
            this.configuration = configuration;
            this.telenet = telenet;
        }
        // GET: /screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!telenet.IsConnected)
            {
                //telenet.IsConnected = true;
                try
                {
                    string ip = configuration.GetSection("SimulatorInfo").GetSection("IP").Value;
                    int port = Int32.Parse(configuration.GetSection("SimulatorInfo")
                        .GetSection("TelnetPort").Value);
                    telenet.connect(ip, port);
                    telenet.write("data\r\n");
                }
                catch
                {
                    //telenet.IsConnected = false;
                    return BadRequest();
                }
            }
            try
            {
                // Create the request. - here need to change to real url!!!!!!!!!!!!!!!!!!!!!!!!
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest
                    .Create("http://localhost:5000/screenshot");
                string ip = configuration.GetSection("SimulatorInfo").GetSection("IP").Value;
                int port = Int32.Parse(configuration.GetSection("SimulatorInfo")
                    .GetSection("HttpPort").Value);
                //  Add this!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                /*HttpWebRequest myRequest = (HttpWebRequest)WebRequest
                .Create("http://"+ip+":"+port+"/screenshot");*/
                myRequest.Timeout = 10000;
                myRequest.Method = "GET";
                WebResponse myResponse = await myRequest.GetResponseAsync();

                // Convert the response to byte array.
                MemoryStream ms = new MemoryStream();
                myResponse.GetResponseStream().CopyTo(ms);
                byte[] data = ms.ToArray();

                // Convert byte array to image.
                return File(data, "image/png");
            }
            catch (Exception)
            {
                // If something went wrong.
                return BadRequest();
            }
        }
    }
}
