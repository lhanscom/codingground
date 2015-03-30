using System;
using System.Configuration;

namespace Interview.Data
{
    public abstract class ConnectionAccess
    {

        protected string TrackingConnectionString
        {
            get { return ""; }
        }

        protected string ClientDataConnectionString
        {
            get { return ""; }
        }

        protected string AnalyticsDataConnectionString
        {
            get { return ""; }
        }
    }
}
