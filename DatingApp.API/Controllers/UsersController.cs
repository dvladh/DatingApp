using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        public IMapper _mapper { get; }

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var usersFromDb = await _datingRepository.GetUsers();

            if (usersFromDb == null)
            {
                NotFound();
            }

            var returnListUsers = _mapper.Map<IEnumerable<UserForListDto>>(usersFromDb);

            return Ok(returnListUsers);
        }

        [HttpGet("{userId}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var userFromDb = await _datingRepository.GetUser(userId);

            if (userFromDb == null)
            {
                return NotFound();
            }

            var returnListUser = _mapper.Map<UserForDetailedDto>(userFromDb);

            return Ok(returnListUser);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserForUpdateDto user)
        {
           if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
              return Unauthorized();

            var userFromDb = await _datingRepository.GetUser(userId); 

            var userForUpdate = _mapper.Map(user, userFromDb);           
            
            if(await _datingRepository.SaveAll())
            {
                return NoContent();   
            }

            throw new Exception($"Updating user {userId} failed on save");            
        }
    }
}