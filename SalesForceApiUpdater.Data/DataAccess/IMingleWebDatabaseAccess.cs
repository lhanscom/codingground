using System;
using System.Data;
namespace SalesForceApiUpdater.Data.DataAccess
{
    public interface IMingleWebDatabaseAccess
    {
        DataTable GetAllEvents();

        DataTable GetUnprocessedEvents();

        bool UpdateEventToDatabase(int recordId, string message);

        DataTable GetClientData(int recordId);

        DataTable GetPracticeData(int recordId);

        DataTable GetProvidersInPractice(int practiceId);

        bool IsProviderAndYearPresent(int providerId, string subYear);

        string GetMeasureGroupCodeFromNumber(int measureGroupNumber);

        int RemoveDuplicateQueueRecords();

        int RemoveProviderGProRecordsFromQueue();

        DataTable GetMeasuresForNonGProProvider(int providerId, string submissionYear);

        DataTable GetMeasuresForGProPractice(int practiceId, string submissionYear);

        void UpdateProviderConsentStatus(int practiceId, int providerId, string subYear, string hasConsent);

        void UpdatePracticeConsentStatus(int PracticeId, string subYear, string hasConsent);
        void UpdateXmlTracking(int xmlTrackingID, int practiceId, string subYear, string batchId, DateTime? submittedDate);
        void InsertXmlTracking(int clientId, int practiceId, string subYear);
        DataTable GetPracticesForClient(int clientId);

        void OverridePracticePaymentStatus(int clientId, int practiceId, int subYear, int hasPaid);
    }
}
