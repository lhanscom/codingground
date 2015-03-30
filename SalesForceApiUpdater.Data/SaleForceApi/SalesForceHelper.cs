using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Net.Http;
using System.Configuration;

namespace SalesForceApiUpdater.Data.SalesForceApi
{
    public interface ISalesForceHelper
    {
        Task<SFToken> GenerateSFToken();
    }

    public class SalesForceHelper : ISalesForceHelper
    {
        public async Task<SFToken> GenerateSFToken()
        {
            HttpClient authClient = new HttpClient();

            // set OAuth key and secret variables
            string sfdcConsumerKey = ConfigurationManager.AppSettings["SalesForceConsumerKey"];
            string sfdcConsumerSecret = ConfigurationManager.AppSettings["SalesForceSecretKey"];
            string sfdcUserName = ConfigurationManager.AppSettings["SalesForceUserName"];
            string sfdcPassword = ConfigurationManager.AppSettings["SalesForcePassword"];
            string sfdcToken = ConfigurationManager.AppSettings["SalesForceApiToken"];

            // create login password value
            string loginPassword = sfdcPassword + sfdcToken;

            // populate values into a post body
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            postData.Add(new KeyValuePair<string, string>("client_id", sfdcConsumerKey));
            postData.Add(new KeyValuePair<string, string>("client_secret", sfdcConsumerSecret));
            postData.Add(new KeyValuePair<string, string>("username", sfdcUserName));
            postData.Add(new KeyValuePair<string, string>("password", loginPassword));

            HttpContent content = new FormUrlEncodedContent(postData);

            // Get token
            string authorizationUrl = ConfigurationManager.AppSettings["SalesForceAuthenticationUrl"];

            try
            {
                HttpResponseMessage message;                
                var response = await authClient.PostAsync(authorizationUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    message = response;
                }
                else
                {
                    throw new Exception(string.Format("Unable to create authorization token: {0}", authorizationUrl));
                }

                string responseString = await message.Content.ReadAsStringAsync();

                SFToken m = JsonConvert.DeserializeObject<SFToken>(responseString);

                return m;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
