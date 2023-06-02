using System;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.TokenHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace Lab.Todo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ISecurityTokenHandler _securityTokenHandler;

        public AccountController(ISecurityTokenHandler securityTokenHandler)
        {
            _securityTokenHandler = securityTokenHandler;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest authenticateRequest)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            if (IsBadUser(authenticateRequest)) { return Unauthorized(); }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, authenticateRequest.Username)
            };

            var token = _securityTokenHandler.CreateToken(claims);

            var response = new AuthenticateResponse
            {
                Username = authenticateRequest.Username,
                Token = token
            };

            return Ok(response);
        }

        private static bool IsBadUser(AuthenticateRequest authenticateRequest) => 
            string.Equals(authenticateRequest.Username.Trim(), "BadUser", StringComparison.OrdinalIgnoreCase);
    }
}