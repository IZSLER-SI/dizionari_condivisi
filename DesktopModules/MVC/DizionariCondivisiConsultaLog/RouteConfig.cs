using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiConsultaLog
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiConsultaLog",
                "DizionariCondivisiConsultaLog",
                "{controller}/{action}",
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiConsultaLog.Controllers" }
            );
        }
    }
}
