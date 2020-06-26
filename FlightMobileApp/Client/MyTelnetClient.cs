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
        private IConfiguration configuration;

        public MyTelnetClient(IConfiguration configuration)
        {
            _queue = new BlockingCollection<AsyncCommand>();
            Disconnected = false;
            // Try to create TcpClient- check if needed!!!!!!!!!!!!!!!!!!!!!
            try
            {
                telnetClient = new TcpClient();
            }
            catch
            {
                // If something got wrong.
            }
            // Take ip and port from configuration and connect to server.
            string ip = configuration.GetSection("SimulatorInfo").GetSection("IP").Value;
            int port = Int32.Parse(configuration.GetSection("SimulatorInfo").GetSection("TelnetPort").Value);
            try
            {
                connect(ip, port);
                //IsConnected = true;
                write("data\r\n");

                // Start the "Start" function.
                Start();
            } catch
            {
                //IsConnected = false;
            }
        }
        public bool Disconnected { get; set; }
        public bool IsConnected { 
            get
            {
                return telnetClient.Connected;
            }
        }
        public void connect(string ip, int port)
        {
            // Define timeout
            telnetClient.SendTimeout = 10000;
            telnetClient.ReceiveTimeout = 10000;

            // Connect to server.
            telnetClient.Connect(ip, port);
            Disconnected = false;
        }

        public void write(string command)
        {
            // Convert the message to byte array and send it to server.
            byte[] message;
            message = Encoding.ASCII.GetBytes(command);
            telnetClient.GetStream().Write(message, 0, message.Length);
        }

        public string read()
        {
            // Read the message from the server to byte array and Convert it to string.
            int numberOfBytesRead;
            byte[] myReadBuffer = new byte[1024];
            string myCompleteMessage;
            numberOfBytesRead = telnetClient.GetStream().Read(myReadBuffer, 0, myReadBuffer.Length);
            myCompleteMessage = Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead);
            return myCompleteMessage;
        }

        public void disconnect()
        {
            // Close the client.
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
                string path;

                // Check about each property if its on range. 
                if (command.Command.Aileron >= -1 
                    && command.Command.Aileron <= 1)
                {
                    // If it is - send the set command to simulator and check if it failed
                    path = "/controls/flight/aileron";
                    if (SendToSimulator(path, command.Command.Aileron.ToString()) == -1)
                    {
                        // Here there is a problem with the connection.
                        command.Completion.SetException(exceptionSimulator);
                        continue;
                    } else if (SendToSimulator(path, command.Command.Aileron.ToString()) == -2) {
                        // Here there is a problem with the values we got fron server.
                        command.Completion.SetException(exceptionValues);
                        continue;
                    }
                }

                // All of the property is the same idea like before.
                if (command.Command.Throttle >= 0
                    && command.Command.Throttle <= 1)
                {
                    path = "/controls/engines/current-engine/throttle";
                    if (SendToSimulator(path, command.Command.Throttle.ToString()) == -1)
                    {
                        command.Completion.SetException(exceptionSimulator);
                        continue;
                    } else if (SendToSimulator(path, command.Command.Throttle.ToString()) == -2)
                    {
                        command.Completion.SetException(exceptionValues);
                        continue;
                    }
                }
                if (command.Command.Rudder >= -1
                    && command.Command.Rudder <= 1)
                {
                    path = "/controls/flight/rudder";
                    if (SendToSimulator(path, command.Command.Rudder.ToString()) == -1)
                    {
                        command.Completion.SetException(exceptionSimulator);
                        continue;
                    } else if (SendToSimulator(path, command.Command.Rudder.ToString()) == -2)
                    {
                        command.Completion.SetException(exceptionValues);
                        continue;
                    }
                }
                if (command.Command.Elevator >= -1
                    && command.Command.Elevator <= 1)
                {
                    path = "/controls/flight/elevator";
                    if (SendToSimulator(path, command.Command.Elevator.ToString()) == -1)
                    {
                        command.Completion.SetException(exceptionSimulator);
                        continue;
                    } else if (SendToSimulator(path, command.Command.Elevator.ToString()) == -2) {
                        command.Completion.SetException(exceptionValues);
                        continue;
                    }
                }

                // If everything was fine.
                res = new OkResult();
                command.Completion.SetResult(res);
            }
        }
        public void Start()
        {
            // Open new task for the commands.
            Task.Factory.StartNew(ProcessCommands);
        }

        public int SendToSimulator(string path,string property)
        {
            string messageFromSimulator;
            try
            {
                // Send to simulator set command.
                write("set " + path + " " + property + "\r\n");
                // Get the value to check if the change succeed
                write("get "+path+"\r\n");
                messageFromSimulator = read();
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
