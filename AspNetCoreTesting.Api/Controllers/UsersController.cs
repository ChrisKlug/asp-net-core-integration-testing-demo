using AspNetCoreTesting.Api.Data.Entities;
using AspNetCoreTesting.Api.Models;
using AspNetCoreTesting.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreTesting.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsers _users;

        public UsersController(IUsers users)
        {
            _users = users;
        }

        [HttpGet()]
        public async Task<ActionResult<User>> GetUsers()
        {
            return Ok(await _users.All());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _users.WithId(id);
            return user != null ? Ok(user) : NotFound();
        }

        [HttpPut("")]
        public async Task<ActionResult<User>> AddUser(AddUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _users.Add(model.FirstName!, model.LastName!);
            return CreatedAtAction("GetUserById", new { id = user.Id }, user);
        }
    }
}
