using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace TestSalesForceAPIUpdater
{
    internal class TestSalesForceHelper : ISalesForceHelper
    {
        private SFToken _sfToken;
        public async Task<SFToken> GenerateSFToken()
        {
            try
            {
                return _sfToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SFToken GenerateSfTokenSync()
        {
            var authClient = new HttpClient();

            // set OAuth key and secret variables
            string sfdcConsumerKey = ConfigurationManager.AppSettings["SalesForceConsumerKey"];
            string sfdcConsumerSecret = ConfigurationManager.AppSettings["SalesForceSecretKey"];
            string sfdcUserName = ConfigurationManager.AppSettings["SalesForceUserName"];
            string sfdcPassword = ConfigurationManager.AppSettings["SalesForcePassword"];
            string sfdcToken = ConfigurationManager.AppSettings["SalesForceApiToken"];

            // create login password value
            string loginPassword = sfdcPassword + sfdcToken;

            // populate values into a post body
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", sfdcConsumerKey),
                new KeyValuePair<string, string>("client_secret", sfdcConsumerSecret),
                new KeyValuePair<string, string>("username", sfdcUserName),
                new KeyValuePair<string, string>("password", loginPassword)
            };

            HttpContent content = new FormUrlEncodedContent(postData);

            // Get token
            string authorizationUrl = ConfigurationManager.AppSettings["SalesForceAuthenticationUrl"];

            try
            {
                HttpResponseMessage message;
                HttpResponseMessage response = authClient.PostAsync(authorizationUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    message = response;
                }
                else
                {
                    throw new Exception(string.Format("Unable to create authorization token: {0}", authorizationUrl));
                }

                string responseString = message.Content.ReadAsStringAsync().Result;

                var m = JsonConvert.DeserializeObject<SFToken>(responseString);
                var ret = new SFToken {InstanceURL = m.InstanceURL, Token = m.Token};
                _sfToken = ret;
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}