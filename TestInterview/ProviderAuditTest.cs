using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Interview.Data.DataAccess;
using Interview.Data.DataModel;
using Interview.Data.RemoteApi;

namespace TestInterview
{
    [TestClass]
    public class ProviderAuditTest
    {
        private IRemoteHelper _remoteHelp;
        private Mock<IApiResponseWrapper> apiWrap;
        private Mock<IMingleWebDatabaseAccess> db;

        

        private async Task<string> cTask(string val)
        {
            return val;
        }

        [TestInitialize]
        public void Test_Setup()
        {
            var remoteHelp = new Mock<IRemoteHelper>();
            remoteHelp.Setup(t => t.GenerateRemoteToken()).ReturnsAsync(new RemoteToken() { InstanceURL = "", Token = "" });
            _remoteHelp = remoteHelp.Object;

            db = new Mock<IMingleWebDatabaseAccess>();
            apiWrap = new Mock<IApiResponseWrapper>();

            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));

            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));

            apiWrap.Setup(t => t.IsSuccessStatusCode)
                .Returns(false);
        }

        [TestMethod]
        public void ProviderAuditTest_SendValidData()
        {
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            try
            {
                var x = new ProviderAudit()
                {
                    Remote = _remoteHelp,
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
        public void AuditProvider_Should_Not_Serialize_ProviderId()
        {
            var providerAudit = GetProviderAudit(); // this will start us off with a null providerId

            // perform the audit
            var response = providerAudit.performProcess();
            response.Wait();

            var responseValue = response.Result == null ? "" : response.Result[0];

            // Assert the results
            Assert.IsFalse(responseValue.Contains("ProviderId"), responseValue);

        }

        [TestMethod]
        public void ProviderAuditTest_SendNullPatientId()
        {
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var x = new ProviderAudit()
                {
                    Remote = _remoteHelp,
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
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var x = new ProviderAudit()
                {
                    Remote = _remoteHelp,
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
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(false);
            try
            {
                var x = new ProviderAudit()
                {
                    Remote = _remoteHelp,
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
            db = new Mock<IMingleWebDatabaseAccess>();
            apiWrap = new Mock<IApiResponseWrapper>();

            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>())).Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            try
            {
                var x = new ProviderAudit()
                {
                    Remote = _remoteHelp,
                    ApiResponseWrapper = apiWrap.Object,
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
                Assert.IsTrue(ret.Contains("{\"PracticeId\":9999,\"ProviderId\":8888,\"PatientId\":77777777,\"subYear\":\"2014\",\"MeasureNumber\":\"2\",\"MeasureConcept\":\"Measure Concept\",\"AuditDescription\":\"Audit Desciption\",\"PatientXID\":\"XID-1a2b3c\",\"FileName\":\"TestFile.txt\",\"FileReceivedDate\":\"2000-02-05\"}"), String.Format("API Message incorrect. Results: {0}", ret));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        #region Helpers
        private ProviderAudit GetProviderAudit()
        {
            var providerAudit = new ProviderAudit()
            {
                Remote = _remoteHelp,
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
            return providerAudit;
        }
        #endregion

    }
}
