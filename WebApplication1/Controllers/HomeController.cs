using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HslCommunication.MQTT;

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
            MqttSyncClient mqttSyncClient = new MqttSyncClient( "127.0.0.1", 1883 );
            HslCommunication.OperateResult<string, string> operate = mqttSyncClient.ReadString( "StartPLC", "" );
            if (operate.IsSuccess)
            {
                return Content( operate.Content1 );
            }
            else
            {
                return Content( "通讯失败！" + operate.Message );
            }
        }

        [HttpPost]
        public ActionResult StopPlc()
        {
            MqttSyncClient mqttSyncClient = new MqttSyncClient( "127.0.0.1", 1883 );
            HslCommunication.OperateResult<string, string> operate = mqttSyncClient.ReadString( "StopPLC", "" );
            if (operate.IsSuccess)
            {
                return Content( operate.Content1 );
            }
            else
            {
                return Content( "通讯失败！" + operate.Message );
            }
        }
    }
}