using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tasks.Controllers
{
    public class Credentials
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AccountController : Controller
    {
        readonly UserManager<IdentityUser> _userManager;
        readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        private async void AddDefaultRole()
        {
            IdentityRole task = await _roleManager.FindByNameAsync("User");
            if (task == null)
            {
                var role = new IdentityRole()
                {
                    Name = "User"
                };
                await _roleManager.CreateAsync(role);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Credentials credentials)
        {
            AddDefaultRole();
            var user = new IdentityUser { UserName = credentials.User, Email = credentials.Email };
            var result = await _userManager.CreateAsync(user, credentials.Password);
            await _userManager.AddToRoleAsync(user, "User");
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            else
            {
                return Ok(CreateToken(user));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] Credentials credentials)
        {
            var result = await _signInManager.PasswordSignInAsync(credentials.User, credentials.Password, false, false);
            if (!result.Succeeded)
            {
                return BadRequest();
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(credentials.Email);

                return Ok(CreateToken(user));
            }
        }

        private async Task<string> CreateToken(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claim = new List<Claim>();
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id);
            }

            foreach (var role in roles)
            {
                bool roleNeeded = true;
                foreach (var c in claim)
                {
                    if (c.Type == ClaimTypes.Role && c.Value == role)
                    {
                        roleNeeded = false; break;
                    }
                }
                if (roleNeeded)
                {
                    claim.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("8D7A9F3B2E6C5A1D9B4FAC7E0F8D3B1A6E9C2A5F8B3D6E4C7A9D3F6B2E5A1D8F9C6B4A7E0F3D2B5A8\r\n"));
            var signingCredential = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(signingCredentials: signingCredential, claims: claim);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [HttpGet("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            var result = await _userManager.GetRolesAsync(user);
            if (result == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(result.ToList());
            }
        }

        [HttpPost("SetRole")]
        public async Task<IActionResult> SetRoles(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (!string.IsNullOrEmpty(user.Email))
            {
                var roles = await _roleManager.FindByNameAsync(role);

                if (!string.IsNullOrEmpty(roles.Name))
                {
                    await _userManager.AddToRoleAsync(user, role);
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
