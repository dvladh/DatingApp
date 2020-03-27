using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using DatingApp.API.Utility;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dataContext;
        private readonly IHelper _helper;

        public AuthRepository(DataContext dataContext, IHelper helper)
        {
            _dataContext = dataContext;
            _helper = helper;
        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            if (!_helper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }        

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            _helper.CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();

            return user;
        }        

        public async Task<bool> UserExists(string username)
        {
            if (await _dataContext.Users.AnyAsync(u => u.Username == username))
                return true;

            return false;
        }
    }
}