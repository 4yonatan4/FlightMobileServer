using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightMobileWeb.Models
{
    public enum Result { Ok, NotOk}
    public class Command
    {
        [JsonProperty("aileron")]
        [JsonPropertyName("aileron")]
        public double Aileron { get; set; }

        [JsonProperty("rudder")]
        [JsonPropertyName("rudder")]
        public double Rudder { get; set; }

        [JsonProperty("elevator")]
        [JsonPropertyName("elevator")]
        public double Elevator { get; set; }

        [JsonProperty("throttle")]
        [JsonPropertyName("throttle")]
        public double Throttle { get; set; }
    }
   

    public class AsyncCommand
    {
        public Command Command { get; private set; }
        public TaskCompletionSource<Result> Completion { get; private set; }
        public Task<Result> Task { get => Completion.Task; }
        public AsyncCommand (Command input)
        {
            Command = input;
            Completion = new TaskCompletionSource<Result>
                (TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
