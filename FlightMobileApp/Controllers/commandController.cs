using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightMobileApp.Client;
using FlightMobileApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class commandController : ControllerBase
    {
        private ITelnetClient telenet;
        private IConfiguration configuration;
        public commandController(IConfiguration iconfg,ITelnetClient telenet)
        {
            string ip = iconfg.GetSection("SimulatorInfo").GetSection("IP").Value;
            int port = Int32.Parse(iconfg.GetSection("SimulatorInfo").GetSection("TelnetPort").Value);
            this.telenet = telenet;
            configuration = iconfg;
            telenet.connect(ip, port);
            telenet.write("data\r\n");
        }

        // POST: api/command
        [HttpPost]
        public ActionResult<string> SendCommand([FromBody] Command c)
        {
            if (c.Aileron < -1 || c.Aileron > 1 || c.Throttle < 0 || c.Throttle > 1 ||
                c.Elevator < -1 || c.Elevator > 1 || c.Rudder < -1 || c.Rudder > 1)
            {
                return BadRequest();
            }
            /*if (commandManager.SendCommand(c,configuration) == -1)
            {
                return BadRequest();
            }*/
            telenet.Execute(c);
            return Ok();
        }
    }
}
