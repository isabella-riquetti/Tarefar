using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Tarefar.DB.Models;
using Tarefar.Infra.Models;
using Tarefar.Services.Helpers;
using Tarefar.Services.Models.Authentication;
using Tarefar.Services.Models.Events;
using Tarefar.Services.Services;

namespace Tarefar.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(
            IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="newEvent">New event to be created</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create([FromBody] CreateEventModel newEvent)
        {
            var result = _eventService.Create(newEvent);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok();
        }
    }
}
