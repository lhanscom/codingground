using System.Collections.Generic;
using System.Threading.Tasks;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace SalesForceApiUpdater.Data.DataModel
{
    public interface ISalesForceProcessor
    {
        IMingleWebDatabaseAccess DatabaseAccess { get; set; }
        string SalesForceApiRoute { get; set; }
        ISalesForceHelper SalesForce { get; set; }
        IApiResponseWrapper ApiResponseWrapper { get; set; }
        Task<List<string>> performProcess();
    }
}