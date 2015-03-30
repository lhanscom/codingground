using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Interview.Data.DataModel;

namespace Interview.Data.Utility
{
    /* These extension methods handle values coming from the database that may contain null values.
     * Since all the values will be sent along an HTTP request they will all be sent as strings.
     * These extensions methods return strings in a format expected by the API
     */
    public static class ExtensionMethods
    {
        public static string GetStringForApi(this object input)
        {
            if (input != null)
            {
                var result = input.ToString().Replace(":", "-");
                result = result.Replace('\t',' ').Trim();
                result = result.Replace("\\", "\\\\");
                return result;
            }
            else
            {
                return "";
            }
        }

        public static string GetIntForApi(this object input)
        {
            if (input != null)
                return input.ToString().Trim();
            else
                return "";
        }

        public static string GetDateForApi(this object input)
        {
            if (input != null)
                return Convert.ToDateTime(input).ToString("yyyy-MM-dd");
            else
                return "";
        }

        public static string GetDateTimeForApi(this object input)
        {
            if (input != null)
                return Convert.ToDateTime(input).ToString("yyyy-MM-dd'T'HH:mm:ss");
            else
                return "";
        }

        // Extension methods for handling return from database
        public static string GetStringFromDatabase(this DataRow row, string columnName)
        {
            if (!DBNull.Value.Equals(row[columnName]))
            {
                var result = row[columnName].ToString();
                return result;
            }
            else
            {
                return null;
            }
        }

        public static int? GetIntFromDatabase(this DataRow row, string columnName)
        {
            return row.IsNull(columnName) ? (int?)null : (int)row[columnName];
        }

        public static DateTime? GetDateTimeFromDatabase(this DataRow row, string columnName)
        {
            if (!DBNull.Value.Equals(row[columnName]))
                return Convert.ToDateTime(row[columnName]);
            else
                return null;
        }

    }
}