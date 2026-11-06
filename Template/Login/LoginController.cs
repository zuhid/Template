using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Zuhid.Template.Login;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class LoginController(AppSetting appSetting) : ControllerBase
{
    // Hard-coded: every login is authenticated as this user until a real user store exists.
    private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string Username = "admin@test.com";

    [HttpPost]
    public ActionResult<LoginResponse> Login()
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(System.IO.File.ReadAllText(appSetting.Jwt.PrivateKeyPath));
        var expires = DateTime.UtcNow.AddMinutes(appSetting.Jwt.ExpiryInMinutes);
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = appSetting.Jwt.Issuer,
            Audience = appSetting.Jwt.Audience,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString()),
                new Claim("nameid", UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, Username),
                new Claim(ClaimTypes.Role, "Admin"),
            ]),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256),
        });

        return new LoginResponse { Token = token, Expires = expires };
    }
}
