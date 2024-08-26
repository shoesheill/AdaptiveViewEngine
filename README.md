# AdaptiveViewEngine

## Overview

**AdaptiveViewEngine** is a .NET Core 8 web application designed to dynamically serve different Razor views based on the request host URL. It includes middleware to handle custom host-specific headers or content and a view location expander to manage dynamic view resolution.

## Features

- **Dynamic View Locations**: Serves Razor views from different folders based on the request host URL.
- **Custom Headers and Content**: Adds custom headers or context values based on the request host.
- **Flexible View Management**: Organizes views into domain-specific folders, with fallback options.
- **Fallback Mechanism**: Provides default or shared views if no specific folder is matched.

## Implementation

### DomainViewLocationExpander

The `DomainViewLocationExpander` class customizes Razor view locations based on the request host.

#### Key Methods

- **`PopulateValues(ViewLocationExpanderContext context)`**:

  - Populates context values used to determine view locations. For this implementation, it extracts optional feature flags from the query string.

- **`ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)`**:
  - Determines view location patterns based on the request host and feature flags. Constructs view paths dynamically.

#### `DomainViewLocationExpander` Implementation

```csharp
namespace AdaptiveViewEngine
{
    using Microsoft.AspNetCore.Mvc.Razor;
    using System.Collections.Generic;

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
            // Retrieve the feature flag from context values
            var feature = context.Values["Feature"];

            // Determine view location patterns based on the feature flag
            var featurePrefix = string.IsNullOrEmpty(feature) ? "Default" : feature;

            var domain = context.ActionContext.HttpContext.Request.Host.Host;

            // Determine the folder based on the domain
            var prefix = domain switch
            {
                "abc.com" => "abc_com",
                "abc.net" => "abc_net",
                "xyz.com" => "xyz_com",
                _ => "localhost" // Fallback to Default folder
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
```

### DomainMiddleware

The `DomainMiddleware` class adds custom headers or context values based on the request host.

#### `DomainMiddleware` Implementation

```csharp
namespace AdaptiveViewEngine
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
                    // Configure a domain-specific feature
                    context.Items["DomainFeature"] = "FeatureForAbc";
                    break;

                case "xyz.com":
                    // Set a different domain-specific header or context value
                    context.Response.Headers["X-Domain-Name"] = "XYZ Domain";
                    // Configure a domain-specific feature
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
```

### Setup Instructions

1. **Configure Services and Middleware in `Program.cs`**:

   Add the `DomainViewLocationExpander` and `DomainMiddleware` to `Program.cs`:

   ```csharp
   using Microsoft.AspNetCore.Builder;
   using Microsoft.AspNetCore.Hosting;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Hosting;
   using AdaptiveViewEngine;

   var builder = WebApplication.CreateBuilder(args);

   // Add services to the container
   builder.Services.AddControllersWithViews()
       .AddRazorOptions(options =>
       {
           options.ViewLocationExpanders.Add(new DomainViewLocationExpander());
       });

   var app = builder.Build();

   // Configure the HTTP request pipeline
   if (app.Environment.IsDevelopment())
   {
       app.UseDeveloperExceptionPage();
   }
   else
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }

   app.UseHttpsRedirection();
   app.UseStaticFiles();

   app.UseRouting();

   app.UseMiddleware<DomainMiddleware>(); // Add DomainMiddleware here

   app.UseAuthorization();

   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");

   app.Run();
   ```

2. **Create Folder Structure for Views**:

   Ensure the following folders exist in your `Views` directory:

   - `/Views/Default/`
     - Place views specific to `default` here.
   - `/Views/abc_com/`
     - Place views specific to `abc.com` here.
     - `/Views/abc_net/`
     - Place views specific to `abc.net` here.
   - `/Views/xyz_com/`
     - Place views specific to `xyz.com` here.
       Example folder structure:

   ```
   /Views
       /Default
           /Home
               Index.cshtml
       /abc_com
           /Home
               Index.cshtml
        /abc_net
           /Home
               Index.cshtml
       /xyz_com
           /Home
               Index.cshtml

   ```

3. **Run the Application**:

   Start your .NET Core application and make requests from different hosts to see the dynamic view resolution and custom headers or content in action.

## Example Usage

- **Request from `http://localhost`**:

  - Views will be served from `/Views/Default/`.
  - Custom header `X-Domain-Name` will be set to `Default Domain`.

- **Request from `http://abc.com`**:

  - Views will be served from `/Views/abc_com/`.
  - Custom header `X-Domain-Name` will be set to `ABC Domain`.

  - **Request from `http://abc.net`**:

  - Views will be served from `/Views/abc_net/`.
  - Custom header `X-Domain-Name` will be set to `ABC Domain`.

- **Request from `http://xyz.com`**:

  - Views will be served from `/Views/xyz_com/`.
  - Custom header `X-Domain-Name` will be set to `XYZ Domain`.

## Additional Notes

- **Feature Flags**: The `PopulateValues` method in `DomainViewLocationExpander` can be modified to read additional feature flags or parameters as needed.

- **Customization**: Adjust the domain-to-folder mappings in `ExpandViewLocations` and domain-specific headers or content in `DomainMiddleware` to fit your specific requirements.

---
