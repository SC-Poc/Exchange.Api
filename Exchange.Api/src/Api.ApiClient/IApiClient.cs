using Swisschain.Api.Api.ApiContract;

namespace Swisschain.Api.Api.ApiClient
{
    public interface IApiClient
    {
        Monitoring.MonitoringClient Monitoring { get; }
    }
}
