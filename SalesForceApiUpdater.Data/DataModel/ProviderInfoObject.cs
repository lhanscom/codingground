using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Data;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.SalesForceApi;
using SalesForceApiUpdater.Data.Utility;
using System.Configuration;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class ProviderInfoObject
    {
        public ProviderInfoObject(int providerId)
        {
            // If the providerId is -1 then don't initialize the object properties.
            // The object properties will be set manually.
            if (providerId != -1)
            {
                this.LoadProviderDataFromDatabase(providerId);
            }
            else
            {
                this.CreateNullProvider();
            }
        }

        public int? ProviderId { get; set; }
        public int? PracticeId { get; set; }
        public int? ClientId { get; set; }
        public string IsActive { get; set; }
        public string FirstName {get; set;}
        public string LastName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Credentials { get; set; }
        public int? DivId { get; set; }
        public DateTime? FirstVisit { get; set; }
        public string GivenCredentials { get; set; }
        public string GivenSpecialty { get; set; }
        public string HaseRxPriv { get; set; }
        public DateTime? LastVisit { get; set; }
        public string LocCode { get; set; }
        public string MUDate { get; set; }
        public string NPI { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string Specialty { get; set; }
        public string Taxonomy { get; set; }
        public string TIN { get; set; }

        private void LoadProviderDataFromDatabase(int providerId)
        {
            // lookup provider information for the record tracked
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();
            DataTable providerDataTable = da.GetProviderData(providerId);

            // create a ProviderInfoObject and populate it with provider data from the DataTable
            // Note the use of three extension methods to handle DBNull values (GetStringFromDatabase, GetIntFromDatabase, and GetDateTimeFromDatabase defined in Utility namespace)
            this.ProviderId = providerDataTable.Rows[0].GetIntFromDatabase("ID");
            this.PracticeId = providerDataTable.Rows[0].GetIntFromDatabase("PracticeID");
            this.ClientId = providerDataTable.Rows[0].GetIntFromDatabase("ClientID");
            this.IsActive = providerDataTable.Rows[0].GetStringFromDatabase("IsActive"); 
            this.FirstName = providerDataTable.Rows[0].GetStringFromDatabase("FirstName");
            this.LastName = providerDataTable.Rows[0].GetStringFromDatabase("LastName");
            this.Zip = providerDataTable.Rows[0].GetStringFromDatabase("Zip");
            this.Phone = providerDataTable.Rows[0].GetStringFromDatabase("Phone");
            this.Email = providerDataTable.Rows[0].GetStringFromDatabase("email");
            this.Credentials = providerDataTable.Rows[0].GetStringFromDatabase("Credentials");
            this.DivId = providerDataTable.Rows[0].GetIntFromDatabase("DivID");
            this.FirstVisit = providerDataTable.Rows[0].GetDateTimeFromDatabase("FirstVisit");
            this.GivenCredentials = providerDataTable.Rows[0].GetStringFromDatabase("GivenCredentials");
            this.GivenSpecialty = providerDataTable.Rows[0].GetStringFromDatabase("GivenSpecialty");
            this.HaseRxPriv = providerDataTable.Rows[0].GetStringFromDatabase("HaseRxPriv");
            this.LastVisit = providerDataTable.Rows[0].GetDateTimeFromDatabase("LastVisit");
            this.LocCode = providerDataTable.Rows[0].GetStringFromDatabase("loccode");
            this.MUDate = providerDataTable.Rows[0].GetStringFromDatabase("MUDate");
            this.NPI = providerDataTable.Rows[0].GetStringFromDatabase("NPI");
            this.RegisterDate = providerDataTable.Rows[0].GetDateTimeFromDatabase("RegisterDate");
            this.Specialty = providerDataTable.Rows[0].GetStringFromDatabase("Specialty");
            this.Taxonomy = providerDataTable.Rows[0].GetStringFromDatabase("Taxonomy");
            this.TIN = providerDataTable.Rows[0].GetStringFromDatabase("TIN");
        }

        private void CreateNullProvider()
        {
            this.ProviderId = null;
            this.PracticeId = null;
            this.ClientId = null;
            this.IsActive = null; 
            this.FirstName = null;
            this.LastName = null;
            this.Zip = null;
            this.Phone = null;
            this.Email = null;
            this.Credentials = null;
            this.DivId = null;
            this.FirstVisit = null;
            this.GivenCredentials = null;
            this.GivenSpecialty = null;
            this.HaseRxPriv = null;
            this.LastVisit = null;
            this.LocCode = null;
            this.MUDate = null;
            this.NPI = null;
            this.RegisterDate = null;
            this.Specialty = null;
            this.Taxonomy = null;
            this.TIN = null;
        }

        public async Task<string> SendInfoToSalesForce(SFToken token)
        {
            HttpClient queryClient = new HttpClient();
            HttpRequestMessage request;

            // generate an authorization token
            //SalesForceHelper sfHelper = new SalesForceHelper();
            //SFToken token = await sfHelper.GenerateSFToken();

            string salesForceApiRoute = ConfigurationManager.AppSettings["ProviderApiRoute"].ToString();
            string restQuery = token.InstanceURL + salesForceApiRoute;

            request = new HttpRequestMessage(HttpMethod.Post, restQuery);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"ProviderId\":\"" + this.ProviderId.GetIntForApi() + "\"");
            sb.Append(",\"PracticeId\":\"" + this.PracticeId.GetIntForApi() + "\"");
            sb.Append(",\"ClientId\":\"" + this.ClientId.GetIntForApi() + "\"");
            sb.Append(",\"isActive\":\"" + this.IsActive.GetStringForApi() + "\""); 
            sb.Append(",\"FirstName\":\"" + this.FirstName.GetStringForApi() + "\"");
            sb.Append(",\"LastName\":\"" + this.LastName.GetStringForApi() + "\"");
            sb.Append(",\"Zip\":\"" + this.Zip.GetStringForApi() + "\"");
            sb.Append(",\"Phone\":\"" + this.Phone.GetStringForApi() + "\"");
            sb.Append(",\"email\":\"" + this.Email.GetStringForApi() + "\"");
            sb.Append(",\"Credentials\":\"" + this.Credentials.GetStringForApi() + "\"");
            sb.Append(",\"DivID\":\"" + this.DivId.GetIntForApi() + "\"");
            sb.Append(",\"FirstVisit\":\"" + this.FirstVisit.GetDateForApi() + "\"");
            sb.Append(",\"GivenCredentials\":\"" + this.GivenCredentials.GetStringForApi() + "\"");
            sb.Append(",\"GivenSpecialty\":\"" + this.GivenSpecialty.GetStringForApi() + "\"");
            sb.Append(",\"HaseRxPriv\":\"" + this.HaseRxPriv.GetStringForApi() + "\"");
            sb.Append(",\"LastVisit\":\"" + this.LastVisit.GetDateForApi() + "\"");
            sb.Append(",\"LocCode\":\"" + this.LocCode.GetStringForApi() + "\"");
            sb.Append(",\"MUDate\":\"" + this.MUDate.GetStringForApi() + "\"");
            sb.Append(",\"NPI\":\"" + this.NPI.GetStringForApi() + "\"");
            sb.Append(",\"RegisterDate\":\"" + this.RegisterDate.GetDateForApi() + "\"");
            sb.Append(",\"Specialty\":\"" + this.Specialty.GetStringForApi() + "\"");
            sb.Append(",\"Taxonomy\":\"" + this.Taxonomy.GetStringForApi() + "\"");
            sb.Append(",\"TIN\":\"" + this.TIN.GetStringForApi() + "\"");
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

            // read the result from the API
            string result = await response.Content.ReadAsStringAsync();

            string resultMessage = DateTime.Now.ToString("HH:mm:ss tt") + " | " + result + " | " + sb.ToString(); // Adding DEBUG information to output

            return resultMessage;
        }

    }
}
