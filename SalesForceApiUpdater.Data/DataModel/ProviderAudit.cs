using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesForceApiUpdater.Data.Utility;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class ProviderAudit : SalesForceProcessorBase
    {
        private string _salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceProviderAuditRoute"];

        [JsonProperty("PracticeId", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public int? PracticeId { get; set; }

        [JsonProperty("ProviderId")]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public int? ProviderId { get; set; }

        [JsonProperty("PatientId", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public int? PatientId { get; set; }

        [JsonProperty("subYear", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string SubYear { get; set; }

        [JsonProperty("MeasureNumber", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string MeasureNumber { get; set; }

        [JsonProperty("MeasureConcept", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string MeasureConcept { get; set; }

        [JsonProperty("AuditDescription", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string AuditDescription { get; set; }

        [JsonProperty("PatientXID", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string PatientXId { get; set; }

        [JsonProperty("FileName", Required = Required.Always)]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public string FileName { get; set; }

        [JsonProperty("FileReceivedDate")]
        [JsonConverter(typeof(SalesForceAPIConverter))]
        public DateTime? FileReceivedDate { get; set; }

        #region "Should Serialize Methods for Json"
        /// <summary>
        /// Called by the JsonConvert.SerializeObject method. Any public bool ShouldSerialize{Property name} will work.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeFileReceivedDate()
        {
            return FileReceivedDate.HasValue;
        }

        public bool ShouldSerializeProviderId()
        {
            return ProviderId.HasValue;
        }
        #endregion

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
                string apiMessage = JsonConvert.SerializeObject(this);
                string result = await ApiResponseWrapper.Send(SalesForce, SalesForceApiRoute, apiMessage);
                results.Add(String.Format("{0:HH:mm:ss tt} | {1} | {2}", DateTime.Now, result, apiMessage));
                return results;
            }
            catch (Exception e)
            {
                results.Add(String.Format("Send Failed while processing ProviderAudit. Error: {0}", e.Message));
                return results;
            }
        }
    }
}