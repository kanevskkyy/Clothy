using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Clothy.ReviewService.Application.Interfaces.Commands
{
    public interface ICommand : IRequest<Unit> 
    {
        
    }

    public interface ICommand<out TResponse> : IRequest<TResponse> 
    {

    }
}
