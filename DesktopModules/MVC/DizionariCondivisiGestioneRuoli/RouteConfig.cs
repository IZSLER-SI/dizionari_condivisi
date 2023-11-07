using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiGestioneRuoli", 
                "DizionariCondivisiGestioneRuoli", 
                "{controller}/{action}", 
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli.Controllers" }
            );
        }
    }
}