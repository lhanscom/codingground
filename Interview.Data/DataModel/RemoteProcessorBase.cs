using Interview.Data.DataAccess;
using Interview.Data.RemoteApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interview.Data.DataModel
{
    public abstract class RemoteProcessorBase : IRemoteProcessor
    {
        private IMingleWebDatabaseAccess databaseAccess = new MingleWebDatabaseAccess();
        
        private IRemoteHelper _remote = new RemoteHelper();
        private IApiResponseWrapper apiResponseWrapper = new ApiResponseWrapper();
        [JsonIgnore]
        public virtual IMingleWebDatabaseAccess DatabaseAccess
        {
            get { return databaseAccess; }
            set { databaseAccess = value; }
        }

        [JsonIgnore]
        public abstract string RemoteApiRoute { get; set; }

        [JsonIgnore]
        public virtual IRemoteHelper Remote
        {
            get { return _remote; }
            set { _remote = value; }
        }

        [JsonIgnore]
        public virtual IApiResponseWrapper ApiResponseWrapper
        {
            get { return apiResponseWrapper; }
            set { apiResponseWrapper = value; }
        }


        public abstract string Serialize();
    }
}
