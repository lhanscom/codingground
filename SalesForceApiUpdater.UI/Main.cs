using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using SalesForceApiUpdater.Data.DataService;
using SalesForceApiUpdater.Data.DataModel;
using System.Text.RegularExpressions;

namespace SalesForceApiUpdater.UI
{
    public partial class Main : Form
    {

        private IMingleWebDataService mingleWebDataService;
        List<String> apiErrorLog;
        List<String> apiSuccessLog;
        List<String> applicationErrorLog;

        public Main()
        {
            InitializeComponent();

            apiErrorLog = new List<String>();
            apiSuccessLog = new List<String>();
            applicationErrorLog = new List<String>();

            this.mingleWebDataService = new MingleWebDataService();

            // Get and process the event data and then shutdown the app once the update was completed
            GetEventData().ContinueWith(t => Environment.Exit(0));
        }


        private async Task GetEventData()
        {
            try
            {
                // get unprocessed record from the "tracking" database
                DataTable dataTable = this.mingleWebDataService.GetUnprocessedEvents();

                // update the grid
                LoadDataGridView();

                string apiResult = string.Empty;
                string temp = string.Empty;
                string prefixTemp = string.Empty;

                // loop through the records
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        switch (row["TableAltered"].ToString().ToUpper())
                        {
                            case "ID_CLIENT":
                                // Create a ClientInfoObject variable
                                ClientInfoObject client;
                                if (row["TableAltered"].ToString().ToUpper() != "DELETE")
                                {
                                    // create new ClientInfoObject and load with data from database
                                    client = new ClientInfoObject((int)row["RecordIdentifier"]);
                                }
                                else
                                {
                                    // The client has been deleted. Sending -1 to the constructor causes all prperties to be set to null
                                    client = new ClientInfoObject(-1);
                                    client.ClientId = (int)row["RecordIdentifier"];
                                    client.IsActive = "N";
                                }
                                // send the client info to Sales Force asynchronusly
                                apiResult = await client.SendInfoToSalesForce();
                                prefixTemp = "Queue ID [" + row["ID"].ToString() + "] Client ID [" + row["RecordIdentifier"].ToString() + "]";
                                ParseApiResults(apiResult, prefixTemp);
                                listBoxApiResults.Items.Insert(0, apiResult);
                                break;
                            case "ID_PRACTICE":
                                PracticeInfoObject practice;
                                if (row["TableAltered"].ToString().ToUpper() != "DELETE")
                                {
                                    // create new PracticeInfoObject and load with data from database
                                    practice = new PracticeInfoObject((int)row["RecordIdentifier"]);

                                }
                                else
                                {
                                    // The practice has been deleted. Sending -1 to the constructor causes all prperties to be set to null
                                    practice = new PracticeInfoObject(-1);
                                    practice.PracticeId = (int)row["RecordIdentifier"];
                                    practice.IsActive = "N";
                                }
                                // send the practice info to Sales Force asynchronusly
                                apiResult = await practice.SendInfoToSalesForce();
                                prefixTemp = "Queue ID [" + row["ID"].ToString() + "] Practice ID [" + row["RecordIdentifier"].ToString() + "]";
                                ParseApiResults(apiResult, prefixTemp);
                                // Send the API result message to the List Box
                                listBoxApiResults.Items.Insert(0, apiResult);
                                break;
                            case "ID_PROVIDER":
                                ProviderInfoObject provider;
                                if (row["TableAltered"].ToString().ToUpper() != "DELETE")
                                {
                                    // create new ProviderInfoObject and load with data from database
                                    provider = new ProviderInfoObject((int)row["RecordIdentifier"]);
                                    provider.Method = null;
                                }
                                else
                                {
                                    // The provider was deleted. Sending -1 to the constructor causes all prperties to be set to null
                                    provider = new ProviderInfoObject(-1);
                                    provider.IsActive = "N";
                                }
                                // send the provider info to Sales Force asynchronusly
                                apiResult = await provider.SendInfoToSalesForce();
                                prefixTemp = "Queue ID [" + row["ID"].ToString() + "] Provider ID [" + row["RecordIdentifier"].ToString() + "]";
                                ParseApiResults(apiResult, prefixTemp);
                                // Send the API result message to the List Box
                                listBoxApiResults.Items.Insert(0, apiResult);
                                break;
                            case "SEL_MEASURE":
                                if (row["TableAltered"].ToString().ToUpper() != "DELETE")
                                {
                                    if ((row["SubmissionMethod"].ToString().ToUpper() == "IM") || (row["SubmissionMethod"].ToString().ToUpper() == "MG"))
                                    {
                                        // Obtain the Provider ID of the provider being modified and send the information to the API
                                        int providerId = (int)row["ProviderId"];
                                        ProviderInfoObject selMeasureProvider = new ProviderInfoObject(providerId);
                                        selMeasureProvider.SubYear = row["SubYear"].ToString();
                                        apiResult = await selMeasureProvider.SendInfoToSalesForce();
                                        prefixTemp = "Queue ID [" + row["ID"].ToString() + "] Provider ID [" + providerId.ToString() + "]";
                                        ParseApiResults(apiResult, prefixTemp);
                                        listBoxApiResults.Items.Insert(0, apiResult);
                                    }
                                    else // Group Registry
                                    {
                                        // Send single Provider API call with all field NULL with the exception of Method which will be 'GR'
                                        ProviderInfoObject selMeasureProvider = new ProviderInfoObject(-1);
                                        selMeasureProvider.ProviderId = null;
                                        selMeasureProvider.PracticeId = (int)row["PracticeId"];
                                        selMeasureProvider.ClientId = (int)row["ClientId"];
                                        selMeasureProvider.SubYear = row["SubYear"].ToString();
                                        selMeasureProvider.Method = "GR";
                                        selMeasureProvider.FirstName = null;
                                        selMeasureProvider.LastName = null;
                                        selMeasureProvider.Zip = null;
                                        selMeasureProvider.Phone = null;
                                        selMeasureProvider.Email = null;
                                        selMeasureProvider.Credentials = null;
                                        selMeasureProvider.DivId = null;
                                        selMeasureProvider.FirstVisit = null;
                                        selMeasureProvider.GivenCredentials = null;
                                        selMeasureProvider.GivenSpecialty = null;
                                        selMeasureProvider.HaseRxPriv = null;
                                        selMeasureProvider.LastVisit = null;
                                        selMeasureProvider.LocCode = null;
                                        selMeasureProvider.MUDate = null;
                                        selMeasureProvider.NPI = null;
                                        selMeasureProvider.RegisterDate = null;
                                        selMeasureProvider.Specialty = null;
                                        selMeasureProvider.Taxonomy = null;
                                        selMeasureProvider.TIN = null;
                                        apiResult = await selMeasureProvider.SendInfoToSalesForce();
                                        prefixTemp = "Queue ID [" + row["ID"].ToString() + "] GPRO Provider ID [" + row["PracticeId"].ToString() + "]";
                                        ParseApiResults(apiResult, prefixTemp);
                                        listBoxApiResults.Items.Insert(0, apiResult);
                                    }
                                }
                                else
                                {
                                    // The sel_measure record was deleted
                                    // Handle here in the future
                                }
                                break;
                        }

                        // Update the database with results of API call
                        // Currently these all do the same thing but in the future we may want to update the 
                        // database differently for each ActionPerformed type
                        switch (row["ActionPerformed"].ToString().ToUpper())
                        {
                            case "UPDATE":
                                this.mingleWebDataService.UpdateEventToDatabase((int)row["ID"], apiResult);
                                break;
                            case "INSERT":
                                this.mingleWebDataService.UpdateEventToDatabase((int)row["ID"], apiResult);
                                break;
                            case "DELETE":
                                this.mingleWebDataService.UpdateEventToDatabase((int)row["ID"], apiResult);
                                break;
                            default:
                                this.mingleWebDataService.UpdateEventToDatabase((int)row["ID"], apiResult);
                                break;
                        }

                        LoadDataGridView();
                    }
                    catch (Exception ex)
                    {
                        applicationErrorLog.Add(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                applicationErrorLog.Add("Error on DataRow : " + ex.Message);
            }

            // Send out alerts
            SendOutAlerts();
        }

        private void ParseApiResults(string apiResult, string identifierPrefix)
        {
            var successRegex = new Regex(ConfigurationManager.AppSettings["SuccessMatchRegEx"]);

            if (successRegex.IsMatch(apiResult))
            {
                apiSuccessLog.Add(identifierPrefix + " => " + apiResult);
            }
            else
            {
                apiErrorLog.Add(identifierPrefix + " => " + apiResult);
            }
        }

        private void SendOutAlerts()
        {
            if (ConfigurationManager.AppSettings["SendAlertOnApiError"].ToUpper() == "TRUE" && apiErrorLog.Count > 0)
            {
                SendAlert errorAlert = new SendAlert();
                errorAlert.Subject = "SalesForceUpdater API Error Log - " + ConfigurationManager.AppSettings["EnvironmentName"];
                errorAlert.MessagePrefix = "API errors";
                errorAlert.AlertCollection = apiErrorLog;
                errorAlert.SendEmail(ConfigurationManager.AppSettings["ApiErrorEmailAddressToNotify"]);
            }
            if (ConfigurationManager.AppSettings["SendAlertOnApiSuccess"].ToUpper() == "TRUE" && apiSuccessLog.Count > 0)
            {
                SendAlert successAlert = new SendAlert();
                successAlert.Subject = "SalesForceUpdater API Success Log - " + ConfigurationManager.AppSettings["EnvironmentName"];
                successAlert.MessagePrefix = "Successful updates";
                successAlert.AlertCollection = apiSuccessLog;
                successAlert.SendEmail(ConfigurationManager.AppSettings["SuccessEmailAddressToNotify"]);
            }
            if (ConfigurationManager.AppSettings["SendAlertOnApplicationError"].ToUpper() == "TRUE" && applicationErrorLog.Count > 0)
            {
                SendAlert applicationErrorAlert = new SendAlert();
                applicationErrorAlert.Subject = "SalesForceUpdater Application Error Log - " + ConfigurationManager.AppSettings["EnvironmentName"];
                applicationErrorAlert.MessagePrefix = "Application errors";
                applicationErrorAlert.AlertCollection = applicationErrorLog;
                applicationErrorAlert.SendEmail(ConfigurationManager.AppSettings["ApplicationErrorEmailAddressToNotify"]);
            }
        }

        private void LoadDataGridView()
        {
            DataTable data = mingleWebDataService.GetUnprocessedEvents();

            // Data grid view column setting            
            dataGridViewEvents.DataSource = data;
            dataGridViewEvents.DataMember = data.TableName;
            dataGridViewEvents.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dataGridViewEvents.Refresh();
        }

    }
}
