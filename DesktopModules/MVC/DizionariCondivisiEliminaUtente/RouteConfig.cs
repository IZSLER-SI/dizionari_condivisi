using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiEliminaUtente
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiEliminaUtente", "DizionariCondivisiEliminaUtente", "{controller}/{action}", 
                new[] {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiEliminaUtente.Controllers"}
            );

        }
    }
}