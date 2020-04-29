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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        public IMapper _mapper { get; }
        public MessagesController(IDatingRepository datingRepository, IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _datingRepository.GetMessage(id);

            if (message != null)
                return Ok(message);

            return NotFound();
        }

        [HttpPost()]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipinet = await _datingRepository.GetUser(messageForCreationDto.RecipientId);

            if (recipinet == null)
                return BadRequest("Could not find user");

            var message = _mapper.Map<Message>(messageForCreationDto);

            _datingRepository.Add(message);

            if (await _datingRepository.SaveAll())
            {
                var messageForReturn = _mapper.Map<MessageForCreationDto>(message);
                return CreatedAtRoute("GetMessage", new { userId = userId, id = message.Id }, messageForReturn);
            }

            throw new System.Exception("Created the message failed on save");
        }
    }
}