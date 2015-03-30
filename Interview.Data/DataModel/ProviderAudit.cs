using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Interview.Data.Utility;
using Newtonsoft.Json;

namespace Interview.Data.DataModel
{
    public class ProviderAudit : RemoteProcessorBase
    {
        private string _remoteApiRoute = ConfigurationManager.AppSettings["RemoteProviderAuditRoute"];

        [JsonProperty("PracticeId", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public int? PracticeId { get; set; }

        [JsonProperty("ProviderId")]
        [JsonConverter(typeof(RemoteApiConverter))]
        public int? ProviderId { get; set; }

        [JsonProperty("PatientId", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public int? PatientId { get; set; }

        [JsonProperty("subYear", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string SubYear { get; set; }

        [JsonProperty("MeasureNumber", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string MeasureNumber { get; set; }

        [JsonProperty("MeasureConcept", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string MeasureConcept { get; set; }

        [JsonProperty("AuditDescription", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string AuditDescription { get; set; }

        [JsonProperty("PatientXID", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string PatientXId { get; set; }

        [JsonProperty("FileName", Required = Required.Always)]
        [JsonConverter(typeof(RemoteApiConverter))]
        public string FileName { get; set; }

        [JsonProperty("FileReceivedDate")]
        [JsonConverter(typeof(RemoteApiConverter))]
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

        public override string RemoteApiRoute
        {
            get { return _remoteApiRoute; }
            set { _remoteApiRoute = value; }
        }

        public override async Task<List<string>> performProcess()
        {
            var results = new List<string>();
            try
            {
                string apiMessage = JsonConvert.SerializeObject(this);
                string result = await ApiResponseWrapper.Send(Remote, RemoteApiRoute, apiMessage);
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