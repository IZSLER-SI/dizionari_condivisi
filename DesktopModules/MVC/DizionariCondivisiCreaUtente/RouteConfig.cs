using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCreaUtente
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiCreaUtente",
                "DizionariCondivisiCreaUtente",
                "{controller}/{action}",
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCreaUtente.Controllers" }
            );
        }
    }
}