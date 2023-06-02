using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Lab.Todo.BLL.Services.TokenHandler.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lab.Todo.BLL.Services.TokenHandler
{
    public class SecurityTokenHandlerAdapter : ISecurityTokenHandler
    {
        private readonly SecurityTokenHandler _tokenHandler;
        private readonly JwtOptions _options;

        public SecurityTokenHandlerAdapter(SecurityTokenHandler tokenHandler, IOptions<JwtOptions> options)
        {
            _tokenHandler = tokenHandler;
            _options = options.Value;
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            if (claims is null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretValue));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_options.TokenExpirationTime.GetValueOrDefault()),
                SigningCredentials = new SigningCredentials(securityKey, _options.EncryptionAlgorithms.FirstOrDefault())
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);

            return _tokenHandler.WriteToken(token);
        }
    }
}