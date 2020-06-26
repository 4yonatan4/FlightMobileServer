using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightMobileWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ScreenshotController : ControllerBase
    {
        private string Http_port { get; set; }
        private string IP { get; set; }

        public ScreenshotController(IConfiguration config)
        {
            Http_port = config.GetConnectionString("http_port");
            IP = config.GetConnectionString("IP");
        }

        // GET: /Screenshot
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Byte[] b = await GetScreenshot();
            return File(b, "image/jpg");
        }

        public async Task<Byte[]> GetScreenshot()
        {
            string param = "http://" + IP + ":" + Http_port + "/screenshot";
            var response = (dynamic)null;
            try
            {
                response = await MakeRequest(param);
            }
            catch (Exception e)
            {
                Console.WriteLine("problem" + e.Message);
            }
            return response;
        }

        public async Task<dynamic> MakeRequest(string url)
        {
            using var client = new HttpClient
            {
                //set time out for the request
                Timeout = TimeSpan.FromSeconds(10)
            };
            var result = await client.GetByteArrayAsync(url);
            return result;
        }

    }
}
