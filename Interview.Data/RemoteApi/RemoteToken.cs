using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Interview.Data.RemoteApi
{
    // Represents a Sales Force token that is necessary to call API
    [DataContract]
    public class RemoteToken
    {
        #region Properties
        [DataMember(Name = "access_token")]
        public string Token { get; set; }

        [DataMember(Name = "instance_url")]
        public string InstanceURL { get; set; }

        #endregion
    }
}
