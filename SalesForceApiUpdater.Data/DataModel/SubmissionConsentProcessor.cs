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
    public class SubmissionConsentProcessor
    {
        public async Task<List<string>> ProcessSubmissionConsent()
        {
            HttpClient queryClient = new HttpClient();

            // generate an authorization token
            SalesForceHelper sfHelper = new SalesForceHelper();
            SFToken token = await sfHelper.GenerateSFToken();

            string salesForceApiRoute = ConfigurationManager.AppSettings["SalesForceSubmissionFormMethod"].ToString();
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
                    List<PracticeProviderConsent> permissionUpdates = JsonConvert.DeserializeObject<List<PracticeProviderConsent>>(result);
                    foreach (PracticeProviderConsent pc in permissionUpdates)
                    {
                        pc.UpdateProviderConsentStatus();
                        resultMessages.Add("Practice/Provider Consent Updated: PracticeId => " + pc.PracticeId + " | ProviderId => " + pc.ProviderId + " | SubYear => " + pc.SubYear + " | HasConsent => " + pc.HasConsent);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Failure Status Code Returned from API when attempting to get Permission Consent: {0}", result));
            }

            return resultMessages;
        }
    }
}
