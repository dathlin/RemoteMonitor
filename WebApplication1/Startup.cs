
using Microsoft.Owin;
using Owin;
using WebApplication1;

[assembly: OwinStartup( typeof( WebApplication1.Startup ) )]


namespace WebApplication1
{
    public class Startup
    {
        public void Configuration( IAppBuilder app )
        {
            app.MapSignalR( );
        }
    }
}