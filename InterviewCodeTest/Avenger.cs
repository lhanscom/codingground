using System;
using System.Collections.Generic;

namespace InterviewCodeTest
{
    public class Avenger
    {
        public string[] PropertyNames { get; set; }

        public object[] PropertyValues { get; set; }

        public string Serialize()
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
        private static HashSet<string> GetNonSerializableProperties()
        {
            return  new HashSet<string>(new []
            {
                "FileName",
                "NemesisName"
            });
        }

    }
}