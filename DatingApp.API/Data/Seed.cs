using System.Collections.Generic;
using System.IO;
using DatingApp.API.Models;
using DatingApp.API.Utility;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        private readonly DataContext _dataContext;
        private readonly IHelper _helper;

        public Seed(DataContext dataContext, IHelper helper)
        {
            _dataContext = dataContext;           
            _helper = helper; 
        }

        public void SeedUsers()
        {
            var userData = File.ReadAllText("Data/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);

            foreach (var user in users)
            {
                byte[] passwordHash, passwordSalt;
                _helper.CreatePasswordHash("password", out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();

                _dataContext.Users.Add(user);                
            }            

            _dataContext.SaveChanges();           
        }        
    }
}