﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MotoHub.Filters
{
    public class AdminAuthorizationFilter : Attribute, IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public AdminAuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userIdentity = context.HttpContext.User.Identity as ClaimsIdentity;

            var isAdmin = userIdentity?.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            if (!isAdmin.GetValueOrDefault())
            {
                context.Result = new ForbidResult();
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenValid = tokenHandler.CanReadToken(token);
            if (!tokenValid)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var jwtKey = _configuration["JwtKey"] ?? throw new InvalidOperationException("JwtKey is not set in the configuration.");

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
