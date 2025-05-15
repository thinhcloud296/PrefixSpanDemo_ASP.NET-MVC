using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization; // ĐẢM BẢO CÓ USING NÀY (BẠN ĐÃ CÓ)
using System.Web.Routing;

namespace PrefixSpanDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles); // QUAN TRỌNG: DÒNG NÀY PHẢI CÓ VÀ KHÔNG BỊ COMMENT (BẠN ĐÃ CÓ)
        }
    }
}