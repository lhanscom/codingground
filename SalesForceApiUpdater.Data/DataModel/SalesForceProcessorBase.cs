using Newtonsoft.Json;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.SalesForceApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.DataModel
{
    public abstract class SalesForceProcessorBase : ISalesForceProcessor
    {
        private IMingleWebDatabaseAccess databaseAccess = new MingleWebDatabaseAccess();
        
        private ISalesForceHelper salesForce = new SalesForceHelper();
        private IApiResponseWrapper apiResponseWrapper = new ApiResponseWrapper();
        [JsonIgnore]
        public virtual IMingleWebDatabaseAccess DatabaseAccess
        {
            get { return databaseAccess; }
            set { databaseAccess = value; }
        }

        [JsonIgnore]
        public abstract string SalesForceApiRoute { get; set; }

        [JsonIgnore]
        public virtual ISalesForceHelper SalesForce
        {
            get { return salesForce; }
            set { salesForce = value; }
        }

        [JsonIgnore]
        public virtual IApiResponseWrapper ApiResponseWrapper
        {
            get { return apiResponseWrapper; }
            set { apiResponseWrapper = value; }
        }


        public abstract Task<List<string>> performProcess();
    }
}
