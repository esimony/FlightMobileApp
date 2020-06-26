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
            // Send command to execute and check if it succeed.
            Task<ActionResult> task = telenet.Execute(c);
            ActionResult result;
            try
            {
                result = task.Result;
            }
            catch (Exception e)
            {
                if (!telenet.Disconnected)
                {
                    telenet.disconnect();
                }
                if(e.InnerException.Message== "ErrorFromSimulator")
                {
                    return BadRequest();
                }
                else if(e.InnerException.Message == "WrongValues")
                {
                    return Conflict();
                }
                return BadRequest();
            }
            //System.Threading.Thread.Sleep(15000);
            return Ok();
        }
    }
}
