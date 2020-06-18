using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightMobileApp.Models
{
    public class Command
    {
        public Command()
        {
            Aileron = -2;
            Rudder = -2;
            Elevator = -2;
            Throttle = -2;

        }
        public double Aileron { get; set; }
        public double Rudder { get; set; }
        public double Elevator { get; set; }
        public double Throttle { get; set; }
    }
}
