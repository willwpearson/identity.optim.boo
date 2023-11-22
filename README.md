# identity.optim.boo

IdentityServer for optim.boo

This project is setup using a default template from Duende, and configured to accept the Next.js client running on optim.boo.

## Setup the Default Template

Here are the steps followed to set up the default template:

First, install dependencies. The project is setup using .NET 6 and ASP.NET Core Identity, so we need the [.NET SDK](https://dotnet.microsoft.com/en-us/) installed.

Next, we need the Duende templates. Run this command:

```bash
dotnet new install Duende.IdentityServer.Templates
```

With the templates installed, we can use the ASP.NET Core Identity template. In the directory of your solution, create the new solution:

```bash
dotnet new sln
```

Make a directory `src` and navigate to it in the terminal, then create the project using the template and add it to the solution:

```bash
# in the src directory
dotnet new isaspid -n IdentityServerAspNetIdentity
dotnet sln add IdentityServerAspNetIdentity/IdentityServerAspNetIdentity.csproj
```

The CLI will prompt if you want to seed the data. If you want to use the default SQLite database, choose Yes. Otherwise, choose No. If you choose No, you will have to update the "ConnectionStrings" in the `applicationsettings.json` file to point to the database you are using.

That's it! The IdentityServer template is set up and ready to be configured for the clients you need to authenticate.

See the documentation [here](documentation) to configure specific clients.
