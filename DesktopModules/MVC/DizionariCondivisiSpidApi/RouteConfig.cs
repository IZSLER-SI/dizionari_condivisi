using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiSpidApi", "DizionariCondivisiSpidApi", "{controller}/{action}", 
                new[] {"it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Controller"}
            );
        }
    }
}