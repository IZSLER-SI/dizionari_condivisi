using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCaricamentoDizionario
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiCaricamentoDizionario", "DizionariCondivisiCaricamentoDizionario", "{controller}/{action}", 
                new[] {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCaricamentoDizionario.Controllers"}
            );
        }
    }
}