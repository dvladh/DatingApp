using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
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

        [HttpGet("{userId}")]
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
    }
}