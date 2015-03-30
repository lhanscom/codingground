using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Data;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.SalesForceApi;
using System.Configuration;
using SalesForceApiUpdater.Data.Utility;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class StepStatusInfoObject
    {
        public int ClientId { get; set; }
        public int PracticeId { get; set; }
        public string SubYear { get; set; }
        public int? Step1 { get; set; }
        public int? Step2 { get; set; }
        public int? Step3 { get; set; }
        public int? Step4 { get; set; }
        public int? Step5 { get; set; }
        public int? Step6 { get; set; }
        public int? Step7 { get; set; }
        public int? Step8 { get; set; }
        public int? Step9 { get; set; }
        public int? Step10 { get; set; }

        private async Task<string> SendApi(string apiMessage, string salesForceApiRoute, SFToken token)
        {
            HttpClient queryClient = new HttpClient();
            HttpRequestMessage request;

            string restQuery = token.InstanceURL + salesForceApiRoute;

            request = new HttpRequestMessage(HttpMethod.Post, restQuery);

            request.Content = new StringContent(apiMessage, Encoding.UTF8, "application/json");

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

        public async Task<string> SendStepStatus(SFToken token)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"subYear\":\"" + this.SubYear.GetStringForApi() + "\"");
            sb.Append(",\"Step1\":\"" + this.Step1.GetStringForApi() + "\"");
            sb.Append(",\"Step2\":\"" + this.Step2.GetStringForApi() + "\"");
            sb.Append(",\"Step3\":\"" + this.Step3.GetStringForApi() + "\"");
            sb.Append(",\"Step4\":\"" + this.Step4.GetStringForApi() + "\"");
            sb.Append(",\"Step5\":\"" + this.Step5.GetStringForApi() + "\"");
            sb.Append(",\"Step6\":\"" + this.Step6.GetStringForApi() + "\"");
            sb.Append(",\"Step7\":\"" + this.Step7.GetStringForApi() + "\"");
            sb.Append(",\"Step8\":\"" + this.Step8.GetStringForApi() + "\"");
            sb.Append(",\"Step9\":\"" + this.Step9.GetStringForApi() + "\"");
            sb.Append(",\"Step10\":\"" + this.Step10.GetStringForApi() + "\"");
            sb.Append("}");

            return await this.SendApi(sb.ToString(), ConfigurationManager.AppSettings["StepStatusSyncRoute"].ToString(), token);
        }

    }
}
