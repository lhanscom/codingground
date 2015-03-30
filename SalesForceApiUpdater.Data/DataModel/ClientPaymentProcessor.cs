using Newtonsoft.Json;
using SalesForceApiUpdater.Data.SalesForceApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class ClientPaymentProcessor
    {
        public async Task<List<string>> ProcessClientPayment()
        {
            HttpClient queryClient = new HttpClient();

            // generate an authorization token
            SalesForceHelper sfHelper = new SalesForceHelper();
            SFToken token = await sfHelper.GenerateSFToken();

            string salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceClientPaymentStatus"].ToString();
            string restQuery = token.InstanceURL + salesForceApiRoute;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restQuery);

            // add token to header
            request.Headers.Add("Authorization", "Bearer " + token.Token);

            // define what we expect back
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            HttpResponseMessage response = await queryClient.SendAsync(request);

            // read the result
            string result = await response.Content.ReadAsStringAsync();
            List<String> resultMessages = new List<String>();

            if (response.IsSuccessStatusCode)
            {
                if (result != "[]")
                {
                    List<ClientPayment> clientPayments = JsonConvert.DeserializeObject<List<ClientPayment>>(result);
                    foreach (ClientPayment cp in clientPayments)
                    {
                        cp.UpdateClientPaymentStatus();
                        resultMessages.Add("Client Payment Status Updated: ClientId => " + cp.ClientId + " | SubYear => " + cp.SubYear + " | HasPaid => " + cp.HasPaid);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Failure Status Code Returned from API when attempting to get Payment status: {0}", result));
            }

            return resultMessages;
        }
    }
}
