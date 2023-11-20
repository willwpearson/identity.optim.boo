using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityServerAspNetIdentity.Models;
using IdentityServerAspNetIdentity.Data;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public RegistrationController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        // Validate request data.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if the username or email is already taken.
        if (await _dbContext.Users.AnyAsync(u=>u.UserName == request.UserName || u.Email == request.Email))
        {
            return Conflict("Username or email is already taken.");
        }

        // Create a new user.
        var newUser = new ApplicationUser
        {
            UserName =  request.UserName,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
        };

        // Add user to the database.
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    private string HashPassword(string password)
    {
        return password;
    }
}