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
    public class ClientClinicalAuditTest
    {
        private async Task<string> cTask(string val)
        {
            return val;
        }


        [TestMethod]
        public void ClientClinicalAuditTest_SendValidData()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();

            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((ISalesForceHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ClientClinicalAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    ClientId = 5015,
                    FileName = "TestFile.txt",
                    FileReceivedDate = new DateTime(2000,2,5),
                    MeasureNumber = "2",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("{\"ClientId\":5015,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"FileName\":\"TestFile.txt\",\"FileReceivedDate\":\"2000-02-05\""));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ClientClinicalAuditTest_SendNullMeasure()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[Success:]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ClientClinicalAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    ClientId = 5015,
                    FileName = "TestFile.txt",
                    FileReceivedDate = DateTime.Now,
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("Send Failed while processing ClientClinicalAudit. Error: Cannot write a null value for property 'MeasureNumber'. Property requires a value. Path ''."));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ClientClinicalAuditTest_SendNullFileName()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[Success:]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ClientClinicalAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    ClientId = 5015,
                    FileReceivedDate = DateTime.Now,
                    MeasureNumber = "1",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("Send Failed while processing ClientClinicalAudit. Error: Cannot write a null value for property 'FileName'. Property requires a value. Path ''."));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        public void ClientClinicalAuditTest_SendNullReceivedDate()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[Success:]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var authToken = sfHelp.GenerateSfTokenSync();
                var x = new ClientClinicalAudit()
                {
                    SalesForce = sfHelp,
                    ApiResponseWrapper = apiWrap.Object,
                    DatabaseAccess = db.Object,
                    ClientId = 5015,
                    MeasureNumber = "1",
                    FileName = "TestFile.txt",
                    SubYear = "2014"
                };

                var result = x.performProcess();
                result.Wait();
                var ret = result.Result == null ? "" : result.Result[0];
                Assert.IsTrue(ret.Contains("Send Failed while processing ClientClinicalAudit. Error: Cannot write a null value for property 'FileReceivedDate'. Property requires a value. Path ''."));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }

}
