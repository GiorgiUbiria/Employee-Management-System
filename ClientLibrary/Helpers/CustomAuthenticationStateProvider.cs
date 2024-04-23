using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseClass.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientLibrary.Helpers;

public class CustomAuthenticationStateProvider(LocalStorageService localStorageService) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var stringToken = await localStorageService.GetToken();
        if (string.IsNullOrEmpty(stringToken)) return await Task.FromResult(new AuthenticationState(anonymous));

        var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
        if (deserializeToken == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var getUserClaims = DecryptToken(deserializeToken.Token!);
        if (getUserClaims == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var claimsPrincipal = SetClaimsPrincipal(getUserClaims);
        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public static ClaimsPrincipal SetClaimsPrincipal(CustomUserClaims claims)
    {
        if (claims.Email is null) return new ClaimsPrincipal();
        return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, claims.Id!),
                new(ClaimTypes.Name, claims.Name!),
                new(ClaimTypes.Email, claims.Email!),
                new(ClaimTypes.Role, claims.Role!),
            }, "JwtAUth"
        ));
    }

    private static CustomUserClaims DecryptToken(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken)) return new CustomUserClaims();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        
        var userId = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier);
        var name = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name);
        var email = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email);
        var role = token.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Role);

        return new CustomUserClaims(userId!.Value!, name!.Value, email!.Value, role!.Value);
    }
}