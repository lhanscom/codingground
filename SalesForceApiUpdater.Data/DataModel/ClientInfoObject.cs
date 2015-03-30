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
    public class ClientInfoObject
    {
        public ClientInfoObject (int clientId)
        {
            if (clientId != -1)
            {
                this.LoadClientDataFromDatabase(clientId);
            }
            else
            {
                this.CreateNullClient();
            }
        }
        public int? ClientId { get; set; }
        public string IsActive { get; set; }
        public string Orgname { get; set; }
        public string TIN { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string City { get; set; }
        public string State {get; set;}
        public string Zip {get; set;}
        public string ExecutiveFirstName {get; set;}
        public string ExecutiveLastName {get; set;}
        public string ExecutivePhone {get; set;}
        public string ExecutiveEmail {get; set;}
        public string PConFirst {get; set;}
        public string PConLast {get; set;}
        public string PConRole {get; set;}
        public string Email {get; set;}
        public DateTime? RegisterDate {get; set;}
        public string Phone {get; set;}
        public string CustomerId {get; set;}

        private void LoadClientDataFromDatabase(int clientId)
        {
            // lookup client information for the record tracked
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();
            DataTable clientDataTable = da.GetClientData(clientId);

            // create a ClientInfoObject and populate it with client data from the DataTable
            this.ClientId = clientDataTable.Rows[0].GetIntFromDatabase("ID");
            // Hardcoded as IsActive is not present in the ID_Client table
            this.IsActive = "Y";
            this.Orgname = clientDataTable.Rows[0].GetStringFromDatabase("OrgName");
            this.TIN = clientDataTable.Rows[0].GetStringFromDatabase("TIN");
            this.Addr1 = clientDataTable.Rows[0].GetStringFromDatabase("addr1");
            this.Addr2 = clientDataTable.Rows[0].GetStringFromDatabase("addr2");
            this.City = clientDataTable.Rows[0].GetStringFromDatabase("City");
            this.State = clientDataTable.Rows[0].GetStringFromDatabase("State");
            this.Zip = clientDataTable.Rows[0].GetStringFromDatabase("Zip");
            this.ExecutiveFirstName = clientDataTable.Rows[0].GetStringFromDatabase("ExecutiveFirst");
            this.ExecutiveLastName = clientDataTable.Rows[0].GetStringFromDatabase("ExecutiveLast");
            this.ExecutivePhone = clientDataTable.Rows[0].GetStringFromDatabase("ExecutivePhone");
            this.ExecutiveEmail = clientDataTable.Rows[0].GetStringFromDatabase("ExecutiveEmail");
            this.PConFirst = clientDataTable.Rows[0].GetStringFromDatabase("PConFirst");
            this.PConLast = clientDataTable.Rows[0].GetStringFromDatabase("PConLast");
            this.PConRole = clientDataTable.Rows[0].GetStringFromDatabase("pconrole");
            this.Email = clientDataTable.Rows[0].GetStringFromDatabase("email");
            this.RegisterDate = clientDataTable.Rows[0].GetDateTimeFromDatabase("RegisterDate");
            this.Phone = clientDataTable.Rows[0].GetStringFromDatabase("phone");
        }

        private void CreateNullClient()
        {
            // create a ClientInfoObject and populate it with NULL
            this.ClientId = null;
            this.Orgname = null;
            this.TIN = null;
            this.Addr1 = null;
            this.Addr2 = null;
            this.City = null;
            this.State = null;
            this.Zip = null;
            this.ExecutiveFirstName = null;
            this.ExecutiveLastName = null;
            this.ExecutivePhone = null;
            this.ExecutiveEmail = null;
            this.PConFirst = null;
            this.PConLast = null;
            this.PConRole = null;
            this.Email = null;
            this.RegisterDate = null;
            this.Phone = null;
        }
        public async Task<string> SendInfoToSalesForce(SFToken token)
        {
            HttpClient queryClient = new HttpClient();
            HttpRequestMessage request;

            // generate an authorization token
            //SalesForceHelper sfHelper = new SalesForceHelper();
            //SFToken token = await sfHelper.GenerateSFToken();

            string salesForceApiRoute = ConfigurationManager.AppSettings["ClientApiRoute"].ToString();
            string restQuery = token.InstanceURL + salesForceApiRoute;

            request = new HttpRequestMessage(HttpMethod.Post, restQuery);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"OrgName\":\"" + this.Orgname.GetStringForApi() + "\"");
            sb.Append(",\"TIN\":\"" + this.TIN.GetStringForApi() + "\"");
            sb.Append(",\"isActive\":\"" + this.IsActive.GetStringForApi() + "\"");
            sb.Append(",\"addr1\":\"" + this.Addr1.GetStringForApi() + "\"");
            sb.Append(",\"addr2\":\"" + this.Addr2.GetStringForApi() + "\"");
            sb.Append(",\"City\":\"" + this.City.GetStringForApi() + "\"");
            sb.Append(",\"State\":\"" + this.State.GetStringForApi() + "\"");
            sb.Append(",\"Zip\":\"" + this.Zip.GetStringForApi() + "\"");
            sb.Append(",\"ExecutiveFirst\":\"" + this.ExecutiveFirstName.GetStringForApi() + "\"");
            sb.Append(",\"ExecutiveLast\":\"" + this.ExecutiveLastName.GetStringForApi() + "\"");
            sb.Append(",\"ExecutivePhone\":\"" + this.ExecutivePhone.GetStringForApi() + "\"");
            sb.Append(",\"ExecutiveEmail\":\"" + this.ExecutiveEmail.GetStringForApi() + "\"");
            sb.Append(",\"PConFirst\":\"" + this.PConFirst.GetStringForApi() + "\"");
            sb.Append(",\"PConLast\":\"" + this.PConLast.GetStringForApi() + "\"");
            sb.Append(",\"PConRole\":\"" + this.PConRole.GetStringForApi() + "\"");
            sb.Append(",\"email\":\"" + this.Email.GetStringForApi() + "\"");
            sb.Append(",\"RegisterDate\":\"" + this.RegisterDate.GetDateForApi() + "\"");
            sb.Append(",\"phone\":\"" + this.Phone.GetStringForApi() + "\"");
            sb.Append(",\"ClientID\":\"" + this.ClientId.GetIntForApi() + "\"");
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
