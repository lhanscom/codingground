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
    public class PracticeInfoObject
    {
        public PracticeInfoObject(int practiceId)
        {
            if (practiceId != -1)
            {
                this.LoadPracticeDataFromDatabase(practiceId);
            }
            else
            {
                this.CreateNullPractice();
            }
        }
        public int? PracticeId { get; set; }
        public int? ClientId { get; set; }
        public string IsActive { get; set; }
        public string PracticeName { get; set; }
        public string XID { get; set; }
        public string SubOf { get; set; }
        public string TIN { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PConFirst { get; set; }
        public string PConLast { get; set; }
        public string PConRole { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        private void LoadPracticeDataFromDatabase(int practiceId)
        {
            // lookup practice information for the record tracked
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();
            DataTable practiceDataTable = da.GetPracticeData(practiceId);

            // create a PracticeInfoObject and populate it with client data from the DataTable
            this.PracticeId = practiceDataTable.Rows[0].GetIntFromDatabase("ID");
            this.ClientId = practiceDataTable.Rows[0].GetIntFromDatabase("ClientID");
            this.IsActive = practiceDataTable.Rows[0].GetStringFromDatabase("IsActive");
            this.PracticeName = practiceDataTable.Rows[0].GetStringFromDatabase("Name");
            this.TIN = practiceDataTable.Rows[0].GetStringFromDatabase("TIN");
            this.Addr1 = practiceDataTable.Rows[0].GetStringFromDatabase("addr1");
            this.Addr2 = practiceDataTable.Rows[0].GetStringFromDatabase("addr2");
            this.City = practiceDataTable.Rows[0].GetStringFromDatabase("City");
            this.State = practiceDataTable.Rows[0].GetStringFromDatabase("State");
            this.Zip = practiceDataTable.Rows[0].GetStringFromDatabase("Zip");
            this.PConFirst = practiceDataTable.Rows[0].GetStringFromDatabase("PConFirst");
            this.PConLast = practiceDataTable.Rows[0].GetStringFromDatabase("PConLast");
            this.PConRole = practiceDataTable.Rows[0].GetStringFromDatabase("pconrole");
            this.Email = practiceDataTable.Rows[0].GetStringFromDatabase("email");
            this.Phone = practiceDataTable.Rows[0].GetStringFromDatabase("phone");
        }

        private void CreateNullPractice()
        {
            // create a PracticeInfoObject and populate it with NULL
            this.PracticeId = null;
            this.ClientId = null;
            this.IsActive = null;
            this.PracticeName = null;
            this.TIN = null;
            this.Addr1 = null;
            this.Addr2 = null;
            this.City = null;
            this.State = null;
            this.Zip = null;
            this.PConFirst = null;
            this.PConLast = null;
            this.PConRole = null;
            this.Email = null;
            this.Phone = null;
        }

        public async Task<string> SendInfoToSalesForce(SFToken token)
        {
            HttpClient queryClient = new HttpClient();
            HttpRequestMessage request;

            // generate an authorization token
            //SalesForceHelper sfHelper = new SalesForceHelper();
            //SFToken token = await sfHelper.GenerateSFToken();

            string salesForceApiRoute = ConfigurationManager.AppSettings["PracticeApiRoute"].ToString();
            string restQuery = token.InstanceURL + salesForceApiRoute;

            request = new HttpRequestMessage(HttpMethod.Post, restQuery);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"ClientId\":\"" + this.ClientId.GetIntForApi() + "\"");
            sb.Append(",\"isActive\":\"" + this.IsActive.GetStringForApi() + "\"");
            sb.Append(",\"OrgName\":\"" + this.PracticeName.GetStringForApi() + "\"");
            sb.Append(",\"XID\":\"" + this.XID.GetStringForApi() + "\"");
            sb.Append(",\"SUbOf\":\"" + this.SubOf.GetStringForApi() + "\"");
            sb.Append(",\"TIN\":\"" + this.TIN.GetStringForApi() + "\"");
            sb.Append(",\"addr1\":\"" + this.Addr1.GetStringForApi() + "\"");
            sb.Append(",\"addr2\":\"" + this.Addr2.GetStringForApi() + "\"");
            sb.Append(",\"City\":\"" + this.City.GetStringForApi() + "\"");
            sb.Append(",\"State\":\"" + this.State.GetStringForApi() + "\"");
            sb.Append(",\"Zip\":\"" + this.Zip.GetStringForApi() + "\"");
            sb.Append(",\"PConFirst\":\"" + this.PConFirst.GetStringForApi() + "\"");
            sb.Append(",\"PConLast\":\"" + this.PConLast.GetStringForApi() + "\"");
            sb.Append(",\"PConRole\":\"" + this.PConRole.GetStringForApi() + "\"");
            sb.Append(",\"Phone\":\"" + this.Phone.GetStringForApi() + "\"");
            sb.Append(",\"email\":\"" + this.Email.GetStringForApi() + "\"");
            sb.Append("}");

            request.Content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");

            //add token to header
            request.Headers.Add("Authorization", "Bearer " + token.Token);

            //return XML to the caller
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            //return JSON to the caller
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            HttpResponseMessage response = await queryClient.SendAsync(request);

            // read the result
            string result = await response.Content.ReadAsStringAsync();

            string resultMessage = DateTime.Now.ToString("HH:mm:ss tt") + " | " + result + " | " + sb.ToString(); // Adding DEBUG information to output

            return resultMessage;
        }
    }
}
