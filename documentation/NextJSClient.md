# Implementing the IdentityServer to Use a Next.js Client

This is documentation describing how to configure this IdentityServer project to handle incoming login and request forms from a Next.js Client.

*Assuming the default template setup has been followed from the README.*

## Table of Contents

1. [Login](#login)
2. [Registration](#registration)
3. [Next.js Configuration](#nextjs-configuration)

### Login

If you want to set up a Next.js client to authenticate for login, there are a few files that need to be configured.

First, we need to add the client into the list of clients in the `Config.cs` file:

```c#
public static IEnumerable<Client> Clients => 
    new Client[]
    {
        // ... other clients
        // Next.js Client
        new Client
        {
            ClientId = "next.js",
            ClientSecrets = { new Secret("secret".Sha256()) },

            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

            RedirectUris = { "URI of Next.js Client/api/auth/callback/credentials" },

            PostLogoutRedirectUris = { "URI of Next.js Client" },
            AllowedCorsOrigins = { "URI of Next.js Client" },

            AllowOfflineAccess = true,
            AllowedScopes = { 
                "openid",
                "profile",
                // ... other scopes
            }
        }
    }
```

Next, we need to verify that the CORS origin is a valid endpoint by adding it to the `HostingExtensions.cs` file. In the builder services section, add the following:

```c#
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "SpecificOrigin", policy =>
    {
        policy.WithOrigins("URI of Next.js Client")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

Now, we add the service to the middleware pipeline. In the same file, in the middleware section, add:

```c#
app.UseCors("SpecificOrigin");
```

*Important Note!!!* The call to `UseCors()` must be made between the calls to `UseRouting()` and `UseIdentityServer()`. Without this order, the IdentityServer will not allow the origin, and reject all incoming requests.

The IdentityServer should now be set up to properly accept and handle incoming login requests from the Next.js Client.

### Registration

In order to handle registering a new user to the database through the IdentityServer, we need to create an API endpoint on the IdentityServer to handle incoming registration requests.

First, we'll create the model for a new registration request. In the `src/IdentityServerAspNetIdentity/Models` directory, add the following `RegistrationRequest.cs` file:

```c#
using System.ComponentModel.DataAnnotations;

public class RegistrationRequest
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}
```

Next, we'll add the controller that handles the incoming requests.

In the `src/IdentityServerAspNetIdentity` directory, create the directories `api/Controllers`.

In here, we're going to add the file `RegistrationController.cs`. This file will be the endpoint that handles the incoming requests.

Add the following code to the file:

```c#
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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
            NormalizedUserName = request.UserName.ToUpper(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpper(),
            PasswordHash = HashPassword(request.Password),
        };

        // Add user to the database.
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    private string HashPassword(string password)
    {
        PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

        return passwordHasher.HashPassword(null, password);
    }
}
```

This controller takes the current database context, and using the incoming request, validates and creates a new user and adds it to the database.

*This uses the default SQLite database given with the template. If using a different database for users, the controller may need to be configured further.*

Lastly, we need to guarantee that the API endpoints accept incoming requests from the specified origins, so we need to add the following to the middleware pipeline in the `HostingExtensions.cs` file:

```c#
app.MapControllers()
    .RequireCors("SpecificOrigin");
```

Now the IdentityServer should be configured to accept incoming requests to the API registration endpoint from the Next.js client.

### Next.js Configuration

If you want to configure a Next.js client to send login and registration requests to an IdentityServer, follow the documentation [here](https://github.com/willwpearson/optim.boo/tree/main).
