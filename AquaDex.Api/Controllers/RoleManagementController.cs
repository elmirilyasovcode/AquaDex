using AquaDex.Core.DTOs;
using AquaDex.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AquaDex.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class RoleManagementController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagementController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

        [HttpGet("roles")]
    public IActionResult GetAllRoles()
    {
        var roles = _roleManager.Roles
            .Select(r => new RoleDto { Id = r.Id, Name = r.Name ?? string.Empty })
            .ToList();
        return Ok(roles);
    }

        [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Role name cannot be empty.");

        var exists = await _roleManager.RoleExistsAsync(dto.Name);
        if (exists)
            return Conflict($"Role '{dto.Name}' already exists.");

        var result = await _roleManager.CreateAsync(new IdentityRole(dto.Name));
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = $"Role '{dto.Name}' created." });
    }

        [HttpDelete("roles/{roleName}")]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
                        var protectedRoles = new[] { "Angler", "VerifiedExpert", "FishingGuide", "ShopOwner", "Admin" };
        if (protectedRoles.Contains(roleName))
            return BadRequest($"'{roleName}' is a core system role and cannot be deleted.");

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return NotFound($"Role '{roleName}' does not exist.");

        await _roleManager.DeleteAsync(role);
        return Ok(new { message = $"Role '{roleName}' deleted." });
    }

        [HttpPost("assign")]
    public async Task<IActionResult> AssignRole(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound($"User with Id {dto.UserId} does not exist.");

        var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
        if (!roleExists)
            return BadRequest($"Role '{dto.RoleName}' does not exist.");

        var alreadyInRole = await _userManager.IsInRoleAsync(user, dto.RoleName);
        if (alreadyInRole)
            return Conflict($"User already has role '{dto.RoleName}'.");

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = $"Role '{dto.RoleName}' assigned to user." });
    }

        [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound($"User with Id {dto.UserId} does not exist.");

        var inRole = await _userManager.IsInRoleAsync(user, dto.RoleName);
        if (!inRole)
            return BadRequest($"User does not have role '{dto.RoleName}'.");

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = $"Role '{dto.RoleName}' removed from user." });
    }

        [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with Id {userId} does not exist.");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserRolesDto
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Roles = roles.ToList()
        });
    }
}