using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.DataModel;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace TestSalesForceAPIUpdater
{
    [TestClass]
    public class ProviderAuditTest
    {
        private async Task<string> cTask(string val)
        {
            return val;
        }

        [TestMethod]
        public void ProviderAuditTest_SendValidData()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();

            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = 8888,
                    PatientId = 77777777,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000, 2, 5),
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("\"PracticeId\":9999,\"ProviderId\":8888,\"PatientId\":77777777,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"MeasureConcept\":\"Measure Concept\",\"AuditDescription\":\"Audit Desciption\",\"PatientXID\":\"1a2b3c\",\"FileName\":\"TestFile.txt\",\"FileReceivedDate\":\"2000-02-05\""));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ProviderAuditTest_SendNullProvider()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = null,
                    PatientId = 77777777,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000, 2, 5),
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("\"PracticeId\":9999,\"PatientId\":77777777,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"MeasureConcept\":\"Measure Concept\",\"AuditDescription\":\"Audit Desciption\",\"PatientXID\":\"1a2b3c\",\"FileName\":\"TestFile.txt\",\"FileReceivedDate\":\"2000-02-05\""), ret);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ProviderAuditTest_SendNullPatientId()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = 8888,
                    PatientId = null,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000, 2, 5),
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("Send Failed while processing ProviderAudit. Error: Cannot write a null value for property 'PatientId'. Property requires a value. Path ''."));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ProviderAuditTest_SendNullSubYear()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = 8888,
                    PatientId = 77777777,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000, 2, 5),
                    MeasureNumber = "2"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("Send Failed while processing ProviderAudit. Error: Cannot write a null value for property 'subYear'. Property requires a value. Path ''."));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void ProviderAuditTest_SendNullReceivedDate()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = 8888,
                    PatientId = 77777777,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = null,
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("\"PracticeId\":9999,\"ProviderId\":8888,\"PatientId\":77777777,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"MeasureConcept\":\"Measure Concept\",\"AuditDescription\":\"Audit Desciption\",\"PatientXID\":\"1a2b3c\",\"FileName\":\"TestFile.txt\""));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ProviderAuditTest_SendXidWithColon()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();

            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ProviderAudit()
                {
                    SalesForce = sfHelp,
                    //ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    PracticeId = 9999,
                    ProviderId = 8888,
                    PatientId = 77777777,
                    MeasureConcept = "Measure Concept",
                    AuditDescription = "Audit Desciption",
                    PatientXId = "XID:1a2b3c",
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000, 2, 5),
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("{\"PracticeId\":9999,\"ProviderId\":8888,\"PatientId\":77777777,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"MeasureConcept\":\"Measure Concept\",\"AuditDescription\":\"Audit Desciption\",\"PatientXID\":\"XID-1a2b3c\",\"FileName\":\"TestFile.txt\",\"FileReceivedDate\":\"2000-02-05\"}"));
                Assert.IsTrue(ret.Contains("Error: ProviderAudit:Could not process because the Practice Id was not found.  Practice ID: 9999"));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
