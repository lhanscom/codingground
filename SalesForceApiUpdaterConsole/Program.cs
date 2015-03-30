using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.DataModel;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace SalesForceApiUpdater.Console
{
    class Program
    {
        static IMingleWebDatabaseAccess mingleWebDataAccess;
        static List<String> applicationErrorLog;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            //System.Console.SetWindowSize(150, 60);
            _logger.Info("SalesForce Api Updater: Started ...");            

            applicationErrorLog = new List<String>();

            // Create instance of Data Service
            mingleWebDataAccess = new MingleWebDatabaseAccess();

            if (ConfigurationManager.AppSettings["ProcessPQRSUpdates"].ToUpper() == "TRUE")
            {
                // Get and process the event data from the ContentUpdateQue
                _logger.Info("Processing PQRS changes ...");
                var pqrsTask = GetEventData();
                pqrsTask.Wait();
                _logger.Info("All PQRS changes processed .... ");
            }

            if (ConfigurationManager.AppSettings["ProcessConsentUpdates"].ToUpper() == "TRUE")
            {
                // Query SalesForce to see if any Practice/Provider Consent changes need to be brought down to the database
                _logger.Info("Processing Practice/Provider consent changes .... ");
                var practiceProviderConsentTask = GetPracticeProviderConsentUpdates();
                practiceProviderConsentTask.Wait();
                _logger.Info("All Practice/Provider consent changes processed .... ");
            }

            if (ConfigurationManager.AppSettings["ProcessClientPayments"].ToUpper() == "TRUE")
            {
                // Query SalesForce to see if any client payments need to be processsed
                _logger.Info("Processing Client payment changes ...");
                var clientPaymentTask = GetClientPaymentUpdates();
                clientPaymentTask.Wait();
                _logger.Info("All Client payment changes processed ...");
            }

            if (ConfigurationManager.AppSettings["ProcessXMLTrackingUpdates"].ToUpper() == "TRUE")
            {
                // Query SalesForce to see if any client payments need to be processsed
                _logger.Info("Processing XML Submission Tracking Changes ...");
                var xmlStatusUpdateTask = GetXmlStatusUpdates();
                xmlStatusUpdateTask.Wait();
                _logger.Info("All XML Submission Tracking changes processed ...");
            }

            if (ConfigurationManager.AppSettings["ProcessXMLFileReRequests"].ToUpper() == "TRUE")
            {
                // Query SalesForce to see if any client payments need to be processsed
                _logger.Info("Processing XML File Requests...");
                var tsk = GetXmlFileReRequests();
                tsk.Wait();
                _logger.Info("All XML File Requests processed ...");
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

                // Generate a SalesForce Authentication Token
                _logger.Info("Obtaining SalesForce Authorization token ...");
                var sfHelper = new SalesForceHelper();
                var authToken = await sfHelper.GenerateSFToken();

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
                            case "ID_CLIENT":
                                await ProcessID_Client(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                            case "ID_PRACTICE":
                                await ProcessID_Practice(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                            case "ID_PROVIDER":
                                await ProcessID_Provider(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                            case "SUBCONSENT":
                                await ProcessSubConsent(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                            case "STEPSTATUS":
                                await ProcessStepStatus(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
                            case "XML_TRACKING":
                                await ProcessXML_Tracking(row, authToken, recordIdentifierPrefix, eventResultCollection);
                                break;
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

            // Send out alerts
            SendOutAlerts(eventResultCollection, "PQRS Updates");
        }

        #region "Processors"
        private static async Task ProcessXML_Tracking(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix,
            ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessXMLTracking"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                var queueItemMessage = "";

                var xmlTracking = new XMLTrackingObject();

                var apiResult = "";
                if (!DBNull.Value.Equals(changeRow["PracticeID"]))
                {
                    xmlTracking.PracticeId = (int) changeRow["PracticeId"];
                    xmlTracking.SubYear = changeRow["SubYear"].ToString();
                    xmlTracking.FileName = changeRow["XMLGeneratedFileName"].ToString();
                    xmlTracking.GeneratedDate = Convert.ToDateTime(changeRow["XMLGenerated"]);
                    xmlTracking.XMLTrackingID = (int) changeRow["RecordIdentifier"];

                    _logger.Info(String.Format("==> Update XML Tracking for Practice {0} => ",
                        xmlTracking.PracticeId));
                    queueItemMessage = String.Format("<br/>==> Update XML Tracking for Practice {0} => ",
                        xmlTracking.PracticeId);

                    apiResult = await xmlTracking.SendXmlTracking(authToken);
                }
                if (IsApiSendSuccessfull(apiResult))
                {
                    _logger.Info("Success");
                    erInfoObject.EventSucceeded = true;
                    erInfoObject.Message = recordIdentifierPrefix + "<br/>" + queueItemMessage + "Success" + "<br/>" +
                                           apiResult;
                    _logger.Info(erInfoObject.Message);
                }
                else
                {                    
                    erInfoObject.EventSucceeded = false;
                    erInfoObject.Message = recordIdentifierPrefix + "<br/>" + queueItemMessage + "API Error" + "<br/>" +
                                           apiResult;
                    _logger.Error(erInfoObject.Message);
                }
                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int) changeRow["ID"], erInfoObject.Message);
            }

        }

        private static async Task ProcessStepStatus(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix,
            ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessStepStatus"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                var queueItemMessage = "";

                var stepStatus = new StepStatusInfoObject();

                var apiResult = "";
                if (!DBNull.Value.Equals(changeRow["PracticeID"]))
                {
                    stepStatus.PracticeId = (int)changeRow["PracticeId"];
                    stepStatus.SubYear = changeRow["SubYear"].ToString();

                    // Notice the unboxing and then conversion from byte to int. 
                    // This is because the TinyInt SQL datatype is actually stored as a byte. 
                    // Therefore we need to convert the byte value to an integer via (int)(byte)
                    // The use of (int?) is needed to allow the ternanry operator to function as 
                    // the return type must be defined if a NULL is returned. 
                    // See http://stackoverflow.com/questions/18260528/type-of-conditional-expression-cannot-be-determined-because-there-is-no-implicit

                    stepStatus.Step1 = (!DBNull.Value.Equals(changeRow["Step1Value"])) ? (int)(byte)changeRow["Step1Value"] : (int?)null;
                    stepStatus.Step2 = (!DBNull.Value.Equals(changeRow["Step2Value"])) ? (int)(byte)changeRow["Step2Value"] : (int?)null;
                    stepStatus.Step3 = (!DBNull.Value.Equals(changeRow["Step3Value"])) ? (int)(byte)changeRow["Step3Value"] : (int?)null;
                    stepStatus.Step4 = (!DBNull.Value.Equals(changeRow["Step4Value"])) ? (int)(byte)changeRow["Step4Value"] : (int?)null;
                    stepStatus.Step5 = (!DBNull.Value.Equals(changeRow["Step5Value"])) ? (int)(byte)changeRow["Step5Value"] : (int?)null;
                    stepStatus.Step6 = (!DBNull.Value.Equals(changeRow["Step6Value"])) ? (int)(byte)changeRow["Step6Value"] : (int?)null;
                    stepStatus.Step7 = (!DBNull.Value.Equals(changeRow["Step7Value"])) ? (int)(byte)changeRow["Step7Value"] : (int?)null;
                    stepStatus.Step8 = (!DBNull.Value.Equals(changeRow["Step8Value"])) ? (int)(byte)changeRow["Step8Value"] : (int?)null;
                    stepStatus.Step9 = (!DBNull.Value.Equals(changeRow["Step9Value"])) ? (int)(byte)changeRow["Step9Value"] : (int?)null;
                    stepStatus.Step10 = (!DBNull.Value.Equals(changeRow["Step10Value"])) ? (int)(byte)changeRow["Step10Value"] : (int?)null;

                    _logger.Info("==> Update Step Status for Practice " + stepStatus.PracticeId + " => ");
                    queueItemMessage = "<br/>==> Update Step Status for Practice " + stepStatus.PracticeId + " => ";

                    // send the provider info to Sales Force asynchronusly
                    apiResult = await stepStatus.SendStepStatus(authToken);
                }

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
                    _logger.Error(erInfoObject.Message);
                }
                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass StepStatus records ==> Skipping");
            }
        }

        private static async Task ProcessSubConsent(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix,
            ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessSelectedMethod"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                string queueItemMessage;

                var selectedMethod = new SelectedMethodInfoObject();

                string apiResult;
                if (!DBNull.Value.Equals(changeRow["ProviderId"]))
                {
                    // ProviderId is not NULL so send as Non-GPro
                    selectedMethod.PracticeId = (int)changeRow["PracticeId"];
                    selectedMethod.ProviderId = (int)changeRow["ProviderId"];
                    selectedMethod.SubYear = changeRow["SubYear"].ToString();
                    selectedMethod.Method = changeRow["SubmissionMethod"].ToString();
                    selectedMethod.Submitting = changeRow["Submitting"].ToString();
                    selectedMethod.HasConsent = changeRow["HasConsent"].ToString();
                    selectedMethod.ConsentBy = changeRow["ConsentBy"].ToString();
                    selectedMethod.Submitted = changeRow["Submitted"].ToString();
                    if (!DBNull.Value.Equals(changeRow["LastGeneratedDate"]))
                    {
                        selectedMethod.LastGeneratedDate = Convert.ToDateTime(changeRow["LastGeneratedDate"]);
                    }
                    else
                    {
                        selectedMethod.LastGeneratedDate = null;
                    }
                    selectedMethod.MedicareBatchId = changeRow["MedicareBatchId"].ToString();

                    _logger.Info("==> Update Method " + selectedMethod.Method + " => ");
                    queueItemMessage = "<br/>==> Update Method " + selectedMethod.Method + " => ";

                    // send the provider info to Sales Force asynchronusly
                    apiResult = await selectedMethod.SendMethodNonGPro(authToken);
                }
                else
                {
                    // ProviderId is NULL so send as GPro method
                    selectedMethod.PracticeId = (int)changeRow["PracticeId"];
                    selectedMethod.SubYear = changeRow["SubYear"].ToString();
                    selectedMethod.GProRegId = changeRow["GProRegId"].ToString();


                    _logger.Info("==> Update Method GPRo => ");
                    queueItemMessage = "<br/>==> Update Method GPro => ";

                    // send the provider info to Sales Force asynchronusly
                    apiResult = await selectedMethod.SendMethodGPro(authToken);
                }

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
                    _logger.Error(erInfoObject.Message);
                }
                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass SubConsent records ==> Skipping");
            }
        }

        private static async Task ProcessID_Provider(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix,
            ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessProvider"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                string queueItemMessage;

                ProviderInfoObject provider;
                if (changeRow["ActionPerformed"].ToString().ToUpper() != "DELETE")
                {
                    // create new ProviderInfoObject and load with data from database
                    provider = new ProviderInfoObject((int)changeRow["RecordIdentifier"]);
                    _logger.Info("==> Update Provider => ");
                    queueItemMessage = "<br/>==> Update Provider => ";
                }
                else
                {
                    // The provider was deleted. Sending -1 to the constructor causes all prperties to be set to null
                    provider = new ProviderInfoObject(-1)
                    {
                        ProviderId = (int) changeRow["RecordIdentifier"],
                        PracticeId = (int) changeRow["PracticeId"],
                        IsActive = "N"
                    };
                    _logger.Info("==> Delete Provider => ");
                    queueItemMessage = "<br/>==> Delete Provider => ";
                }

                // send the provider info to Sales Force asynchronusly
                var apiResult = await provider.SendInfoToSalesForce(authToken);

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
                    _logger.Error(erInfoObject.Message);
                }

                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass ID_Provider records ==> Skipping");
            }
        }

        private static async Task ProcessID_Practice(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix,
            ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessPractice"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                string queueItemMessage;

                PracticeInfoObject practice;
                if (changeRow["ActionPerformed"].ToString().ToUpper() != "DELETE")
                {
                    // create new PracticeInfoObject and load with data from database
                    practice = new PracticeInfoObject((int)changeRow["RecordIdentifier"]);
                    _logger.Info("==> Update Practice => ");
                    queueItemMessage = "<br/>==> Update Practice => ";
                }
                else
                {
                    // The practice has been deleted. Sending -1 to the constructor causes all prperties to be set to null
                    practice = new PracticeInfoObject(-1)
                    {
                        PracticeId = (int) changeRow["RecordIdentifier"],
                        IsActive = "N"
                    };
                    _logger.Info("==> Delete Practice => ");
                    queueItemMessage = "<br/>==> Delete Practice => ";
                }

                // send the practice info to Sales Force asynchronusly
                var apiResult = await practice.SendInfoToSalesForce(authToken);

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
                    _logger.Error(erInfoObject.Message);
                }
                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass ID_Practice records ==> Skipping");
            }
        }
        private static async Task ProcessID_Client(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix, ICollection<EventResultInfoObject> eventResultCollection)
        {
            if (ConfigurationManager.AppSettings["ProcessClient"].ToUpper() == "TRUE")
            {
                var erInfoObject = new EventResultInfoObject();
                string queueItemMessage;

                // Create a ClientInfoObject variable
                ClientInfoObject client;
                if (changeRow["ActionPerformed"].ToString().ToUpper() != "DELETE")
                {
                    // create new ClientInfoObject and load with data from database
                    client = new ClientInfoObject((int)changeRow["RecordIdentifier"]);
                    _logger.Info("==> Update Client => ");
                    queueItemMessage = "<br/>==> Update Client => ";
                }
                else
                {
                    // The client has been deleted. Sending -1 to the constructor causes all prperties to be set to null
                    client = new ClientInfoObject(-1)
                    {
                        ClientId = (int)changeRow["RecordIdentifier"],
                        IsActive = "N"
                    };
                    _logger.Info("==> Delete Client => ");
                    queueItemMessage = "<br/>==> Delete Client => ";
                }

                // send the client info to Sales Force asynchronusly
                var apiResult = await client.SendInfoToSalesForce(authToken);

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
                    _logger.Error(erInfoObject.Message);
                }

                // Add the results of the API call to the collection
                eventResultCollection.Add(erInfoObject);
                // Update the database with results of API call
                mingleWebDataAccess.UpdateEventToDatabase((int)changeRow["ID"], erInfoObject.Message);
            }
            else
            {
                _logger.Info("Application configured to bypass ID_Client records ==> Skipping");
            }            
        }

        private static async Task ProcessProviderAudit(DataRow changeRow, SFToken authToken, string recordIdentifierPrefix, ICollection<EventResultInfoObject> eventResultCollection)
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

        private static async Task GetXmlStatusUpdates()
        {
            var eventResultCollection = new List<EventResultInfoObject>();

            try
            {
                var p = new XmlTrackingProcessor();
                var results = await p.performProcess();
                foreach (var result in results)
                {
                    var erInfoObject = new EventResultInfoObject {EventSucceeded = true, Message = result};
                    // add the event to the collection
                    eventResultCollection.Add(erInfoObject);
                    _logger.Info("SalesForce XML Tracking Updates => " + result);
                }
            }
            catch (Exception ex)
            {
                var erInfoObject = new EventResultInfoObject
                {
                    EventSucceeded = false,
                    Message = ex.Message + " | " + ex.InnerException.Message + " | " + ex.Source
                };
                eventResultCollection.Add(erInfoObject);
                _logger.Info("SalesForce XML Tracking Update Error => " + ex.Message);
            }

            // Send out alerts
            SendOutAlerts(eventResultCollection, "XML Tracking Updates");            
        }
        private static async Task GetXmlFileReRequests()
        {
            var eventResultCollection = new List<EventResultInfoObject>();

            try
            {
                var p = new XmlFileRequestProcessor();
                var results = await p.performProcess();
                foreach (var result in results)
                {
                    var erInfoObject = new EventResultInfoObject {EventSucceeded = true, Message = result};
                    _logger.Error(erInfoObject.Message);
                    // add the event to the collection
                    eventResultCollection.Add(erInfoObject);
                    _logger.Info("SalesForce XML File ReRequest => " + result);
                }
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException == null ? "" : ex.InnerException.Message;
                var erInfoObject = new EventResultInfoObject
                {
                    EventSucceeded = false,
                    Message =
                        ex.Message + " | " + innerEx + " | " + ex.Source
                };
                _logger.Error(erInfoObject.Message);
                eventResultCollection.Add(erInfoObject);
            }

            // Send out alerts
            SendOutAlerts(eventResultCollection, "XML File ReRequests");
        }
        private static async Task GetPracticeProviderConsentUpdates()
        {
            var eventResultCollection = new List<EventResultInfoObject>();

            try
            {
                var sc = new SubmissionConsentProcessor();
                var results = await sc.ProcessSubmissionConsent();
                foreach (var result in results)
                {
                    var erInfoObject = new EventResultInfoObject
                    {
                        EventSucceeded = true,
                        Message = result
                    };
                    _logger.Info(erInfoObject.Message);
                    // add the event to the collection
                    eventResultCollection.Add(erInfoObject);                    
                }
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException == null ? "" : ex.InnerException.Message;
                var erInfoObject = new EventResultInfoObject
                {
                    EventSucceeded = false,
                    Message = ex.Message + " | " + innerEx + " | " + ex.Source
                };
                _logger.Error(erInfoObject.Message);
                eventResultCollection.Add(erInfoObject);                
            }

            // Send out alerts
            SendOutAlerts(eventResultCollection, "Practice/Provider Consent Updates");
        }

        private static async Task GetClientPaymentUpdates()
        {
            var eventResultCollection = new List<EventResultInfoObject>();

            try
            {
                var cp = new ClientPaymentProcessor();
                var results = await cp.ProcessClientPayment();
                foreach (var result in results)
                {
                    var erInfoObject = new EventResultInfoObject
                    {
                        EventSucceeded = true,
                        Message = result
                    };
                    _logger.Info(erInfoObject.Message);
                    // add the event to the collection
                    eventResultCollection.Add(erInfoObject);                    
                }
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException == null ? "" : ex.InnerException.Message;
                var erInfoObject = new EventResultInfoObject
                {
                    EventSucceeded = false,
                    Message = ex.Message + " | " + innerEx + " | " + ex.Source
                };
                _logger.Error(erInfoObject.Message);
                eventResultCollection.Add(erInfoObject);
            }

            // Send out alerts
            SendOutAlerts(eventResultCollection, "Client Payment Updates");
        }

        private static bool IsApiSendSuccessfull(string apiResult)
        {
            var successRegex = new Regex(ConfigurationManager.AppSettings["SuccessMatchRegEx"]);

            if (successRegex.IsMatch(apiResult))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void SendOutAlerts(List<EventResultInfoObject> resultCollection, string alertPrefix)
        {
            var apiErrorLog = new List<String>();
            var apiSuccessLog = new List<String>();

            foreach (var x in resultCollection)
            {
                if (!x.EventSucceeded)
                {
                    apiErrorLog.Add(x.Message);
                }
                else
                {
                    apiSuccessLog.Add(x.Message);
                }
            }

            if (ConfigurationManager.AppSettings["SendAlertOnApiError"].ToUpper() == "TRUE" && apiErrorLog.Count > 0)
            {
                var errorAlert = new SendAlert
                {
                    Subject =
                        alertPrefix + " Error Log | SalesForce: " +
                        ConfigurationManager.AppSettings["SalesForceEnvironmentName"] + " | Database: " +
                        ConfigurationManager.AppSettings["DatabaseEnvironmentName"],
                    MessagePrefix = alertPrefix + " error(s)",
                    AlertCollection = apiErrorLog
                };
                errorAlert.SendEmail(ConfigurationManager.AppSettings["ApiErrorEmailAddressToNotify"]);
            }
            if (ConfigurationManager.AppSettings["SendAlertOnApiSuccess"].ToUpper() == "TRUE" && apiSuccessLog.Count > 0)
            {
                var successAlert = new SendAlert
                {
                    Subject =
                        alertPrefix + " Success Log | SalesForce: " +
                        ConfigurationManager.AppSettings["SalesForceEnvironmentName"] + " | Database: " +
                        ConfigurationManager.AppSettings["DatabaseEnvironmentName"],
                    MessagePrefix = alertPrefix + " update(s)",
                    AlertCollection = apiSuccessLog
                };
                successAlert.SendEmail(ConfigurationManager.AppSettings["SuccessEmailAddressToNotify"]);
            }
            if (ConfigurationManager.AppSettings["SendAlertOnApplicationError"].ToUpper() == "TRUE" && applicationErrorLog.Count > 0)
            {
                var applicationErrorAlert = new SendAlert
                {
                    Subject =
                        "SalesForceUpdater Application Error Log | SalesForce: " +
                        ConfigurationManager.AppSettings["SalesForceEnvironmentName"] + " | Database: " +
                        ConfigurationManager.AppSettings["DatabaseEnvironmentName"],
                    MessagePrefix = " error(s) - " + alertPrefix,
                    AlertCollection = applicationErrorLog
                };
                applicationErrorAlert.SendEmail(ConfigurationManager.AppSettings["ApplicationErrorEmailAddressToNotify"]);
            }
        }
    }
}
