using FlightMobileApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightMobileApp.Client
{
    public interface ITelnetClient
    {
        // Connecting to server
        void Connect(string ip, int port);
        // Writing to server.
        void Write(string command);
        // Blocking call
        string Read();
        // Disconncting to server
        void Disconnect();
        Task<ActionResult> Execute(Command cmd);
    }
}
