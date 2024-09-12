using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Fonts;
using System.Reflection;

namespace AktBob.DocumentGenerator;
public static class ModuleServices
{
    public static IServiceCollection AddDocumentGeneratorModule(this IServiceCollection services, List<Assembly> mediatrAssemblies)
    {
        // Register the custom font resolver globally
        GlobalFontSettings.FontResolver = new CustomFontResolver();
        mediatrAssemblies.Add(typeof(ModuleServices).Assembly);
        return services;
    }
}
