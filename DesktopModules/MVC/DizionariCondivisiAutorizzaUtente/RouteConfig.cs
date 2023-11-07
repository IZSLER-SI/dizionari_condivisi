using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAutorizzaUtente
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiAutorizzaUtente", "DizionariCondivisiAutorizzaUtente", "{controller}/{action}", 
                new[] {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAutorizzaUtente.Controllers"}
            );

        }
    }
}