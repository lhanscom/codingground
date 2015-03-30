using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesForceApiUpdater.Data.DataAccess;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class XMLFileReRequested
    {
        private IMingleWebDatabaseAccess databaseAccess = new MingleWebDatabaseAccess();

        public IMingleWebDatabaseAccess DatabaseAccess
        {
            get { return databaseAccess; }
            set { databaseAccess = value; }
        }

        [Newtonsoft.Json.JsonProperty("ClientId")]
        int ClientId { get; set; }

        [Newtonsoft.Json.JsonProperty("PracticeId")]
        int PracticeId { get; set; }

        [Newtonsoft.Json.JsonProperty("subYear")]
        string SubYear { get; set; }

        public async Task<string> UpdateXmlTracking()
        {
            if (ClientId <= 0 || PracticeId <= 0 || SubYear.Length != 4)
                return
                    String.Format(
                        "Failure: XML Tracking could not be added. ClientID: {0} ProviderID: {1} SubYear: {2}", ClientId,
                        PracticeId, SubYear);

            databaseAccess.InsertXmlTracking(ClientId, PracticeId, SubYear);
            return
                String.Format(
                    "XML Tracking record added. ClientID: {0} ProviderID: {1} SubYear: {2}",
                    ClientId, PracticeId, SubYear);
        }
    }
}
