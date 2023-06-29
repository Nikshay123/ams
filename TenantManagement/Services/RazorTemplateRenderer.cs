using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TenantManagement.Models;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services;

public class RazorTemplateRenderer : IRazorEmailRenderer
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorTemplateRenderer(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> Render(Dictionary<string, object> data, string template)
    {
        var viewPath = $"~/EmailTemplates/{template}.cshtml";
        var result = _razorViewEngine.GetView(null, viewPath, true);

        if (result.Success != true)
        {
            var searchedLocations = string.Join("\n", result.SearchedLocations);
            throw new InvalidOperationException($"Could not find this view: {viewPath}. Searched locations:\n{searchedLocations}");
        }

        var view = result.View;

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = _serviceProvider;

        var actionContext = new ActionContext(
                httpContext,
                httpContext.GetRouteData(),
                new ActionDescriptor()
            );

        using (var writer = new StringWriter())
        {
            var viewDataDict = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary());

            viewDataDict.Model = new EmailBaseModel(data);

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewDataDict,
                new TempDataDictionary(
                    httpContext.HttpContext,
                    _tempDataProvider
                ),
                writer,
                new HtmlHelperOptions { }
            );

            await view.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}