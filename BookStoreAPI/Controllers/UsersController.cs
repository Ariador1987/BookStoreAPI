using AutoMapper;
using BookStoreAPI.Contracts;
using BookStoreAPI.Data.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly IConfiguration _config;

        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ILoggerService loggerService,
            IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _loggerService = loggerService;
            _config = config;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var username = userDTO.Username;
                var password = userDTO.Password;
                _loggerService.LogInfo($"{location}: {username} Login Attempt");
                //var id = userDTO.Id;
                //var email = userDTO.Email;
                ////SignInResult result;

                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
                if (result.Succeeded)
                {
                    _loggerService.LogInfo($"{location}: {username} Successfully Authenticated");
                    var user = await _userManager.FindByNameAsync(username);
                    // nakon što nažemo usera
                    var tokenString = await GenerateJSONWebToken(user);
                    //var userToReturn = _mapper.Map<UserViewDTO>(user);
                    
                    return StatusCode(200, new
                    {
                        token = tokenString
                    });
                } else
                {
                    _loggerService.LogWarn($"{ModelState}");
                }

                _loggerService.LogWarn($"{location}: {username} Not Authenticated");
                return StatusCode(401, userDTO);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}.");
            }
        }

        // PRIVATES
        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, new Guid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),

            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(x => new Claim(ClaimsIdentity.DefaultRoleClaimType, x)));

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                null,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
