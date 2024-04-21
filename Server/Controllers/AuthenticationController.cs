using BaseClass.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(Register user)
    {
        if (user == null) return BadRequest("Model is empty.");
        var result = await accountInterface.CreateAsync(user);
        return Ok(result);
    }
    
}