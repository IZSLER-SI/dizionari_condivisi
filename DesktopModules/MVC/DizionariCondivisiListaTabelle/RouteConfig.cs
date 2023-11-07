using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute("DizionariCondivisiListaTabelle", "DizionariCondivisiListaTabelle", "{controller}/{action}", new[]
            {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle.Controllers"});          
        }
    }
}