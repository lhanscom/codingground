using System;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.DataModel
{
    public interface IApiResponseWrapper
    {
        Task<string> getResponse(SalesForceApiUpdater.Data.SalesForceApi.ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage = "");
        Task<string> Send(SalesForceApi.ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage);
        bool IsSuccessStatusCode { get; set; }
    }
}
