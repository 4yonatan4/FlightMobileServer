using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FlightMobileWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;

namespace FlightMobileWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        public FlightGearClient _flightGearClient;

        public CommandController(FlightGearClient flightGearClient)
        {
            _flightGearClient = flightGearClient;
        }

        // POST: api/Command
        [HttpPost]
        public Task<Result> Post([FromBody] Command cmd)
        {
            return _flightGearClient.Execute(cmd);
        }
    }
}
