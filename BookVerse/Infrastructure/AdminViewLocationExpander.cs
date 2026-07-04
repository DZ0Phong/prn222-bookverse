using Microsoft.AspNetCore.Mvc.Razor;

namespace BookVerse.Infrastructure
{
    /// <summary>
    /// Expands view search locations to include Views/Admin/{controller}/{action}
    /// when the controller is in the Controllers.Admin namespace.
    /// This allows Admin controllers to find their views under Views/Admin/ folder.
    /// </summary>
    public class AdminViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            // Check if the controller is an admin controller
            // by looking for "Admin" in the controller type's namespace
            if (context.ActionContext.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
            {
                // Check if this is an admin route (Admin/* pattern)
                var routeValues = context.ActionContext.ActionDescriptor.RouteValues;
                bool isAdminController = false;

                if (routeValues.TryGetValue("area", out var area) && area == "Admin")
                {
                    isAdminController = true;
                }
                else
                {
                    // Check the display name contains Admin namespace
                    var displayName = context.ActionContext.ActionDescriptor.DisplayName ?? "";
                    if (displayName.Contains("BookVerse.Controllers.Admin"))
                        isAdminController = true;
                }

                if (isAdminController)
                {
                    // Prepend Admin-specific locations
                    return new[]
                    {
                        "/Views/Admin/{1}/{0}.cshtml",
                        "/Views/Admin/Shared/{0}.cshtml",
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Store whether this is an admin controller for caching
            var displayName = context.ActionContext.ActionDescriptor.DisplayName ?? "";
            context.Values["isAdmin"] = displayName.Contains("BookVerse.Controllers.Admin") ? "1" : "0";
        }
    }
}
