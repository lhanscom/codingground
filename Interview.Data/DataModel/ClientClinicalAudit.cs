using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesForceApiUpdater.Data.Utility;

#pragma warning disable 1998

namespace SalesForceApiUpdater.Data.DataModel
{
    public class ClientClinicalAudit : SalesForceProcessorBase
    {
        private string _salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceClientClinicalAuditRoute"];

        [JsonProperty("ClientId", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public int ClientId { get; set; }

        [JsonProperty("subYear", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string SubYear { get; set; }

        [JsonProperty("MeasureNumber", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string MeasureNumber { get; set; }

        [JsonProperty("FileName", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string FileName { get; set; }

        [JsonProperty("FileReceivedDate", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public DateTime? FileReceivedDate { get; set; }

        public override string SalesForceApiRoute
        {
            get { return _salesForceApiRoute; }
            set { _salesForceApiRoute = value; }
        }

        public override async Task<List<string>> performProcess()
        {
            var results = new List<string>();
            try
            {
                var apiMessage = JsonConvert.SerializeObject(this);
                var result = await ApiResponseWrapper.getResponse(SalesForce, SalesForceApiRoute, apiMessage);
                results.Add(String.Format("{0:HH:mm:ss tt} | {1} | {2}", DateTime.Now, result, apiMessage));
                return results;
            }
            catch (Exception e)
            {
                results.Add(String.Format("Send Failed while processing ClientClinicalAudit. Error: {0}", e.Message));
                return results;
            }
        }
    }
}