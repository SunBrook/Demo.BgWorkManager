using Manager.WebApi.Helper;
using System.Net.WebSockets;

namespace Manager.WebApi.Middleware
{
    public class WebSocketMiddleware2
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware2(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path == "/ws")
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    try
                    {
                        var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
                        await new WebSocketHelper().WebSocketReceive(socket);
                    }
                    catch (Exception ex)
                    {
                        await httpContext.Response.WriteAsync(ex.Message);
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = 404;
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
