using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using SalesForceApiUpdater.Data.SalesForceApi;
using System.Configuration;
using SalesForceApiUpdater.Data.Utility;
using SalesForceApiUpdater.Data.DataAccess;
using System.Data;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class SelectedMethodInfoObject
    {
        public int? ProviderId { get; set; }
        public int PracticeId { get; set; }
        public string Method { get; set; }
        public string Measures { get; set; }
        public string SubYear { get; set; }
        public string Submitting { get; set; }
        public string HasConsent { get; set; }
        public string ConsentBy { get; set; }
        public string Submitted { get; set; }
        public DateTime? LastGeneratedDate { get; set; }
        public string MedicareBatchId { get; set; }
        public string GProRegId { get; set; }

        private void GetMeasures()
        {
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();
            DataTable measureTable;

            if (this.ProviderId != null)
            {
                measureTable = da.GetMeasuresForNonGProProvider(this.ProviderId.Value, this.SubYear);
            }
            else
            {
                measureTable = da.GetMeasuresForGProPractice(this.PracticeId, this.SubYear);
            }

            // If there are measures the add them to a comma delimited string
            String measureCollection = string.Empty;
            if (measureTable.Rows.Count > 0)
            {
                foreach (DataRow row in measureTable.Rows)
                {
                    if (!DBNull.Value.Equals(row["Measnum"]))
                    {
                        if (this.IsMeasureGroupNumber((int)row[0]))
                        {
                            measureCollection += da.GetMeasureGroupCodeFromNumber((int)row[0]);
                        }
                        else
                        {
                            measureCollection += row[0].ToString() + ",";
                        }
                    }
                }
            }

            // Assign to the Measures property
            this.Measures = measureCollection.TrimEnd(',');
        }

        private async Task<string> SendApi(string apiMessage, string salesForceApiRoute, SFToken token)
        {
            HttpClient queryClient = new HttpClient();
            HttpRequestMessage request;

            // generate an authorization token
            //SalesForceHelper sfHelper = new SalesForceHelper();
            //SFToken token = await sfHelper.GenerateSFToken();

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

        public async Task<string> SendMethodGPro(SFToken token)
        {
            this.GetMeasures();

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"subYear\":\"" + this.SubYear.GetStringForApi() + "\"");
            sb.Append(",\"GPRORegID\":\"" + this.GProRegId.GetStringForApi() + "\"");
            sb.Append(",\"Measures\":\"" + this.Measures.GetStringForApi() + "\"");
            sb.Append("}");

            return await this.SendApi(sb.ToString(), ConfigurationManager.AppSettings["GProSubmissionApiRoute"].ToString(), token);
        }

        public async Task<string> SendMethodNonGPro(SFToken token)
        {
            this.GetMeasures();

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"ProviderId\":\"" + this.ProviderId.GetIntForApi() + "\"");
            sb.Append(",\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"subYear\":\"" + this.SubYear.GetStringForApi() + "\"");
            sb.Append(",\"Method\":\"" + this.Method.GetStringForApi() + "\"");
            sb.Append(",\"Measures\":\"" + this.Measures.GetStringForApi() + "\"");
            sb.Append(",\"SubmittingInd\":\"" + this.Submitting.GetStringForApi() + "\"");
            sb.Append(",\"HasConsentedInd\":\"" + this.HasConsent.GetStringForApi() + "\"");
            sb.Append(",\"hasSubmittedInd\":\"" + this.Submitted.GetStringForApi() + "\"");
            sb.Append(",\"LastGeneratedDate\":\"" + this.LastGeneratedDate.GetStringForApi() + "\"");
            sb.Append(",\"MedicareBatchID\":\"" + this.MedicareBatchId.GetStringForApi() + "\"");
            sb.Append("}");

            return await this.SendApi(sb.ToString(), ConfigurationManager.AppSettings["NonGProSubmissionApiRoute"].ToString(), token);
        }

        private bool IsMeasureGroupNumber(int measureNumber)
        {
            string temp = measureNumber.ToString();
            if ((temp.Length == 4) && (temp.IndexOf('5') == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
