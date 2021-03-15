using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace QuRest.WebUI.OpenApi
{
    public class FeatureGateFilter : IDocumentFilter
    {
        private readonly IFeatureManager featureManager;

        public FeatureGateFilter(IFeatureManager featureManager)
        {
            this.featureManager = featureManager;
        }

        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                var controllerActionDescriptor = (apiDescription.ActionDescriptor as ControllerActionDescriptor);

                var attribute = controllerActionDescriptor?.MethodInfo.GetCustomAttributes(typeof(FeatureGateAttribute), true).FirstOrDefault();

                var featureGateAttribute = attribute as FeatureGateAttribute;

                if (featureGateAttribute is null) continue;

                foreach (var featureName in featureGateAttribute.Features)
                {
                    if (this.featureManager.IsEnabledAsync(featureName).Result) continue;

                    var route = "/" + apiDescription.RelativePath.TrimEnd('/');
                    openApiDoc.Paths.Remove(route);
                }
            }
        }
    }
}
