using Clothy.NotificationService.BLL.Services.Interfaces;
using Microsoft.CodeAnalysis;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Services
{
    public class RazorTemplateRender : ITemplateRender
    {
        private RazorLightEngine engine;

        public RazorTemplateRender()
        {
            string templateRoot = Path.Combine(AppContext.BaseDirectory, "Services", "EmailTemplates");

            engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templateRoot)
                .UseMemoryCachingProvider()
                .Build();
        }

        public Task<string> RenderAsync<TModel>(string templatePath, TModel model, CancellationToken cancellationToken = default)
        {
            return engine.CompileRenderAsync(templatePath, model);
        }
    }
}
