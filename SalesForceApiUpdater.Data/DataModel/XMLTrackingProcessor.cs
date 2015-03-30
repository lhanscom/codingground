using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace SalesForceApiUpdater.Data.DataModel
{
    //public class SfHttpResponseMessage : I
    public class XmlTrackingProcessor : SalesForceProcessorBase
    {
        private string salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceXmlTracking"];

        public override async Task<List<string>> performProcess()
        {
            var result = await ApiResponseWrapper.getResponse(SalesForce, SalesForceApiRoute);
            var resultMessages = new List<String>();

            if (ApiResponseWrapper.IsSuccessStatusCode)
            {
                if (result == "[]") return resultMessages;
                var xmlTrackingUpdates = JsonConvert.DeserializeObject<List<XMLTrackingObject>>(result);
                foreach (var updt in xmlTrackingUpdates)
                {
                    updt.DatabaseAccess = DatabaseAccess;
                    var dbRslt = await updt.UpdateXmlTracking();
                    resultMessages.Add(dbRslt);
                }
            }
            else
            {
                throw new Exception(
                    string.Format(
                        "Failure Status Code Returned from API when attempting to get XML Tracking: {0}", result));
            }

            return resultMessages;
        }


        public override string SalesForceApiRoute
        {
            get { return salesForceApiRoute; }
            set { salesForceApiRoute = value; }
        }
    }
}