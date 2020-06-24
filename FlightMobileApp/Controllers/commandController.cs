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
        private MyTelnetClient telenet;
        public commandController(MyTelnetClient telenet)
        {
            this.telenet = telenet;
        }

        // POST: api/command
        [HttpPost]
        public ActionResult<string> SendCommand([FromBody] Command c)
        {
            try
            {
                telenet.Execute(c);
            }
            catch
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
