using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using SalesForceApiUpdater.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesForceApiUpdater.Data.SalesForceApi;
using SalesForceApiUpdater.Data.Utility;

namespace SalesForceApiUpdater.Data.DataModel
{
    public interface IXMLTrackingObject
    {
        IMingleWebDatabaseAccess DatabaseAccess { get; set; }

        [Newtonsoft.Json.JsonProperty("XMLTrackingID")]
        int XMLTrackingID { get; set; }
        [Newtonsoft.Json.JsonProperty("PracticeId")]
        int PracticeId { get; set; }

        [Newtonsoft.Json.JsonProperty("subYear")]
        string SubYear { get; set; }

        string FileName { get; set; }
        DateTime GeneratedDate { get; set; }

        [Newtonsoft.Json.JsonProperty("medicareBatchId")]
        string XMLMedicareBatchId { get; set; }

        [Newtonsoft.Json.JsonProperty("submittedDate")]
        DateTime? XMLSubmitted { get; set; }

        Task<string> UpdateXmlTracking();
        Task<string> SendXmlTracking(SFToken token);
    }

    public class XMLTrackingObject : IXMLTrackingObject
    {
        private IMingleWebDatabaseAccess databaseAccess = new MingleWebDatabaseAccess();

        public IMingleWebDatabaseAccess DatabaseAccess
    {
            get { return databaseAccess; }
            set { databaseAccess = value; }
        }

        [Newtonsoft.Json.JsonProperty("XMLTrackingID")]
        public int XMLTrackingID { get; set; }
        [Newtonsoft.Json.JsonProperty("PracticeId")]
        public int PracticeId { get; set; }
        [Newtonsoft.Json.JsonProperty("subYear")]
        public string SubYear { get; set; }
        public string FileName { get; set; }
        public DateTime GeneratedDate { get; set; }
        [Newtonsoft.Json.JsonProperty("medicareBatchId")]
        public string XMLMedicareBatchId { get; set; }
        [Newtonsoft.Json.JsonProperty("submittedDate")]
        public DateTime? XMLSubmitted { get; set; }

        public async Task<string> UpdateXmlTracking()
        {
            var da = new MingleWebDatabaseAccess();
            var xmlSub = XMLSubmitted.HasValue ? XMLSubmitted.Value.ToShortDateString() : "null";
            if (XMLTrackingID < 1)
                return
                    String.Format(
                        "Failure: Invalid XMLTrackingID, this can happen when things were done out of order. XML_Tracking ID: {0} ProviderID: {1} SubYear: {2} Medicare Batch ID: {3} Submission Date: {4}",
                        XMLTrackingID, PracticeId, SubYear, XMLMedicareBatchId, xmlSub);
            databaseAccess.UpdateXmlTracking(XMLTrackingID, PracticeId, SubYear, XMLMedicareBatchId, XMLSubmitted);
            return
                String.Format(
                    "XML Tracking updated for XML_Tracking ID: {0} ProviderID: {1} SubYear: {2} Medicare Batch ID: {3} Submission Date: {4}",
                    XMLTrackingID, PracticeId, SubYear, XMLMedicareBatchId, xmlSub);
        }

        public async Task<string> SendXmlTracking(SFToken token)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"subYear\":\"" + this.SubYear.GetStringForApi() + "\"");
            sb.Append(",\"FileName\":\"" + this.FileName.GetStringForApi() + "\"");
            sb.Append(",\"GeneratedDate\":\"" + this.GeneratedDate.GetDateTimeForApi() + "\"");
            sb.Append(",\"XMLTrackingID\":\"" + this.XMLTrackingID.GetIntForApi() + "\"");
            sb.Append("}");

            return await this.SendApi(sb.ToString(), ConfigurationManager.AppSettings["XMLFileGeneratedRoute"].ToString(), token);
        }

        private async Task<string> SendApi(string apiMessage, string salesForceApiRoute, SFToken token)
        {
            var queryClient = new HttpClient();

            string restQuery = token.InstanceURL + salesForceApiRoute;

            var request = new HttpRequestMessage(HttpMethod.Post, restQuery)
            {
                Content = new StringContent(apiMessage, Encoding.UTF8, "application/json")
            };

            //add token to header
            request.Headers.Add("Authorization", "Bearer " + token.Token);

            //return JSON to the caller
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            HttpResponseMessage response = await queryClient.SendAsync(request);

            // read the result from the API
            string result = await response.Content.ReadAsStringAsync();

            string resultMessage = DateTime.Now.ToString("HH:mm:ss tt") + " | " + result + " | " + apiMessage; // Adding DEBUG information to output

            return resultMessage;
        }
    }
}
