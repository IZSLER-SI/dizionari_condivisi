using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidProfilo
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiSpidProfilo",
                "DizionariCondivisiSpidProfilo",
                "{controller}/{action}",
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidProfilo.Controllers" }
            );
        }
    }
}