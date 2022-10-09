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
        /// Try to authorize user via basic authorization containing login only
        /// </summary>
        /// <returns></returns>
        // GET api/auth/tryauth
        [HttpGet]
        public ParrotLoginModel TryAuth()
        {
            var authVal = Request.Headers.Authorization.Parameter;

            if (Request.Headers.Authorization?.Scheme == "Basic")
            {
                if (TryParseLogin(authVal, out string login))
                {
                    return new ParrotLoginModel
                    {
                        Name = login,
                        Roles = new string[] { },
                        LoginSuccessful = true,
                    };
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

        private bool TryParseLogin(string authVal, out string login)
        {
            login = null;
            if (!String.IsNullOrEmpty(authVal))
            {
                try
                {
                    var base64bytes = Convert.FromBase64String(authVal);
                    var inputStr = System.Text.Encoding.UTF8.GetString(base64bytes);
                    //todo: parse login out
                    login = inputStr;                    
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
