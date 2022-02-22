using Domain.Models;
using Domain.Utilities;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public class TokenService : BaseService
    {
        public TokenService(IOptions<AppSettings> appSettings) {
            var settings = appSettings.Value;
            linkKey = settings.Link;
        }

        public async Task<OneTimePasswordModel> CreateLinkToken(string email)
        {
            var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.Email.Equals(email)).ConfigureAwait(false);
            var result = new OneTimePasswordModel { Success = false };
            if (user == null) return result;
            var now = DateTime.UtcNow;
            var token = new TokenModel { Created = now, UserId = user.UserId, UserToken = user.Token };
            var encryptedToken = EncryptionHelper.EncryptString(linkKey, token);
            var userToken = new OneTimePasswordModel { Success = true, Token = encryptedToken, Name = user.Name };
            return userToken;
        }

        public async Task<TokenModel> GetTokenValue(string token) {
            return EncryptionHelper.DecryptString<TokenModel>(linkKey, token);
        } 

        public async Task<int?> ValidateToken(string token)
        {
            try
            {
                var validatedToken = await this.GetTokenValue(token);
                var expiration = DateTime.UtcNow.AddMinutes(-expirationInMinutes);
                if (validatedToken.Created > expiration)
                {
                    var user = await Context.UserProfiles.SingleOrDefaultAsync(x => x.UserId == validatedToken.UserId && x.Token == validatedToken.UserToken);
                    // we flip the token so all tokens before this are no longer valid
                    if (user != null)
                    {
                        //user.Token = CreateToken();
                        //await Context.SaveChangesAsync();
                        return user.UserId;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Login Issue - {token}", e);
            }
            return null;
        }

        private int expirationInMinutes = 7 * 60 * 24;
        private string linkKey = "";
        private ILog logger = LogManager.GetLogger(nameof(UserService));
    }
}
