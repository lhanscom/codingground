using System;
using System.Data;
using System.Data.SqlClient;

namespace Interview.Data.DataAccess
{
    public class MingleWebDatabaseAccess: ConnectionAccess, IMingleWebDatabaseAccess
    {
        public DataTable GetAllEvents()
        {
            throw new NotImplementedException();
        }

        public DataTable GetUnprocessedEvents()
        {
            throw new NotImplementedException();
        }

        public bool UpdateEventToDatabase(int recordId, string message)
        {
            throw new NotImplementedException();
        }

        public DataTable GetClientData(int recordId)
        {
            throw new NotImplementedException();
        }

        public DataTable GetPracticeData(int recordId)
        {
            throw new NotImplementedException();
        }

        public DataTable GetProviderData(int practiceId)
        {
            throw new NotImplementedException();
        }

        public DataTable GetProvidersInPractice(int practiceId)
        {
            throw new NotImplementedException();
        }

        public bool IsProviderAndYearPresent(int providerId, string subYear)
        {
            throw new NotImplementedException();
        }

        public string GetMeasureGroupCodeFromNumber(int measureGroupNumber)
        {
            throw new NotImplementedException();
        }

        public int RemoveDuplicateQueueRecords()
        {
            throw new NotImplementedException();
        }

        public int RemoveProviderGProRecordsFromQueue()
        {
            throw new NotImplementedException();
        }

        public DataTable GetMeasuresForNonGProProvider(int providerId, string submissionYear)
        {
            throw new NotImplementedException();
        }

        public DataTable GetMeasuresForGProPractice(int practiceId, string submissionYear)
        {
            throw new NotImplementedException();
        }

        public void UpdateProviderConsentStatus(int practiceId, int providerId, string subYear, string hasConsent)
        {
            throw new NotImplementedException();
        }

        public void UpdatePracticeConsentStatus(int PracticeId, string subYear, string hasConsent)
        {
            throw new NotImplementedException();
        }

        public void UpdateXmlTracking(int xmlTrackingID, int practiceId, string subYear, string batchId, DateTime? submittedDate)
        {
            throw new NotImplementedException();
        }

        public void InsertXmlTracking(int clientId, int practiceId, string subYear)
        {
            throw new NotImplementedException();
        }

        public DataTable GetPracticesForClient(int clientId)
        {
            throw new NotImplementedException();
        }

        public void OverridePracticePaymentStatus(int clientId, int practiceId, int subYear, int hasPaid)
        {
            throw new NotImplementedException();
        }
    }
}
