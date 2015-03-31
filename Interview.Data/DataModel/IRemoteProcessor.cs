using System.Collections.Generic;
using System.Threading.Tasks;
using Interview.Data.DataAccess;
using Interview.Data.RemoteApi;

namespace Interview.Data.DataModel
{
    public interface IRemoteProcessor
    {
        IMingleWebDatabaseAccess DatabaseAccess { get; set; }
        string RemoteApiRoute { get; set; }
        IRemoteHelper Remote { get; set; }
        IApiResponseWrapper ApiResponseWrapper { get; set; }
        List<string> performProcess();
    }
}