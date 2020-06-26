using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlightMobileWeb.Models
{
    public class FlightGearClient
    {
        private readonly BlockingCollection<AsyncCommand> _queue;
        private readonly TcpClient _client;
        readonly string _ip;
        readonly string _socket_port;
        private StreamReader _sr;
        private StreamWriter _sw;

        public FlightGearClient(IConfiguration config)
        {
            _queue = new BlockingCollection<AsyncCommand>();
            _client = new TcpClient();
            _ip = config.GetConnectionString("IP");
            _socket_port = config.GetConnectionString("socket_port");
            Start();
        }

        public Task<Result> Execute(Command cmd)
        {
            var asyncCommand = new AsyncCommand(cmd);
            _queue.Add(asyncCommand);
            return asyncCommand.Task;
        }
        public void Start()
        {
            Task.Factory.StartNew(ProcessCommands);
        }

        public void ProcessCommands()
        {
            _client.Connect(_ip, Int32.Parse(_socket_port));
            _sr = new StreamReader(_client.GetStream());
            _sw = new StreamWriter(_client.GetStream());
            _sw.WriteLine("data\n");
            _sw.Flush();
            foreach (AsyncCommand command in _queue.GetConsumingEnumerable())
            {
                Result res;
                // send command to flightgear
                // then check the value - i.e. send another command to get the value
                // and then check
                res = SetRudder(command.Command.Rudder.ToString());
                if (res == Result.NotOk)
                    command.Completion.SetResult(res);
                res = SetAileron(command.Command.Aileron.ToString());
                if (res == Result.NotOk)
                    command.Completion.SetResult(res);
                System.Console.WriteLine("after set aileron");
                res = SetElevator(command.Command.Elevator.ToString());
                if (res == Result.NotOk)
                    command.Completion.SetResult(res);
                res = SetThrottle(command.Command.Throttle.ToString());
                // anyway update the result of the task
                command.Completion.SetResult(res);
                System.Console.WriteLine("finish");
            }
        }

        public Result SetThrottle(string s)
        {
            string toSend = "set " + "/controls/engines/current-engine/throttle " + s + "\n";
            _sw.WriteLine(toSend);
            return ValidSet(s, "Throttle");
        }

        public Result SetRudder(string s)
        {
            string toSend = "set " + "/controls/flight/rudder " + s + "\n";
            _sw.WriteLine(toSend);
            _sw.Flush();
            return ValidSet(s, "Rudder");
        }

        public Result SetElevator(string s)
        {
            string toSend = "set " + "/controls/flight/elevator " + s + "\n";
            _sw.WriteLine(toSend);
            System.Console.WriteLine("after write aileron");
            return ValidSet(s, "Elevator");
        }

        public Result SetAileron(string s)
        {
            string toSend = "set " + "/controls/flight/aileron " + s + "\n";
            _sw.WriteLine(toSend);
            return ValidSet(s, "Aileron");
        }

 
        public Result ValidSet(string source, string variable)
        {
            string var;
            switch (variable)
            {
                case "Throttle":
                    var = "/controls/engines/engine[0]/throttle";
                    break;
                case "Aileron":
                    var = "/controls/flight/aileron";
                    break;
                case "Elevator":
                    var = "/controls/flight/elevator";
                    break;
                case "Rudder":
                    var = "/controls/flight/rudder";
                    break;
                default:
                    Console.WriteLine("json file invalid");
                    return Result.NotOk;
            }
            _sw.Write("get " + var + "\n");
            string current = _sr.ReadLine();
            if (!Double.TryParse(source, out double d1))
            {
                Console.WriteLine("json file invalid");
                return Result.NotOk;
            }
            if (!Double.TryParse(current, out double d2))
            {
                Console.WriteLine("get invalid value from the simulator");
                return Result.NotOk;
            }
            string s1 = String.Format("{0:0.00000000}", d1);
            string s2 = String.Format("{0:0.00000000}", d2);
            if (!s1.Equals(s2))
            {
                // Error
                Console.WriteLine("need to be " + source + " but get: " + current);
                Console.WriteLine("error post " + variable);
                return Result.NotOk;
            }
            return Result.Ok;
        }

    }
}
