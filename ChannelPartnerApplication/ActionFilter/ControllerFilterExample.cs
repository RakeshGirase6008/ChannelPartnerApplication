using ChannelPartnerApplication.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace ChannelPartnerApplication.ActionFilter
{
    public class ControllerFilterExample : IActionFilter
    {
        #region Fields

        private readonly ChannelPartnerManagementContext _context;
        private readonly ClassBookManagementContext _classcontext;

        #endregion

        #region Ctor
        public ControllerFilterExample(ChannelPartnerManagementContext context,
            ClassBookManagementContext classcontext)
        {
            this._context = context;
            this._classcontext = classcontext;
        }

        #endregion

        #region Method
        public void OnActionExecuting(ActionExecutingContext context)
        {
            #region Secret_Key

            var secretKey = _context.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            StringValues secretKeyToken;
            var status = context.HttpContext.Request.Headers.TryGetValue("Secret_Key", out secretKeyToken);
            var mySringSecretKey = secretKeyToken.ToString();
            if (secretKey == null || mySringSecretKey != secretKey.Value.ToString() || status == false)
            {
                var validationError = new
                {
                    Message = "Secret_Key is not Valid"
                };
                context.Result = new UnauthorizedObjectResult(validationError);
                return;
            }

            #endregion

            #region AuthorizeTokenKey

            StringValues authorizationToken;
            var status1 = context.HttpContext.Request.Headers.TryGetValue("AuthorizeTokenKey", out authorizationToken);
            var mySringauthorizationToken = authorizationToken.ToString();
            if (mySringauthorizationToken != "Default")
            {
                var authorizationTokenKey = _classcontext.Users.Where(x => x.AuthorizeTokenKey == mySringauthorizationToken).AsNoTracking();
                if (!authorizationTokenKey.Any() || status1 == false)
                {
                    var validationError = new
                    {
                        Message = "AuthorizeTokenKey is not Valid"
                    };
                    context.Result = new UnauthorizedObjectResult(validationError);
                    return;
                }
            }

            #endregion
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //our code after action executes
        }

        #endregion
    }
}
