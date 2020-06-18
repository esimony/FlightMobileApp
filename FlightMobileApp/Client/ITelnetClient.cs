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
        void connect(string ip, int port);
        // Writing to server.
        void write(string command);
        // Blocking call
        string read();
        // Disconncting to server
        void disconnect();
        Task<ActionResult> Execute(Command cmd);
    }
}
