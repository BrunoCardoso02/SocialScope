using Microsoft.Extensions.DependencyInjection;
using SocialScope.Application.Features.Auth.Login;
using SocialScope.Application.Features.Auth.Register;

namespace SocialScope.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        return services;
    }
}
