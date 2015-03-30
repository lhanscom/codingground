using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.Enums
{
    using System.ComponentModel;
    public enum ApiType
    {
        [Description("Client")]
        Client = 1,

        [Description("Practice")]
        Practice,

        [Description("Provider")]
        Provider,

        [Description("Providers submitting via IM or MG as method")]
        ProviderIMMG,

        [Description("Providers submitting GR method")]
        ProviderGR
    }
}
