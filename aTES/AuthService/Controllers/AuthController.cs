using AuthService.Db;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AuthService.Controllers
{
    public class AuthController : ApiController
    {
        /// <summary>
        /// Try to authorize user via basic authorization containing email only
        /// </summary>
        /// <returns></returns>
        // GET api/auth/tryauth
        [HttpGet]
        public ParrotLoginModel TryAuth()
        {            
            if (Request.Headers.Authorization?.Scheme == "Basic")
            {
                var authVal = Request.Headers.Authorization.Parameter;
                if (TryParseEmail(authVal, out string email) && TryGetParrot(email, out ParrotLoginModel parrot))
                {
                    parrot.LoginSuccessful = true;
                    return parrot;
                }
                else
                {
                    return new ParrotLoginModel
                    {
                        LoginSuccessful = false,
                    };
                }
            }
            else
            {
                return new ParrotLoginModel
                {
                    LoginSuccessful = false,
                };
            }
        }

        private bool TryGetParrot(string email, out ParrotLoginModel parrot)
        {
            using(var db = new AuthDB())
            {
                var query = from p in db.Parrots
                            where p.Email == email
                            select new ParrotLoginModel
                            {
                                LoginSuccessful = true,
                                Name = p.Name,
                                PublicId = p.PublicId,
                                Roles = new[] { p.Role.Name }
                            };
                parrot = query.FirstOrDefault();
                return parrot != null;
            }
        }

        private bool TryParseEmail(string authVal, out string email)
        {
            email = null;
            if (!String.IsNullOrEmpty(authVal))
            {
                try
                {
                    var base64bytes = Convert.FromBase64String(authVal);
                    var inputStr = System.Text.Encoding.UTF8.GetString(base64bytes);
                    //todo: email and password
                    email = inputStr.Split(':').First();                    
                    return true;
                }
                catch
                {
                    // todo: log?
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
