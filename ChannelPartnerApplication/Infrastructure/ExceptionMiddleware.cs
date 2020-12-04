using ChannelPartnerApplication.DataContext;
using ChannelPartnerApplication.Models.ResponseModel;
using ChannelPartnerApplication.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChannelPartnerApplication.Infrastructure
{
    public class ExceptionMiddleware
    {

        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Method
        public async Task Invoke(HttpContext context, LogsService logsService, ChannelPartnerManagementContext channelPartnerManagementContext)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, logsService, channelPartnerManagementContext);
            }
        }
        private Task HandleExceptionAsync(HttpContext httpContext, Exception exception, LogsService _logsService, ChannelPartnerManagementContext _channelPartnerManagementContext)
        {
            #region Add Logs

            var endpoint = httpContext.GetEndpoint();
            int userId = 0;
            string controllerName = string.Empty;
            if (endpoint != null)
            {
                StringValues secretKeyToken;
                httpContext.Request.Headers.TryGetValue("AuthorizeTokenKey", out secretKeyToken).ToString();
                var authorizationTokenKey = secretKeyToken.ToString();
                var singleUser = _channelPartnerManagementContext.Users.Where(x => x.AuthorizeTokenKey == authorizationTokenKey).AsNoTracking();
                if (singleUser.Any())
                {
                    userId = singleUser.FirstOrDefault().Id;
                }
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                    controllerName = controllerActionDescriptor.ControllerName;
            }
            _logsService.InsertLogs(controllerName, exception, httpContext.Request.Path.Value, userId);

            #endregion

            #region Exception Response

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return httpContext.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = exception?.Message
            }.ToString());

            #endregion
        }
        #endregion
    }
}