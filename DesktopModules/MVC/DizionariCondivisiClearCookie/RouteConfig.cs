using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiClearCookie
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiClearCookie", "DizionariCondivisiClearCookie", "{controller}/{action}", 
                new[] {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiClearCookie.Controllers"}
            );
        }
    }
}