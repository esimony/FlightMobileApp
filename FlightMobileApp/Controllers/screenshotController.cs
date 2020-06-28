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
        private IConfiguration configuration;
        private MyTelnetClient telenet;
        public screenshotController(IConfiguration configuration,MyTelnetClient telenet)
        {
            this.configuration = configuration;
            this.telenet = telenet;
        }
        // GET: /screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!telenet.IsConnected) {
                try {
                    // If we didnt connect to server yet - try connect to it now.
                    string ip = configuration.GetSection("SimulatorInfo").GetSection("IP").Value;
                    int port = Int32.Parse(configuration.GetSection("SimulatorInfo")
                        .GetSection("TelnetPort").Value);
                    telenet.Connect(ip, port);
                    telenet.Write("data\r\n");
                }
                catch {
                    return BadRequest();
                }
            }
            byte[] data = await GetScreenshotFromSimulator();
            if (data != null) {
                // Convert byte array to image.
                return File(data, "image/png");
            } else {
                // If something went wrong.
                return BadRequest();
            }
        }

        private async Task<byte[]> GetScreenshotFromSimulator()
        {
            byte[] data;
            try {
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
                data = ms.ToArray();
            } catch (Exception) {
                data = null;
            }
            return data;
        }
    }
}
