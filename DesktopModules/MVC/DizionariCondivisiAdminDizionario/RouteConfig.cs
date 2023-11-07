using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiAdminDizionario",
                "DizionariCondivisiAdminDizionario",
                "{controller}/{action}",
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Controllers" }
            );
        }
    }
}