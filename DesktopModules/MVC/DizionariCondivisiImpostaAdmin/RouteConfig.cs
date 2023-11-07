using DotNetNuke.Web.Mvc.Routing;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiImpostaAdmin
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute(
                "DizionariCondivisiImpostaAdmin",
                "DizionariCondivisiImpostaAdmin", 
                "{controller}/{action}", 
                new[] { "it.invisiblefarm.dizionaricondivisi.DizionariCondivisiImpostaAdmin.Controllers" }
            );
        }
    }
}