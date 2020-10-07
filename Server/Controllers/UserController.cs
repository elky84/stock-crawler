using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly UserService _userService;

        public UserController(ILogger<UserController> logger,
            UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        public async Task<Protocols.Response.Users> Get()
        {
            return new Protocols.Response.Users
            {
                Datas = (await _userService.Get()).ConvertAll(x => x.ToProtocol())
            };
        }

        [HttpPost]
        public async Task<Protocols.Response.User> Create([FromBody] Protocols.Request.User user)
        {
            return await _userService.Create(user);
        }

        [HttpGet("UserId/{id}")]
        public async Task<Protocols.Response.User> GetByUserId(string id)
        {
            return new Protocols.Response.User
            {
                Data = (await _userService.GetByUserId(id)).ToProtocol()
            };
        }

        [HttpGet("{id}")]
        public async Task<Protocols.Response.User> Get(string id)
        {
            return await _userService.Get(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.User> Update(string id, [FromBody] Protocols.Request.User user)
        {
            return await _userService.Update(id, user);
        }


        [HttpPut("UserId/{id}")]
        public async Task<Protocols.Response.User> UpdateByUserId(string id, [FromBody] Protocols.Request.User user)
        {
            return await _userService.UpdateByUserId(id, user);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.User> Delete(string id)
        {
            return await _userService.Delete(id);
        }

        [HttpDelete("UserId/{id}")]
        public async Task<Protocols.Response.User> DeleteByUserId(string id)
        {
            return await _userService.DeleteByUserId(id);
        }

    }
}
