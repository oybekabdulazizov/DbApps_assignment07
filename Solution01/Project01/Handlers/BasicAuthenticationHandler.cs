using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Project01.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
            // IDbService _idbService
            ) : base(options, logger, encoder, clock)
        { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization")) 
            {
                return AuthenticateResult.Fail("Missing authentication header!");
            }

            // Authentication: Basic airngeoi348tf==
            var authenticationHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authenticationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(":"); // ["oybek123", "asd123"]

            if (credentials.Length !=2) 
            {
                return AuthenticateResult.Fail("Incorrect authentication header!");
            }

            // here we need to check if the password is correct in the database 

            // if everything is correct
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "oybek123"), 
                new Claim(ClaimTypes.Role, "admin"), 
                new Claim(ClaimTypes.Role, "student")
            };

            // this is something like a passport
            var identity = new ClaimsIdentity(claims, Scheme.Name);

            // this is something like a wallet. We can have multiple identities within a single wallet 
            // (e.g. sometimes within a single web app, we can use multiple authentication/authorization)
            var principal = new ClaimsPrincipal(identity); 

            // this is a ticket which allow a user to use a specific service
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
