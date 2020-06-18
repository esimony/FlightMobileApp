using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightMobileApp.Models
{
    //public enum  Result {ok,NotOk}
    public class AsyncCommand
    {
        public Command Command { get; private set;}
        public Task<ActionResult> Task { get => Completion.Task; }
        public TaskCompletionSource<ActionResult> Completion { get;private set;}
        public AsyncCommand(Command input)
        {
            Command = input;
            Completion = new TaskCompletionSource<ActionResult>
                (TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
