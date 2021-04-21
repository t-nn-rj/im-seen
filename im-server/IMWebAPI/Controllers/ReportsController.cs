﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMWebAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IMWebAPI.Controllers
{
    [ApiController]
    [EnableCors]
    [Route("Reports")]
    public class ReportsController : ControllerBase
    {

        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Get")]
        public string Get()
        {
            return "This worked!";
        }
    }
}