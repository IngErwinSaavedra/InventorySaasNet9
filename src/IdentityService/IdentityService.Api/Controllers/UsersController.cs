using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

/*Crud de usuarios
 consulta usuarios activos/inactivos
 actualizacion de datos basicos de usuarios
 activar/desactivar usuarios
*/

public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _userManager.Users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.EmailConfirmed,
            u.LockoutEnabled,
            u.LockoutEnd
        }).ToList();
        
        return Ok(users);
    }
    
    // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            // Opcional: incluir roles
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.EmailConfirmed,
                Roles = roles
            });
        }

        // PUT api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            user.UserName = model.UserName ?? user.UserName;
            user.Email = model.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Usuario actualizado correctamente" });
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return NoContent(); // 204 sin contenido
        }

        // POST api/users/{id}/roles
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AddRoles(string id, [FromBody] RolesDto model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            var result = await _userManager.AddToRolesAsync(user, model.Roles);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Roles asignados correctamente" });
        }

        // DELETE api/users/{id}/roles
        [HttpDelete("{id}/roles")]
        public async Task<IActionResult> RemoveRoles(string id, [FromBody] RolesDto model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            var result = await _userManager.RemoveFromRolesAsync(user, model.Roles);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Message = "Roles removidos correctamente" });
        }
    }

    // DTOs
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }

    public class RolesDto
    {
        public string[] Roles { get; set; } = new string[0];
    }
}