using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesForceApiUpdater.Data
{
    public static class DatabaseScripts
    {
        public static readonly string SqlGetAllEvents = "SELECT * FROM ContentUpdateQueue ORDER BY [DateTimeAddedToQueue] ASC";

        public static readonly string SqlGetUnprocessedEvents = @"SELECT * 
                                                                FROM ContentUpdateQueue 
                                                                WHERE [IsDeliveredToSalesForce] = 0 
                                                                AND 
                                                                    (
                                                                    [TableAltered] = 'ID_Client' 
                                                                    OR [TableAltered] = 'ID_Practice'
                                                                    OR [TableAltered] = 'ID_Provider' 
                                                                    OR [TableAltered] = 'SubConsent' 
                                                                    OR [TableAltered] = 'StepStatus' 
													                OR [TableAltered] = 'XML_Tracking'
                                                                    OR [TableAltered] = 'CMSAudit'
                                                                    ) 
                                                                ORDER BY [DateTimeAddedToQueue] ASC, 
                                                                CASE TableAltered 
                                                                    WHEN 'ID_Client' THEN 1 
                                                                    WHEN 'ID_Practice' THEN 2 
                                                                    WHEN 'ID_Provider' THEN 3 
                                                                    WHEN 'SubConsent' Then 4 
                                                                    WHEN 'StepStatus' Then 5
													                WHEN 'XML_Tracking' THEN 6
                                                                    WHEN 'CMSAudit' THEN 7
                                                                END";

        public static readonly string SqlUpdateSuccessfulProcessedEvent = "UPDATE ContentUpdateQueue " +
                                                                "SET [IsDeliveredToSalesForce] = 1, " +
                                                                "[DateTimeDeliveredToSalesForce] = GetDate(), " +
                                                                "[ResultMessage] = @ResultMessage " +
                                                                "WHERE [ID] = @ID";

        public static readonly string SqlUpdateFailedProcessedEvent = "UPDATE ContentUpdateQueue " +
                                                        "SET [IsDeliveredToSalesForce] = 0, " +
                                                        "[DateTimeDeliveredToSalesForce] = GetDate(), " +
                                                        "[ResultMessage] = @ResultMessage " +
                                                        "WHERE [ID] = @ID";

        public static readonly string SqlGetClientInfo = "SELECT TIN, OrgName, addr1, addr2, City, [State], Zip, ExecutiveFirst, ExecutiveLast, ExecutivePhone, ExecutiveEmail, " +
                                                         "PConFirst, Pconlast, pconrole, email, RegisterDate, phone, ID " +
                                                         "FROM id_client " +
                                                         "WHERE ID = @ID";

        public static readonly string SqlGetPracticeInfo = "SELECT [ID],[ClientID],[Name],[Abb],[XID],[SUbOf],[TIN],[addr1],[addr2],[City],[State],[Zip]," +
                                                           "[locCode],[PConFirst],[PConLast],[PConRole],[Phone],[email],[IsActive],[CreateDate],[LastUpDate] " +
                                                           "FROM ID_Practice " +
                                                           "WHERE ID = @ID";

        public static readonly string SqlGetProviderInfo = "SELECT prov.id, prov.PracticeID, prov.ClientID, prov.IsActive, sel.Methd, prov.FirstName, prov.LastName, prov.Zip, " +
                                                           "prov.Phone, prov.email, prov.Credentials, prov.DivID, prov.FirstVisit, prov.GivenCredentials, " +
                                                           "prov.GivenSpecialty, prov.HaseRxPriv, prov.LastVisit, prov.loccode, prov.MUDate, prov.NPI, " +
                                                           "prov.RegisterDate, prov.Specialty, prov.Taxonomy, prov.TIN " +
                                                           "FROM dbo.ID_Provider prov " +
                                                           "LEFT JOIN dbo.Sel_Measure sel ON prov.id = sel.ProviderID " + 
                                                           "WHERE prov.ID = @ID";

        public static readonly string SqlGetProviderInfoFromQueue = "SELECT RecordIdentifier, ProviderId, PracticeId, SubYear FROM ContentUpdateQueue";


        public static readonly string SqlGetProvidersInPractice = "SELECT * FROM dbo.ID_Provider WHERE PracticeID = @PracticeId";

        public static readonly string SqlGetPracticeIdFromSelectedMeasure = "SELECT PracticeId FROM sel_measure WHERE ID = @ID";

        public static readonly string SqlGetProviderYearComboFromSelectedMeasures = "SELECT COUNT(*) FROM sel_measure WHERE ProviderID = @ProviderId AND SubYear = @SubYear";

        public static readonly string SqlGetMeasureCodeFromMeasureNumber = "SELECT DISTINCT GrpCode FROM [pqrs].[MeasureGroup] WHERE gMeasNum = @MeasureGroupNumber";

        public static readonly string SqlRemoveDuplicateQueueRecords = "DELETE contentUpdateQueue " +
                                                                       "FROM contentUpdateQueue " +
                                                                       "LEFT OUTER JOIN (" +
                                                                        "SELECT MAX(ID) as RowId, TableAltered, ActionPerformed, " +
                                                                        "RecordIdentifier, SubmissionMethod, ClientId, PracticeId, " +
                                                                        "ProviderId, SubYear, Submitting, HasConsent, " +
                                                                        "Submitted, LastGeneratedDate, MedicareBatchId, GProRegId " +
                                                                        "FROM contentUpdateQueue " +
                                                                        "GROUP BY TableAltered, ActionPerformed, RecordIdentifier, SubmissionMethod, " +
                                                                        "ClientId, PracticeId, ProviderId, SubYear, Submitting, HasConsent, " +
                                                                        "Submitted, LastGeneratedDate, MedicareBatchId, GProRegId" +
                                                                        ") as KeepRows ON " +
                                                                        "contentUpdateQueue.ID = KeepRows.RowId " +
                                                                        "WHERE " +
                                                                        "KeepRows.RowId IS NULL AND contentUpdateQueue.IsDeliveredToSalesForce = 0";

        public static readonly string SqlRemoveProviderGProQueueRecords = "DELETE FROM ContentUpdateQueue WHERE ((SubmissionMethod = 'GREG') " + 
                                                                          "OR (SubmissionMethod = 'GR')) AND ProviderID is not null AND IsDeliveredToSalesForce = 0";

        public static readonly string SqlGetMeasuresForNonGProProvider = "SELECT DISTINCT Measnum FROM Sel_Measure WHERE ProviderID = @ProviderId AND SubYear = @SubmissionYear";

        public static readonly string SqlGetMeasuresForGProPractice = "SELECT DISTINCT Measnum FROM Sel_Measure " + 
                                                                      "WHERE PracticeID = @PracticeId AND ProviderID is NULL AND SubYear = @SubmissionYear";

		public static readonly string SqlUpdateProviderConsentStatus = "UPDATE SubConsent SET HasConsent = @HasConsent WHERE ProviderID = @ProviderId AND PracticeId = @PracticeId AND SubYear = @SubYear";

        public static readonly string SqlUpdatePracticeConsentStatus = "UPDATE SubConsent SET HasConsent = @HasConsent " + 
																	   "WHERE PracticeID = @PracticeId AND SubYear = @SubYear";

        public static readonly string SqlOverrideClientPaymentStatusClientDashboardv1 = @"MERGE INTO Analytics.cal.Step_Status T
                                                                                            USING
                                                                                            (SELECT 
	                                                                                            DISTINCT
	                                                                                            client.ID AS ClientId, 
	                                                                                            prac.ID AS PracticeId
                                                                                            FROM id_client client LEFT JOIN id_practice prac ON client.ID = prac.ClientID
                                                                                            WHERE client.id = @ClientId) AS S
                                                                                            ON (T.ClientId = S.ClientId) 
	                                                                                            AND (T.PracticeId = S.PracticeId) 
	                                                                                            AND SubYear = @SubYear
                                                                                            WHEN MATCHED THEN
	                                                                                            UPDATE SET Step8 = @HasPaid
                                                                                            WHEN NOT MATCHED THEN
	                                                                                            INSERT (ClientId, PracticeId, subyear, step8)
	                                                                                            VALUES (S.ClientId, S.PracticeId, @SubYear, @HasPaid);";

        public static readonly string SqlGetPracticesForClient = @"SELECT DISTINCT ID FROM ID_Practice WHERE ClientID = @ClientId";

        public static readonly string SqlUpsertClientPaymentOverride = @"BEGIN TRAN
                                                                           UPDATE cal.StepStatusOverride 
                                                                             SET StepValue = @HasPaid
                                                                           WHERE ClientID = @ClientId
                                                                             AND PracticeID = @PracticeId 
                                                                             AND SubYear = @SubYear 
                                                                             AND StepNumber = '9' 

                                                                           IF @@rowcount = 0
                                                                             BEGIN 
                                                                               INSERT INTO cal.StepStatusOverride
                                                                                 (ClientID, PracticeID, SubYear, StepNumber, StepValue) 
                                                                               VALUES 
                                                                                 (@ClientId, @PracticeId, @SubYear, '9', @HasPaid)
                                                                              END

                                                                           EXEC cal.CalculateStepStatus @ClientId, @PracticeId, @SubYear;
                                                                         COMMIT TRAN";
    }
}
