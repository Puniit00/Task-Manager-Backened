using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Tasks.Controllers
{
    public class Roles
    {
        public string roleName { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class AdministrationController : ControllerBase
    {

        private RoleManager<IdentityRole> _roleManager;
        private readonly UserDbContext _context;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserDbContext context)
        {
            this._roleManager = roleManager;
            this._context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Roles roles)
        {
            var role = new IdentityRole()
            {
                Name = roles.roleName
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.ToString());
        }

        [HttpGet]
        [Route("GetRoles")]
        public  IEnumerable<IdentityRole> GetRoles()
        {
            return  _roleManager.Roles;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRoles(Roles roles, string roleName)
        {
            IdentityRole role = await _roleManager.FindByNameAsync(roles.roleName);
            if (role == null)
            {
                return BadRequest();
            }
            else
            {
                role.Name = roleName;
                await _roleManager.UpdateAsync(role);
                return Ok(roleName);
            }
        }
    }
}
