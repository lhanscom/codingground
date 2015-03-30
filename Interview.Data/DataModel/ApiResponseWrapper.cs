using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Interview.Data.RemoteApi;

namespace Interview.Data.DataModel
{
    public class ApiResponseWrapper : IApiResponseWrapper
    {
        public bool IsSuccessStatusCode { get; set; }
        public async Task<string> getResponse(IRemoteHelper _remote, string salesForceApiRoute, string apiMessage = "")
        {
            var queryClient = new HttpClient();

            // generate an authorization token
            var token = await _remote.GenerateRemoteToken();

            var restQuery = token.InstanceURL + salesForceApiRoute;

            var request = new HttpRequestMessage(HttpMethod.Get, restQuery);

            // add token to header
            request.Headers.Add("Authorization", "Bearer " + token.Token);

            // define what we expect back
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            var response = await queryClient.SendAsync(request);

            // read the result
            var result = await response.Content.ReadAsStringAsync();

            IsSuccessStatusCode = response.IsSuccessStatusCode;
            return result;
        }


        public async Task<string> Send(IRemoteHelper _remote, string salesForceApiRoute, string apiMessage)
        {
            var queryClient = new HttpClient();

            // generate an authorization token
            var token = await _remote.GenerateRemoteToken();

            var restQuery = token.InstanceURL + salesForceApiRoute;

            var request = new HttpRequestMessage(HttpMethod.Post, restQuery)
            {
                Content = new StringContent(apiMessage, Encoding.UTF8, "application/json")
            };

            // add token to header
            request.Headers.Add("Authorization", "Bearer " + token.Token);

            // define what we expect back
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //call endpoint async
            var response = await queryClient.SendAsync(request);

            // read the result
            var result = await response.Content.ReadAsStringAsync();

            IsSuccessStatusCode = response.IsSuccessStatusCode;
            return result;
        }
    }
}
