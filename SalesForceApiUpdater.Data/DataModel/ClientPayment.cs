using SalesForceApiUpdater.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data.DataModel
{
    public class ClientPayment
    {
        public int ClientId { get; set; }
        public int SubYear { get; set; }
        public bool HasPaid { get; set; }

        public void UpdateClientPaymentStatus()
        {
            MingleWebDatabaseAccess da = new MingleWebDatabaseAccess();
            DataTable dtpractices = da.GetPracticesForClient(this.ClientId);

            foreach (DataRow row in dtpractices.Rows)
            {
                if (this.HasPaid)
                {
                    da.OverridePracticePaymentStatus(this.ClientId, (int)row["ID"], this.SubYear, 1);
                }
                else
                {
                    da.OverridePracticePaymentStatus(this.ClientId, (int)row["ID"], this.SubYear, 0);
                }
            }
        }
    }
}
