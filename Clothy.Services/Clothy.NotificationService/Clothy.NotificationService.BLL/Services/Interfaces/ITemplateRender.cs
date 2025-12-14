using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Services.Interfaces
{
    public interface ITemplateRender
    {
        Task<string> RenderAsync<TModel>(string templatePath, TModel model, CancellationToken cancellationToken = default);
    }
}
