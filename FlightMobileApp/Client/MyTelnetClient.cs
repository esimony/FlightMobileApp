using FlightMobileApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlightMobileApp.Client
{
    public class MyTelnetClient : ITelnetClient
    {
        private BlockingCollection<AsyncCommand> _queue;
        private TcpClient telnetClient;

        public MyTelnetClient(IConfiguration configuration)
        {
            _queue = new BlockingCollection<AsyncCommand>();
            Disconnected = false;
            telnetClient = new TcpClient();
            // Take ip and port from configuration and connect to server.
            string ip = configuration.GetSection("SimulatorInfo").GetSection("IP").Value;
            int port = Int32.Parse(configuration.GetSection("SimulatorInfo")
                .GetSection("TelnetPort").Value);
            try
            {
                Connect(ip, port);
                Write("data\r\n");

                // Start the "Start" function.
                Start();
            } catch
            {
                // If connect to server failed - continue the program without him
                // and try connect later.
            }
        }
        public bool Disconnected { get; set; }
        public bool IsConnected { 
            get
            {
                return telnetClient.Connected;
            }
        }
        public void Connect(string ip, int port)
        {
            // Define timeout.
            telnetClient.SendTimeout = 10000;
            telnetClient.ReceiveTimeout = 10000;

            // Connect to server.
            telnetClient.Connect(ip, port);
            Disconnected = false;
        }

        public void Write(string command)
        {
            // Convert the message to byte array and send it to server.
            byte[] message;
            message = Encoding.ASCII.GetBytes(command);
            telnetClient.GetStream().Write(message, 0, message.Length);
        }

        public string Read()
        {
            // Read the message from the server to byte array and Convert it to string.
            int numberOfBytesRead;
            byte[] myReadBuffer = new byte[1024];
            string myCompleteMessage;
            numberOfBytesRead = telnetClient.GetStream().Read(myReadBuffer, 0, myReadBuffer.Length);
            myCompleteMessage = Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead);
            return myCompleteMessage;
        }

        public void Disconnect()
        {
            // Close the client and open new one for next time.
            telnetClient.Close();
            Disconnected = true;
            telnetClient = new TcpClient();
        }

        public Task<ActionResult> Execute(Command cmd)
        {
            // Create a AsyncCommand from the Command and push it to _queue
            var asyncCommand = new AsyncCommand(cmd);
            _queue.Add(asyncCommand);
            return asyncCommand.Task;
        }

        public void ProcessCommands()
        {
            ActionResult res;
            foreach (AsyncCommand command in _queue.GetConsumingEnumerable())
            {
                // Define the exeption that could happen.
                Exception exceptionSimulator = new Exception("ErrorFromSimulator");
                Exception exceptionValues = new Exception("WrongValues");
                int result;

                result = CheckPropertyBeforeSend(command,command.Command.Aileron, 
                    "/controls/flight/aileron",-1,1, exceptionSimulator, exceptionValues);
                if (result == -1) {
                    // Here there is a problem with the connection.
                    continue;
                }
                result = CheckPropertyBeforeSend(command,command.Command.Throttle,
                    "/controls/engines/current-engine/throttle",0,1, 
                    exceptionSimulator, exceptionValues);
                if (result == -1) {
                    continue;
                }
                result = CheckPropertyBeforeSend(command,command.Command.Rudder,
                    "/controls/flight/rudder", -1, 1, exceptionSimulator, exceptionValues);
                if (result == -1) {
                    continue;
                }
                result = CheckPropertyBeforeSend(command,command.Command.Elevator, 
                    "/controls/flight/elevator", -1 ,1, exceptionSimulator, exceptionValues);
                if (result == -1) {
                    continue;
                }
                // If everything was fine.
                res = new OkResult();
                command.Completion.SetResult(res);
            }
        }
        private void Start()
        {
            // Open new task for the commands.
            Task.Factory.StartNew(ProcessCommands);
        }

        private int CheckPropertyBeforeSend(AsyncCommand command, double val, string path,
            int from, int to, Exception exceptionSimulator, Exception exceptionValues)
        {
            // Check about each property if its on range. 
            if (val >= from
                && val <= to)
            {
                // If it is - send the set command to simulator and check if it failed.
                int result = SendToSimulator(path, val.ToString());
                if (result == -1)
                {
                    // Here there is a problem with the connection.
                    command.Completion.SetException(exceptionSimulator);
                    return -1;
                }
                else if (result == -2)
                {
                    // Here there is a problem with the values we got fron server.
                    command.Completion.SetException(exceptionValues);
                    return -1;
                }
            }
            return 0;
        }

        private int SendToSimulator(string path,string property)
        {
            string messageFromSimulator;
            try
            {
                // Send to simulator set command.
                Write("set " + path + " " + property + "\r\n");
                // Get the value to check if the change succeed
                Write("get "+path+"\r\n");
                messageFromSimulator = Read();
                if (messageFromSimulator.Contains("ERR") || Double.Parse(messageFromSimulator) != Double.Parse(property))
                {
                    // If something went wrong.
                    return -2;
                }
            }
            catch
            {
                // If one of the commands failed.
                return -1;
            }
            return 0;
        }
    }
}
