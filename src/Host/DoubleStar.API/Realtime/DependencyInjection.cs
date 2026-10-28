
using DoubleStar.SharedKernel.Contracts.Repairs;
using MediatR;

namespace DoubleStar.Api.Realtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRealtimeServices(
        this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddTransient<
            INotificationHandler<RepairStatusChangedEvent>,
            RepairStatusChangedSignalRHandler>();

        return services;
    }
}
