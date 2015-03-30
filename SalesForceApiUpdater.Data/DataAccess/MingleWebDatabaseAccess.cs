using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesForceApiUpdater.Data.DataAccess
{
    public class MingleWebDatabaseAccess: ConnectionAccess, IMingleWebDatabaseAccess
    {
        public DataTable GetAllEvents()
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.TrackingConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetAllEvents;
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }
        public DataTable GetUnprocessedEvents()
        {
            DataTable dt = new DataTable();
            using(SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.TrackingConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetUnprocessedEvents;
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public bool UpdateEventToDatabase(int recordId, string message)
        {
            using(SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.TrackingConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlUpdateSuccessfulProcessedEvent;
                sqlCommand.Parameters.AddWithValue("@ID", recordId);
                sqlCommand.Parameters.AddWithValue("@ResultMessage", message);
                sqlCommand.Connection.Open();
                var rowsAffected = sqlCommand.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool IsProviderAndYearPresent(int providerId, string subYear)
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlGetProviderYearComboFromSelectedMeasures;
                sqlCommand.Parameters.AddWithValue("@ProviderID", providerId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);
                sqlCommand.Connection.Open();
                int numRows = (int)sqlCommand.ExecuteScalar();
                return (numRows == 0) ? true : false;
            }
        }

        public DataTable GetClientData(int recordId)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetClientInfo;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@ID", recordId);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetPracticeData(int recordId)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetPracticeInfo;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@ID", recordId);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetProviderData(int recordId)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetProviderInfo;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@ID", recordId);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetProvidersInPractice(int practiceId)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetProvidersInPractice;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PracticeId", practiceId);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public string GetMeasureGroupCodeFromNumber(int measureGroupNumber)
        {
            string result = string.Empty;
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.AnalyticsDataConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlGetMeasureCodeFromMeasureNumber;
                sqlCommand.Parameters.AddWithValue("@MeasureGroupNumber", measureGroupNumber);

                sqlCommand.Connection.Open();
                var returnValue = sqlCommand.ExecuteScalar();
                if (returnValue != null)
                {
                    result = returnValue.ToString();
                }
            }
            return result;
        }

        public int RemoveDuplicateQueueRecords()
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.TrackingConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlRemoveDuplicateQueueRecords;
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public int RemoveProviderGProRecordsFromQueue()
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.TrackingConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlRemoveProviderGProQueueRecords;
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public DataTable GetMeasuresForNonGProProvider(int providerId, string submissionYear)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetMeasuresForNonGProProvider;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@ProviderId", providerId);
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@SubmissionYear", submissionYear);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public DataTable GetMeasuresForGProPractice(int practiceId, string submissionYear)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetMeasuresForGProPractice;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PracticeId", practiceId);
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@SubmissionYear", submissionYear);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public void UpdateProviderConsentStatus(int practiceId, int providerId, string subYear, string hasConsent)
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlUpdateProviderConsentStatus;
                sqlCommand.Parameters.AddWithValue("@PracticeID", practiceId);
                sqlCommand.Parameters.AddWithValue("@ProviderID", providerId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);
                sqlCommand.Parameters.AddWithValue("@HasConsent", hasConsent);
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }

        public void UpdatePracticeConsentStatus(int PracticeId, string subYear, string hasConsent)
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlUpdatePracticeConsentStatus;
                sqlCommand.Parameters.AddWithValue("@PracticeId", PracticeId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);
                sqlCommand.Parameters.AddWithValue("@HasConsent", hasConsent);
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }

        public DataTable GetPracticesForClient(int clientId)
        {
            DataTable dt = new DataTable();
            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand();
                sqlDataAdapter.SelectCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.Text;
                sqlDataAdapter.SelectCommand.CommandText = DatabaseScripts.SqlGetPracticesForClient;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@ClientId", clientId);
                sqlDataAdapter.Fill(dt);
            }

            return dt;
        }

        public void OverridePracticePaymentStatus(int clientId, int practiceId, int subYear, int hasPaid)
        {
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = DatabaseScripts.SqlUpsertClientPaymentOverride;
                sqlCommand.Parameters.AddWithValue("@ClientId", clientId);
                sqlCommand.Parameters.AddWithValue("@PracticeId", practiceId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);
                sqlCommand.Parameters.AddWithValue("@HasPaid", hasPaid);
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }

        public void UpdateXmlTracking(int xmlTrackingId, int practiceId, string subYear, string batchId, System.DateTime? submittedDate)
        {
            using (var sqlCommand = new SqlCommand("cal.UpdateXML_Tracking"))
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@PracticeId", practiceId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);
                if (submittedDate.HasValue)
                    sqlCommand.Parameters.AddWithValue("@SubmittedDate", submittedDate.Value);
                else
                    sqlCommand.Parameters.AddWithValue("@SubmittedDate", DBNull.Value);
                if (batchId == null)
                    sqlCommand.Parameters.AddWithValue("@BatchID", DBNull.Value);
                else
                    sqlCommand.Parameters.AddWithValue("@BatchId", batchId);
                sqlCommand.Parameters.AddWithValue("@XMLTrackingID", xmlTrackingId);
                
                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }


        public void InsertXmlTracking(int clientId, int practiceId, string subYear)
        {
            using (var sqlCommand = new SqlCommand("cal.InsertXML_Tracking"))
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@ClientID", clientId);
                sqlCommand.Parameters.AddWithValue("@PracticeId", practiceId);
                sqlCommand.Parameters.AddWithValue("@SubYear", subYear);

                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }

        public void CalculateStepStatus(int clientId, int practiceId, string subYear)
        {
            using (var sqlCommand = new SqlCommand("cal.CalculateStepStatus"))
            {
                sqlCommand.Connection = new SqlConnection(this.ClientDataConnectionString);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@pClientID", clientId);
                sqlCommand.Parameters.AddWithValue("@pPracticeID", practiceId);
                sqlCommand.Parameters.AddWithValue("@pSubYear", subYear);

                sqlCommand.Connection.Open();
                int rowsAffected = sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
