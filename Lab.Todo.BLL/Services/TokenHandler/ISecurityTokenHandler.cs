using System.Collections.Generic;
using System.Security.Claims;

namespace Lab.Todo.BLL.Services.TokenHandler
{
    public interface ISecurityTokenHandler
    {
        string CreateToken(IEnumerable<Claim> claims);
    }
}
