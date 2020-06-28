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
            try {
                // Check if task finished with ok code.
                result = task.Result;
            }
            catch (Exception e) {
                int res = IfTaskFailed(e);
                if (res == 1) {
                    return BadRequest();
                } else if (res == 2) {
                    return Conflict();
                }
            }
            return Ok();
        }
        private int IfTaskFailed(Exception e)
        {
            // If task didnt succeed - check why.
            if (e.InnerException.Message == "ErrorFromSimulator")
            {
                if (!telenet.Disconnected)
                {
                    telenet.Disconnect();
                }
                return 1;
            }
            else if (e.InnerException.Message == "WrongValues")
            {
                return 2;
            }
            return 1;
        }
    }
}
