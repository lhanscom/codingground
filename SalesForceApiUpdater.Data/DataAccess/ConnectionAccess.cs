using System;
using System.Configuration;

namespace SalesForceApiUpdater.Data
{
    public abstract class ConnectionAccess
    {

        protected string TrackingConnectionString
        {
            get 
            {
                return ConfigurationManager.ConnectionStrings["TrackingDatabase"].ConnectionString;
            }
        }

        protected string ClientDataConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["PQRSInfoDatabase"].ConnectionString;
            }
        }

        protected string AnalyticsDataConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["AnalyticsInfoDatabase"].ConnectionString;
            }
        }
    }
}
