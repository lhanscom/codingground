using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interview.Data.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Interview.Data.DataModel
{
    public class ProviderAudit : RemoteProcessorBase
    {
        private string _remoteApiRoute = ConfigurationManager.AppSettings["RemoteProviderAuditRoute"];

        public string[] PropertyNames { get; set; }

        public object[] PropertyValues { get; set; }

        public override string RemoteApiRoute
        {
            get { return _remoteApiRoute; }
            set { _remoteApiRoute = value; }
        }

        public override List<string> performProcess()
        {
            var results = new List<string>();
            try
            {
                HashSet<string> bannedPropertySet = GetBannedProperties();


                string message = "";
                int valueIndex = 0;               
                foreach (var propertyName in PropertyNames)
                {
                    var propertyValue = PropertyValues[valueIndex];
                    if (propertyValue == null)
                        if (bannedPropertySet.Contains(propertyName))
                            continue;
                        else
                        {
                            propertyValue = "null";
                        }


                    message += String.Format("\"{0}\":\"{1}\",", propertyName, propertyValue);
                    valueIndex++;
                }

                string apiMessage = message;

                results.Add(String.Format("{0:HH:mm:ss tt} | {1}", DateTime.Now, apiMessage));
                return results;
            }
            catch (Exception e)
            {
                results.Add(String.Format("Send Failed while processing ProviderAudit. Error: {0}", e.Message));
                return results;
            }
        }

        private HashSet<string> GetBannedProperties()
        {
            return  new HashSet<string>(new [] {"FileName"});
        }

        private static string GetApiRequestMesesage(Dictionary<string, string> jsonDict)
        {
            return JObject.FromObject(jsonDict).ToString();
        }
    }
}