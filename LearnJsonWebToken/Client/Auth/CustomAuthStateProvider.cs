using Blazored.LocalStorage;
using Blazored.SessionStorage;
using LearnJsonWebToken.Client.Utility;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LearnJsonWebToken.Client.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;

        public CustomAuthStateProvider(ILocalStorageService sessionStorage)
        {
            this.localStorage = sessionStorage;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await this.localStorage.GetItemAsStringAsync("access_token");
            if (accessToken != null && accessToken.Length > 0)
            {
                var accessTokenHandler = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var claimsList = accessTokenHandler.Claims.ToList();
                var expiryDateMs = claimsList[4].Value;
                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiryDateMs));
                if (expiryDate.DateTime > DateTime.Now)
                {
                    var identity = new ClaimsIdentity("Bearer");
                    identity.AddClaim(new Claim(ClaimTypes.Email, claimsList[1].Value));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claimsList[2].Value));
                    var user = new ClaimsPrincipal(identity);
                    var authState = new AuthenticationState(user);
                    SetUserState(true, claimsList[1].Value, claimsList[2].Value);
                    NotifyAuthenticationStateChanged(Task.FromResult(authState));
                    return await Task.FromResult(authState);
                }
                else
                {
                    SetUserState(false);
                    await this.localStorage.RemoveItemAsync("access_token");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
                    return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
                }
            }
            SetUserState(false);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
        }

        private void SetUserState(bool isAuthenticated, string? emailId = null, string? Uid = null)
        {
            UserState.IsAuthenticated = isAuthenticated;
            UserState.Email = emailId;
            UserState.Uid = Uid;
        }
    }
}
