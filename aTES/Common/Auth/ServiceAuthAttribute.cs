using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Common.Auth
{
    public class ServiceAuthAttribute : Attribute, IAuthenticationFilter
    {
        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var client = new HttpClient();            
            client.DefaultRequestHeaders.Authorization = context.Request.Headers.Authorization;

            // todo: get url from service config
            var response = await client.GetAsync(@"https://localhost:44342/api/auth/tryauth");
            ParrotLoginModel model = null;
            if (response.IsSuccessStatusCode)
            {
                model = await response.Content.ReadAsAsync<ParrotLoginModel>();
            }
            else
            {
                context.ErrorResult = new AuthenticationFailureResult("invalid response from auth service", context.Request);
            }

            if (model.LoginSuccessful)
            {
                IIdentity identity = new GenericIdentity(model.Name);
                IPrincipal principal = new GenericPrincipal(identity, model.Roles);
                context.Principal = principal;
            }
            else
            {
                context.ErrorResult = new AuthenticationFailureResult("no user provided", context.Request);
            }
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            // no op
            await Task.CompletedTask;
        }
    }
}
