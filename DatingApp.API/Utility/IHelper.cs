namespace DatingApp.API.Utility
{
    public interface IHelper
    {
         bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
         void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
    }
}