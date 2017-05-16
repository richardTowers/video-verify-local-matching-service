using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace local_matching_service.Controllers
{
    [Route("local-matching")]
    public class LocalMatchingController : Controller
    {
        private Random rand = new Random();

        [HttpPost("match")]
        public dynamic Match()
        {
            if (rand.Next(2) == 1) {
                return new { result = "match" };
            }
            return new { result = "no-match" };
        }
    }
}
