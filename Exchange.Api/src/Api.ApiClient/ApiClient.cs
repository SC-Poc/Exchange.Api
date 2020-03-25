using Swisschain.Api.Api.ApiClient.Common;
using Swisschain.Api.Api.ApiContract;

namespace Swisschain.Api.Api.ApiClient
{
    public class ApiClient : BaseGrpcClient, IApiClient
    {
        public ApiClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
    }
}
