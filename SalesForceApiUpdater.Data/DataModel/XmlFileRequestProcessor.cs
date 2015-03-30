using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class XmlFileRequestProcessor: SalesForceProcessorBase
    {
        private string salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceXmlFileReCreate"];        

        public override string SalesForceApiRoute
        {
            get { return salesForceApiRoute; }
            set { salesForceApiRoute = value; }
        }

        public override async Task<List<string>> performProcess()
        {
            var result = await ApiResponseWrapper.getResponse(SalesForce, SalesForceApiRoute);
            var resultMessages = new List<String>();

            if (ApiResponseWrapper.IsSuccessStatusCode)
            {
                try
                {
                    if (result == "[]") return resultMessages;
                    var results = JsonConvert.DeserializeObject<List<XMLFileReRequested>>(result);
                    foreach (var updt in results)
                    {
                        updt.DatabaseAccess = DatabaseAccess;
                        var dbRslt = await updt.UpdateXmlTracking();
                        resultMessages.Add(dbRslt);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(
                        string.Format(
                            "Failure when attempting to process XML File Request. Error: {0}", e.Message), e);
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
    }
}
