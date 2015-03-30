using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interview.Data.DataAccess;
using Interview.Data.DataModel;
using Interview.Data.RemoteApi;
using NLog;

namespace Interview.Console
{
    class Program
    {
        static IMingleWebDatabaseAccess mingleWebDataAccess;
        static List<String> applicationErrorLog;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            //System.Console.SetWindowSize(150, 60);
            _logger.Info("Remote Api Updater: Started ...");            

            applicationErrorLog = new List<String>();

            // Create instance of Data Service
            mingleWebDataAccess = new MingleWebDatabaseAccess();

            if (ConfigurationManager.AppSettings["ProcessUpdates"].ToUpper() == "TRUE")
            {
                // Get and process the event data from the ContentUpdateQue
                _logger.Info("Processing PQRS changes ...");
                var pqrsTask = GetEventData();
                pqrsTask.Wait();
                _logger.Info("All PQRS changes processed .... ");
            }
            
            // End processing of ContentUpdateQueue

            _logger.Info("Exiting application .... ");

            Environment.Exit(0);
        }

        private static async Task GetEventData()
        {
            var eventResultCollection = new List<EventResultInfoObject>();

            try
            {
                // Remove duplicate records from the ContentUpdateQue table. Keep the most recentProcessClientPaymentsEnvironment
                _logger.Info("Removing duplicate queue records ... ");
                var numberOfDuplicatedDeleted = mingleWebDataAccess.RemoveDuplicateQueueRecords();
                _logger.Info(numberOfDuplicatedDeleted + " duplicates removed");

                // Remove provider GPro records
                _logger.Info("Removing provider GPro records ... ");
                var numberOfProviderGProDeleted = mingleWebDataAccess.RemoveProviderGProRecordsFromQueue();
                _logger.Info(numberOfProviderGProDeleted + " provider GPro records removed");

                // get unprocessed record from the "tracking" database
                var dataTable = mingleWebDataAccess.GetUnprocessedEvents();

                // Generate a Remote Authentication Token
                _logger.Info("Obtaining Remote Authorization token ...");
                var sfHelper = new RemoteHelper();
                var authToken = await sfHelper.GenerateRemoteToken();

                var numberOfRows = dataTable.Rows.Count;
                var counter = 0;

                // loop through the records
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        counter++;
                        _logger.Info("Processing record " + counter + " of " + numberOfRows + ":");
                        var recordIdentifierPrefix = ("Queue ID (" + row["ID"] 
                                                         + ") | Altered Table (" + row["TableAltered"].ToString().ToUpper() 
                                                         + ") | Action (" + row["ActionPerformed"].ToString().ToUpper() 
                                                         + ") | Method (" + row["SubmissionMethod"].ToString().ToUpper() 
                                                         + "):");

                        
                        switch (row["TableAltered"].ToString().ToUpper())
                        {
                            case "CMSAUDIT":
                                await ProcessProviderAudit(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        var innerEx = ex.InnerException == null ? "" : ex.InnerException.Message;
                        var errMessage = "Application Error => (Record " + counter + " of " + numberOfRows + ") => " + ex.Message + 
                            " | " + innerEx + " | " + ex.Source;
                        applicationErrorLog.Add(errMessage);
                        _logger.Error(errMessage);
                        if (ConfigurationManager.AppSettings["ReprocessApplicationErrors"].ToUpper() == "FALSE")
                        {
                            mingleWebDataAccess.UpdateEventToDatabase((int)row["ID"], errMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException == null ? "" : ex.InnerException.Message;
                var errMessage = "Application Error => Error getting records from queue => " + ex.Message +
                            " | " + innerEx + " | " + ex.Source;
                applicationErrorLog.Add(errMessage);
                _logger.Info(errMessage);
            }
        }

        #region "Processors"
        private static async Task ProcessProviderAudit(DataRow changeRow, RemoteToken authToken, string recordIdentifierPrefix, ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessProviderAudits"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();

                var provAudit = new ProviderAudit
                {
                    PracticeId = (int) changeRow["PracticeId"],
                    PatientId = (int) changeRow["PatientId"],
                    SubYear = changeRow["SubYear"].ToString(),
                    MeasureNumber = changeRow["MeasNum"].ToString(),
                    MeasureConcept = changeRow["MeasureConcept"].ToString(),
                    AuditDescription = changeRow["AuditDescription"].ToString(),
                    PatientXId = changeRow["PatientIdentifier"].ToString(),
                    FileName = changeRow["AuditFileName"].ToString()
                };
                if (!DBNull.Value.Equals(changeRow["ProviderID"]))
                provAudit.ProviderId = (int)changeRow["ProviderId"];
                else
                {
                    provAudit.ProviderId = null;
                }

                if (!DBNull.Value.Equals(changeRow["FileReceivedDate"]))
                {
                    provAudit.FileReceivedDate = Convert.ToDateTime(changeRow["FileReceivedDate"]);
                }
                else
                {
                    provAudit.FileReceivedDate = null;
                }

                _logger.Info("==> Update Provider Audit => ");
                const string queueItemMessage = "<br/>==> Update Provider Audit => ";

                // send the provider audit info to Sales Force asynchronusly
                var apiResults = await provAudit.performProcess();
                var apiResult = apiResults[0];

                if (IsApiSendSuccessfull(apiResult))
                {
                    _logger.Info("Success");
                    erInfoObject.EventSucceeded = true;
                    erInfoObject.Message = recordIdentifierPrefix + "<br/>" + queueItemMessage + "Success" + "<br/>" + apiResult;
                    _logger.Info(erInfoObject.Message);
                }
                else
                {
                    _logger.Info("API Error");
                    erInfoObject.EventSucceeded = false;
                    erInfoObject.Message = recordIdentifierPrefix + "<br/>" + queueItemMessage + "API Error" + "<br/>" + apiResult;
                }
                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);

                if (!erInfoObject.EventSucceeded)
                    throw new Exception(erInfoObject.Message);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass ProviderAudit records ==> Skipping");
            }
        }
        #endregion


        private static bool IsApiSendSuccessfull(string apiResult)
        {
            var successRegex = new Regex(ConfigurationManager.AppSettings["SuccessMatchRegEx"]);

            return successRegex.IsMatch(apiResult);
        }
    }
}
