using SalesForceApiUpdater.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class PracticeProviderConsent
    {
        public int ClientId { get; set; }
        public int PracticeId { get; set; }
        public int ProviderId { get; set; }
        public string SubYear { get; set; }
        public bool HasConsent { get; set; }

        public void UpdateProviderConsentStatus()
        {
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();

            if (this.ProviderId != 0)
            {
                if (this.HasConsent)
                {
                    da.UpdateProviderConsentStatus(this.PracticeId, this.ProviderId, this.SubYear, "Y");
                }
                else
                {
                    da.UpdateProviderConsentStatus(this.PracticeId, this.ProviderId, this.SubYear, "N");
                }
            }
            else
            {
                // This is a GPro practice. The ProviderId  = 0. Pass in PracticeId
                if (this.HasConsent)
                {
                    da.UpdatePracticeConsentStatus(this.PracticeId, this.SubYear, "Y");
                }
                else
                {
                    da.UpdatePracticeConsentStatus(this.PracticeId, this.SubYear, "N");
                }
            }

            // Call the stored procedure that updates StepStatus
            da.CalculateStepStatus(ClientId, PracticeId, SubYear);
        }
    }
}
