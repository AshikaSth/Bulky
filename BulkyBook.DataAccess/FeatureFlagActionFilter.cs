using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess
{
    public class FeatureFlagActionFilter : IAsyncActionFilter
    {
        private readonly IFeatureFlagRepository _featureFlagRepository;

        public FeatureFlagActionFilter(IFeatureFlagRepository featureFlagRepository)
        {
            _featureFlagRepository = featureFlagRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
			// Exclude specific actions like FeatureDisabled
			var actionName = context.ActionDescriptor.RouteValues["action"];
			var controllerName = context.ActionDescriptor.RouteValues["controller"];
			// Use the repository to check feature status
            if (controllerName == "Home" && actionName == "FeatureDisabled")
            {
                await next(); // Bypass filter for this action
                return;
            }
			// Use the repository to check feature status
			bool isAddToCartEnabled = await _featureFlagRepository.GetFeatureFlagStatusAsync("AddToCart");
			bool isIncludeCategoryEnabled = await _featureFlagRepository.GetFeatureFlagStatusAsync("IncludeCategory");

			context.HttpContext.Items["AddToCart"] = isAddToCartEnabled;
			context.HttpContext.Items["IncludeCategory"] = isIncludeCategoryEnabled;
			if (!isAddToCartEnabled)
            {
                // Redirect to a feature disabled page if the flag is not enabled
                context.Result = new RedirectToActionResult("FeatureDisabled", "Home", null);
                return;
            }

            if (isIncludeCategoryEnabled)
            {
                // Set a flag to be passed to views, indicating the category feature is enabled
                context.HttpContext.Items["IncludeCategory"] = true;
            }
            else
            {
                context.HttpContext.Items["IncludeCategory"] = false;
            }

            // Continue with the action execution if the feature flag is enabled
            await next();

        }
    }
}
