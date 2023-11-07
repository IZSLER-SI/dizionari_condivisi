using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpid
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiSpid", "DizionariCondivisiSpid", "{controller}/{action}", 
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpid.Controllers" }
            );
        }
    }
}