using HslCommunication.Enthernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index( )
        {
            return View( );
        }

        public ActionResult About( )
        {
            ViewBag.Message = "Your application description page.";

            return View( );
        }

        public ActionResult Contact( )
        {
            ViewBag.Message = "Your contact page.";

            return View( );
        }

        [HttpPost]
        public ActionResult StartPlc( )
        {
            // 启动PLC的操作
            NetSimplifyClient simplifyClient = new NetSimplifyClient( "127.0.0.1", 23457 );
            HslCommunication.OperateResult<string> operate = simplifyClient.ReadFromServer( 1, "" );
            if (operate.IsSuccess)
            {
                return Content( operate.Content );
            }
            else
            {
                return Content( "启动失败！" + operate.Message );
            }
        }

        [HttpPost]
        public ActionResult StopPlc()
        {
            NetSimplifyClient simplifyClient = new NetSimplifyClient( "127.0.0.1", 23457 );
            HslCommunication.OperateResult<string> operate = simplifyClient.ReadFromServer( 2, "" );
            if (operate.IsSuccess)
            {
                return Content( operate.Content );
            }
            else
            {
                return Content( "停止失败！" + operate.Message );
            }
        }
    }
}