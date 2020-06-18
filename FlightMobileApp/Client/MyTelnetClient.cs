using FlightMobileApp.Models;
using Microsoft.AspNetCore.Mvc;
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
        
        public MyTelnetClient()
        {
            _queue = new BlockingCollection<AsyncCommand>();
            // Try to create TcpClient.
            try
            {
                telnetClient = new TcpClient();
            }
            catch
            {
                // If something got wrong- dont do anything (the model will send a message).
            }
            Start();
        }
        public void connect(string ip, int port)
        {
            // Define timeout
            telnetClient.SendTimeout = 10000;
            telnetClient.ReceiveTimeout = 10000;

            // Connect to server.
            telnetClient.Connect(ip, port);
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
        }

        public Task<ActionResult> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            _queue.Add(asyncCommand);
            return asyncCommand.Task;
        }

        public void ProcessCommands()
        {
            ActionResult res;
            foreach (AsyncCommand command in _queue.GetConsumingEnumerable())
            {
                //Task has multiple exceptions.
                //var faultedTask = Task.WhenAll(Task.Run(() => { throw new Exception("ErrorFromSimulator"); }), Task.Run(() => { throw new Exception("ErrorFromSimulator"); }));
                Exception exception = new Exception("ErrorFromSimulator");
                string path = "/controls/flight/aileron";
                if (SendToSimulator(path, command.Command.Aileron.ToString()) == -1)
                {
                    command.Completion.SetException(exception);
                }
                path = "/controls/engines/current-engine/throttle";
                if (SendToSimulator(path, command.Command.Throttle.ToString()) == -1)
                {
                    command.Completion.SetException(exception);
                }
                path = "/controls/flight/rudder";
                if (SendToSimulator(path, command.Command.Rudder.ToString()) == -1)
                {
                    command.Completion.SetException(exception);
                }
                path = "/controls/flight/elevator";
                if (SendToSimulator(path, command.Command.Elevator.ToString()) == -1)
                {
                    command.Completion.SetException(exception);
                }
                res = new OkResult();
                command.Completion.SetResult(res);
            }
        }
        public void Start()
        {
            try
            {
                Task.Factory.StartNew(ProcessCommands);
            }
            catch
            {

            }
        }

        public int SendToSimulator(string path,string property)
        {
            string messageFromSimulator;
            write("set "+ path + " " + property + "\r\n");
            try
            {
                write("get "+path+"\r\n");
                try
                {
                    messageFromSimulator = read();
                    if (messageFromSimulator.Contains("ERR") || Double.Parse(messageFromSimulator) != Double.Parse(property))
                    {
                        return -1;
                    }
                }
                catch
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
            return 0;
        }
    }
}
