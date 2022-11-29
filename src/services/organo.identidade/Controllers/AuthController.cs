using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Organo.Auth.Extensions;
using Organo.Auth.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace organo.identidade.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SecretSettings _secretSettings;

        public AuthController(UserManager<IdentityUser> userManager, IOptions<SecretSettings> secretSettings, SignInManager<IdentityUser> signInManager)
        {
            if (secretSettings is null) throw new ArgumentNullException(nameof(secretSettings));

            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _secretSettings = secretSettings.Value;
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, true);

            if (result.Succeeded)
                return Ok(await GetJwt(model.Email));

            return BadRequest();
        }

        [Authorize(Policy = "admin")]
        [HttpGet]
        public string Teste()
        {
            return "autorizado";
        }

        [HttpGet("teste")]
        public async Task<IActionResult> Testes()
        {
            var userTeste = await _userManager.FindByIdAsync("57b1545a-9040-4263-8d53-bafb82eae6b8");
            var addClaim = await _userManager.AddClaimAsync(userTeste, new Claim("adm", "del"));
            var claimsTeste = await _userManager.GetUsersForClaimAsync(new Claim("adm", "del"));
            return Ok();
        }

        [HttpPost("new-account")]
        public async Task<ActionResult> Registrar(UserRegister user)
        {
            if (!ModelState.IsValid) return BadRequest();

            var identityUser = new IdentityUser
            {
                UserName = user.Email,
                Email = user.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, user.Password);

            if (result.Succeeded)
            {
                if (user.Email == "vini.souza00@gmail.com")
                    await _userManager.AddClaimAsync(identityUser, new Claim("adm", "del"));

                return Ok(await GetJwt(identityUser.Email));
            }

            string error = "";
            for (int i = 0; i < result.Errors.Count(); i++)
            {
                if (i == result.Errors.Count() - 1)
                    error += result.Errors.ToList()[i].Description;

                else
                    error += result.Errors.ToList()[i].Description + " - ";
            }

            return BadRequest(error);
        }

        private async Task<UserLoginResponse> GetJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var identityClaims = await GetUserClaims(claims, user);
            var encodedToken = WriteToken(identityClaims);
            return GetResponseToken(encodedToken, user, claims);
        }

        private async Task<ClaimsIdentity> GetUserClaims(ICollection<Claim> claims, IdentityUser user)
        {
            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);
            return identityClaims;
        }

        private string WriteToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretSettings.Secret);

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _secretSettings.Issuer,
                Audience = _secretSettings.Audience,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_secretSettings.ExpiresIn),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        private UserLoginResponse GetResponseToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            return new UserLoginResponse
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_secretSettings.ExpiresIn).TotalSeconds,
                UsuarioToken = new UserToken
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Claims = claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                }
            };
        }
    }
}