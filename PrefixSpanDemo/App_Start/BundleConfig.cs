using System.Web;
using System.Web.Optimization;

namespace PrefixSpanDemo
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Bundle cho jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js")); // Đảm bảo bạn có file jQuery trong /Scripts, ví dụ jquery-3.7.1.js

            // Bundle cho jQuery Validation mà bạn đang cố render
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*", // Dấu * sẽ bao gồm jquery.validate.js, jquery.validate.unobtrusive.js
                        "~/Scripts/jquery.validate.unobtrusive.min.js")); // Có thể chỉ định rõ nếu muốn

            // Bundle cho Bootstrap JS
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.min.js")); // Hoặc bootstrap.js nếu bạn quản lý Popper riêng

            // Bundle cho CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css", // Hoặc bootstrap.css
                      "~/Content/site.css")); // File CSS tùy chỉnh của bạn
        }
    }
}