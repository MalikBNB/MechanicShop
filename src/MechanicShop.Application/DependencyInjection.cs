using System.Reflection;
using FluentValidation;
using MechanicShop.Application.Common.Behaviours;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        /*
        Registers all:
            IRequestHandler<TRequest, TResponse>
            INotificationHandler<T>
        Found in the Application assembly
        */
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); //Scans the current assembly and Automatically registers all classes that implement: IValidator<T>

            /*
            These are cross-cutting concerns applied to every request. Think of them like ASP.NET middleware, but per request/command/query.

            For a request:
                await mediator.Send(command);
            Pipeline runs like this:
                ValidationBehavior
                → PerformanceBehaviour
                → UnhandledExceptionBehaviour
                → CachingBehavior
                → Handler
            (Then unwinds back up)
            */
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}