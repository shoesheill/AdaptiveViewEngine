namespace AdaptiveViewEngine.Middleware
{
    public class DomainMiddleware
    {
        private readonly RequestDelegate _next;

        public DomainMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var domain = context.Request.Host.Host;

            // Perform domain-specific actions
            switch (domain)
            {
                case "abc.com":
                    // Set a domain-specific header or context value
                    context.Response.Headers["X-Domain-Name"] = "ABC Domain";
                    // Example: Configure a domain-specific feature
                    context.Items["DomainFeature"] = "FeatureForAbc";
                    break;

                case "xyz.com":
                    // Set a different domain-specific header or context value
                    context.Response.Headers["X-Domain-Name"] = "XYZ Domain";
                    // Example: Configure a domain-specific feature
                    context.Items["DomainFeature"] = "FeatureForXyz";
                    break;

                default:
                    // Default behavior for unknown domains
                    context.Response.Headers["X-Domain-Name"] = "Default Domain";
                    break;
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
