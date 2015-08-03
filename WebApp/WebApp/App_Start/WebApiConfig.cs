

using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using WebApp.Models;
using Repository.Models;

namespace WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

			builder.EntitySet<Product>("Product");
			builder.EntitySet<OrderLine>("OrderLine");
			builder.EntitySet<Order>("Order");
			builder.EntitySet<Occupation>("Occupation");
			builder.EntitySet<Client>("Client");
            config.MapODataServiceRoute(
				routeName: "ODataRoute",
                routePrefix: "FabricsOdata",
                model: builder.GetEdmModel());
        }
    }
}