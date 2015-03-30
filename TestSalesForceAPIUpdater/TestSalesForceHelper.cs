using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Interview.Data.RemoteApi;

namespace TestInterview
{
    internal class TestRemoteHelper : IRemoteHelper
    {
        private RemoteToken _remoteToken;
        public async Task<RemoteToken> GenerateRemoteToken()
        {
            try
            {
                return _remoteToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
   }
}