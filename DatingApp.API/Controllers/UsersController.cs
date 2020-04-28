using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
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
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParam)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromDb = await _datingRepository.GetUser(currentUserId);

            userParam.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParam.Gender))
            {
                userParam.Gender = userFromDb.Gender == "male" ? "female" : "male";
            }

            var usersFromDb = await _datingRepository.GetUsers(userParam);

            if (usersFromDb == null)
            {
                NotFound();
            }

            var returnListUsers = _mapper.Map<IEnumerable<UserForListDto>>(usersFromDb);

            Response.AddPagination(usersFromDb.CurrentPage, usersFromDb.PageSize, usersFromDb.TotalCount, usersFromDb.TotalPages);

            return Ok(returnListUsers);
        }

        [HttpGet("{userId}", Name = "GetUser")]
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
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromDb = await _datingRepository.GetUser(userId);

            var userForUpdate = _mapper.Map(user, userFromDb);

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {userId} failed on save");
        }

        [HttpPost("{id}/Like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _datingRepository.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already liked this user");

            if (await _datingRepository.GetUser(recipientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _datingRepository.Add<Like>(like);

            if (await _datingRepository.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");
        }        
    }
}