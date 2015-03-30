using System;
using System.Threading.Tasks;
using Interview.Data.RemoteApi;

namespace Interview.Data.DataModel
{
    public interface IApiResponseWrapper
    {
        Task<string> getResponse(IRemoteHelper _remote, string salesForceApiRoute, string apiMessage = "");
        Task<string> Send(RemoteApi.IRemoteHelper _remote, string salesForceApiRoute, string apiMessage);
        bool IsSuccessStatusCode { get; set; }
    }
}
