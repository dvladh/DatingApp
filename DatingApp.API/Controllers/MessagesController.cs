using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet()]
        public async Task<ActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            var messagesFromDb = await _datingRepository.GetMessagesForUser(messageParams);

            if (messagesFromDb == null)
                NotFound();

            var messagesForReturn = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromDb);

            Response.AddPagination(messagesFromDb.CurrentPage, messagesFromDb.PageSize, messagesFromDb.TotalCount, messagesFromDb.TotalPages);

            return Ok(messagesForReturn);
        }



        [HttpPost()]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _datingRepository.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;
        
            var recipinet = await _datingRepository.GetUser(messageForCreationDto.RecipientId);

            if (recipinet == null)
                return BadRequest("Could not find user");

            var message = _mapper.Map<Message>(messageForCreationDto);
            
            _datingRepository.Add(message);

            if (await _datingRepository.SaveAll())
            {
                var messageForReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId = userId, id = message.Id }, messageForReturn);
            }

            throw new System.Exception("Created the message failed on save");
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromDb = await _datingRepository.GetMssageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromDb);

            return Ok(messageThread);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageForUpdate = await _datingRepository.GetMessage(id);

            if(messageForUpdate.SenderId == userId) {
                messageForUpdate.SenderDeleted = true;
            }

            if(messageForUpdate.RecipientId == userId) {
                messageForUpdate.RecipientDeleted = true;
            }

            if(messageForUpdate.SenderDeleted && messageForUpdate.RecipientDeleted)
                _datingRepository.Delete(messageForUpdate);
           
            if(await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id/read}")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageForUpdate = await _datingRepository.GetMessage(id);

            if(messageForUpdate.RecipientId != userId) {
                return Unauthorized();
            }

            messageForUpdate.IsRead = true;
            messageForUpdate.DateRead = DateTime.Now;

            await _datingRepository.SaveAll();

            return NoContent();
        }
    }
}