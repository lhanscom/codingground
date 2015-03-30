using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SalesForceApiUpdater.Data;
using SalesForceApiUpdater.Data.DataAccess;
using SalesForceApiUpdater.Data.DataModel;
using SalesForceApiUpdater.Data.SalesForceApi;

namespace TestSalesForceAPIUpdater
{
    [TestClass]
    public class XMLTrackingProcessorTest
    {
        private async Task<string> cTask(string val)
        {
            return val;
        }
        
        
        [TestMethod]
        public void Test_BadResponse()
        {
            Test_BadResponse_Impl();
        }

        [TestMethod]
        public void Test_Null_SubmittedDate_Response()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            SFToken authToken = sfHelp.GenerateSfTokenSync();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[{\"XMLTrackingID\":1,\"subYear\":\"2014\",\"submittedDate\":null,\"PracticeId\":2952,\"medicareBatchId\":\"test\"},{\"XMLTrackingID\":1,\"subYear\":\"2014\",\"submittedDate\":\"2015-01-08T01:00:25.000Z\",\"PracticeId\":4361,\"medicareBatchId\":\"sdfgsf\"}]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);

            var proc = new XmlTrackingProcessor()
            {
                DatabaseAccess = db.Object,
                SalesForce = sfHelp,
                SalesForceApiRoute = "/services/apexrest/XMLFileSubmittedService/",
                ApiResponseWrapper = apiWrap.Object
            };
            try
            {
                var ret = proc.performProcess();
                ret.Wait();

                foreach (var s in ret.Result)
                {
                    Debug.Print(s);
                    Assert.IsFalse(s.StartsWith("Failure"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Test_Null_XmlTrackingID_Response()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            SFToken authToken = sfHelp.GenerateSfTokenSync();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[{\"XMLTrackingID\":0,\"subYear\":\"2014\",\"submittedDate\":\"2015-01-08T01:00:25.000Z\",\"PracticeId\":4361,\"medicareBatchId\":\"sdfgsf\"}]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);

            var proc = new XmlTrackingProcessor()
            {
                DatabaseAccess = db.Object,
                SalesForce = sfHelp,
                SalesForceApiRoute = "/services/apexrest/XMLFileSubmittedService/",
                ApiResponseWrapper = apiWrap.Object
            };
            try
            {
                var ret = proc.performProcess();
                ret.Wait();

                foreach (var s in ret.Result)
                {
                    Debug.Print(s);
                    Assert.IsTrue(s.StartsWith("Failure"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Test_Null_XmlFileReRequest_Response()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            SFToken authToken = sfHelp.GenerateSfTokenSync();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[{\"ClientId\":0,\"subYear\":\"2014\",\"PracticeId\":4361}]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);

            var proc = new XmlFileRequestProcessor()
            {
                DatabaseAccess = db.Object,
                SalesForce = sfHelp,
                SalesForceApiRoute = "/services/apexrest/XMLFileReRequested/",
                ApiResponseWrapper = apiWrap.Object
            };
            try
            {
                var ret = proc.performProcess();
                ret.Wait();

                foreach (var s in ret.Result)
                {
                    Debug.Print(s);
                    Assert.IsTrue(s.StartsWith("Failure"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Test_XmlFileReRequest_Response()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelp = new TestSalesForceHelper();
            SFToken authToken = sfHelp.GenerateSfTokenSync();
            var apiWrap = new Mock<IApiResponseWrapper>();
            apiWrap.Setup(t => t.getResponse(It.IsAny<ISalesForceHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cTask("[{\"ClientId\":1,\"subYear\":\"2014\",\"PracticeId\":1}]"));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);

            var proc = new XmlFileRequestProcessor()
            {
                DatabaseAccess = db.Object,
                SalesForce = sfHelp,
                SalesForceApiRoute = "/services/apexrest/XMLFileReRequested/",
                ApiResponseWrapper = apiWrap.Object
            };
            try
            {
                var ret = proc.performProcess();
                ret.Wait();

                foreach (var s in ret.Result)
                {
                    Debug.Print(s);
                    Assert.IsFalse(s.StartsWith("Failure"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        [TestMethod]
        public void Test_SendXMLTracking()
        {
            try
            {
                Test_SendXMLTracking_Impl();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private void Test_SendXMLTracking_Impl()
        {
            var sfHelp = new TestSalesForceHelper();
            try
            {
                SFToken authToken = sfHelp.GenerateSfTokenSync();
                var db = new Mock<IMingleWebDatabaseAccess>();
                var x = new XMLTrackingObject()
                {
                    DatabaseAccess = db.Object,
                    FileName = "TestFile.txt",
                    GeneratedDate = DateTime.Now,
                    PracticeId = 4361,
                    SubYear = "2014",
                    XMLTrackingID = 14
                };
                var result = x.SendXmlTracking(authToken);
                result.Wait();
                var ret = result.Result;
                Assert.IsTrue(ret.Contains("Success"));
            }
            catch (Exception e)
            {                
                Assert.Fail(e.Message);
            }
        }
        private async void Test_BadResponse_Impl()
        {
            var db = new Mock<IMingleWebDatabaseAccess>();
            var sfHelper = new Mock<ISalesForceHelper>();

            var proc = new XmlTrackingProcessor()
            {
                DatabaseAccess = db.Object,
                SalesForce = sfHelper.Object,
                SalesForceApiRoute = "/services/apexrest/XMLFileSubmittedService/bad/"
            };
            try
            {
                await proc.performProcess();
                Assert.Fail("Exception should have been thrown");
            }
            catch (InvalidOperationException)
            {

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        private void Test_Response_Impl()
        {

        }
    }
}
