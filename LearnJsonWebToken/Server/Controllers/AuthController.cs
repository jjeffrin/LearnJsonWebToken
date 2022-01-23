using LearnJsonWebToken.Server.Services;
using LearnJsonWebToken.Shared.DTO;
using LearnJsonWebToken.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LearnJsonWebToken.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IConfiguration configuration;
        public AuthController(AppDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<bool>> Register(UserDto user)
        {
            if (this.dbContext.Users.Any(x => x.Email.Equals(user.Email)))
            {
                return BadRequest("Email ID is already registered");
            }

            try
            {
                // create new user object
                var newUser = new User();
                // set GUID to the new user
                newUser.Id = new Guid();
                // set email to the new user
                newUser.Email = user.Email;

                // compute hash, salt and set it to the new user
                using (var hmac = new HMACSHA512())
                {
                    newUser.PasswordSalt = hmac.Key;
                    newUser.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
                }

                await this.dbContext.Users.AddAsync(newUser);
                await this.dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }            

            return Ok(true);
        }

        [HttpPost("login")]
        public ActionResult<string> Login(UserDto user)
        {
            try
            {
                var userInDB = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(user.Email));
                if (userInDB == null)
                {
                    return NotFound("User does not exist");
                }
                using (var hmac = new HMACSHA512(userInDB.PasswordSalt))
                {
                    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
                    if (computedHash.SequenceEqual(userInDB.PasswordHash))
                    {
                        // then credentials match! return a token!
                        string token = CreateJWTokenForUser(userInDB);
                        return token;
                    }
                }

                return Unauthorized("Incorrect password");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }            
        }

        private string CreateJWTokenForUser(User user)
        {
            var userClaims = new List<Claim>();
            userClaims.Add(new Claim(ClaimTypes.Name, user.Name));
            userClaims.Add(new Claim(ClaimTypes.Email, user.Email));
            userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            userClaims.Add(new Claim(ClaimTypes.GivenName, user.DisplayName));
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration.GetSection("JWT:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: userClaims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwToken;
        }
    }    
}
