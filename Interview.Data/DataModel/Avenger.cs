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
    public class Avenger : RemoteProcessorBase
    {
        private string _remoteApiRoute = ConfigurationManager.AppSettings["RemoteProviderAuditRoute"];

        public string[] PropertyNames { get; set; }

        public object[] PropertyValues { get; set; }

        public override string RemoteApiRoute
        {
            get { return _remoteApiRoute; }
            set { _remoteApiRoute = value; }
        }

        public override string Serialize()
        {
            try
            {
                HashSet<string> propertiesNotToSerializeWhenNull = GetNonSerializableProperties();

                string message = "";
                int valueIndex = 0;               
                foreach (string propertyName in PropertyNames)
                {
                    var propertyValue = PropertyValues[valueIndex];
                    if (propertyValue == null && propertiesNotToSerializeWhenNull.Contains(propertyName))
                        continue;

                    message += String.Format("\"{0}\":\"{1}\",", propertyName, propertyValue);
                    valueIndex++;
                }

                return String.Format("{0:HH:mm:ss tt} | {1}", DateTime.Now, message);
            }
            catch (Exception e)
            {
                return String.Format("Send Failed while processing Avenger. Error: {0}", e.Message);
            }
        }

        /// <summary>
        /// These properties should not be serialized when they contain a null value.
        /// </summary>
        /// <returns>A HashSet of property names</returns>
        private HashSet<string> GetNonSerializableProperties()
        {
            return  new HashSet<string>(new []
            {
                "FileName",
                "NemesisName"
            });
        }

    }
}