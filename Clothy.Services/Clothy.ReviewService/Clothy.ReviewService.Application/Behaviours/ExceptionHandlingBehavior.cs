using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Clothy.ReviewService.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clothy.ReviewService.Application.Behaviours
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger;

        public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (DomainException domainException)
            {
                logger.LogWarning(domainException, "Domain exception in {RequestName}: {@Request}", typeof(TRequest).Name, request);
                throw;
            }
            catch (FluentValidation.ValidationException validationsErrors)
            {
                foreach (var failure in validationsErrors.Errors)
                {
                    logger.LogWarning(validationsErrors, "Validation failed in {RequestName}: Property {PropertyName}, Error: {ErrorMessage}", typeof(TRequest).Name, failure.PropertyName, failure.ErrorMessage);
                }
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception in {RequestName}: {@Request}", typeof(TRequest).Name, request);
                throw;
            }
        }
    }
}
