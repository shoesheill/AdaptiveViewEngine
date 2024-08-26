using Microsoft.AspNetCore.Mvc.Razor;

namespace AdaptiveViewEngine.Helper
{
    public class DomainViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // You can populate context values here if needed
            var feature = context.ActionContext.HttpContext.Request.Query["feature"].ToString();

            // Add the feature flag to the context values
            context.Values["Feature"] = feature;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            //skip start
            // Retrieve the feature flag from context values
            var feature = context.Values["Feature"];

            // Determine view location patterns based on the feature flag
            var featurePrefix = string.IsNullOrEmpty(feature) ? "Default" : feature;

            // above is example of populated values only
            //skip end
            var domain = context.ActionContext.HttpContext.Request.Host.Host;

            // Determine the folder based on the domain
            var prefix = domain switch
            {
                "abc.com" => "abc_com",
                "abc.net" => "abc_net",
                "xyz.com" => "xyz_com",
                _ => "default" // Fallback to Default folder
            };

            // Define view locations with a default folder as fallback
            return new[]
            {
            $"/Views/{prefix}/{{1}}/{{0}}.cshtml",
            $"/Views/{prefix}/Shared/{{0}}.cshtml",
            "/Views/Shared/{{0}}.cshtml" // Fallback to Shared views
        };
        }
    }
}
