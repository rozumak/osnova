using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Osnova.StaticRazorPages.Infrastructure;

namespace Osnova.StaticRazorPages.ApplicationModels;

public class StaticPageApplicationModelPartsProvider : IPageApplicationModelPartsProvider
{
    private readonly IPageApplicationModelPartsProvider _originalProvider;

    public StaticPageApplicationModelPartsProvider(IPageApplicationModelPartsProvider originalProvider)
    {
        _originalProvider = originalProvider;
    }

    public PageHandlerModel? CreateHandlerModel(MethodInfo method)
    {
        if (!IsHandler(method))
        {
            return null;
        }

        if (method.Name.Equals(StaticPageMethodNames.OnGetStatic, StringComparison.Ordinal))
        {
            var handlerModel = new PageHandlerModel(method, Array.Empty<object>())
            {
                Name = StaticPageMethodNames.OnGetStatic,
                HandlerName = null,
                HttpMethod = "Get",
            };

            var methodParameters = handlerModel.MethodInfo.GetParameters();

            foreach (var parameter in methodParameters)
            {
                var parameterModel = CreateParameterModel(parameter);
                parameterModel.Handler = handlerModel;

                handlerModel.Parameters.Add(parameterModel);
            }

            return handlerModel;
        }

        var pageHandler = _originalProvider.CreateHandlerModel(method);
        return pageHandler;
    }

    public PageParameterModel CreateParameterModel(ParameterInfo parameter)
    {
        return _originalProvider.CreateParameterModel(parameter);
    }

    public PagePropertyModel CreatePropertyModel(PropertyInfo property)
    {
        return _originalProvider.CreatePropertyModel(property);
    }

    public bool IsHandler(MethodInfo methodInfo)
    {
        return _originalProvider.IsHandler(methodInfo);
    }
}