using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Interview.Data.RemoteApi
{
    public interface IRemoteHelper
    {
        Task<RemoteToken> GenerateRemoteToken();
    }

    public class RemoteHelper : IRemoteHelper
    {
        public Task<RemoteToken> GenerateRemoteToken()
        {
            throw new NotImplementedException();
        }
    }
}