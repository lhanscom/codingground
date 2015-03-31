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
        public void AuditProvider_Should_Not_Serialize_ProviderId()
        {
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            var result = GetProviderAudit().performProcess();
            result.Wait();
            var ret = result.Result == null ? "" : result.Result[0];
            Assert.IsFalse(ret.Contains("ProviderId"));
        }

        [TestMethod]
        public void AuditProvider_PatientId_Is_Mapped_Correctly()
        {
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            var result = GetProviderAudit().performProcess();
            result.Wait();
            var ret = result.Result == null ? "" : result.Result[0];
            Assert.IsTrue(ret.Contains("\"PatientId\":\"24\""));
        }

        [TestMethod]
        public void AuditProvider_FileName_Is_Mapped_Correctly()
        {
            apiWrap.Setup(t => t.getResponse(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.Send(It.IsAny<IRemoteHelper>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRemoteHelper salesForce, string salesForceApiRoute, string apiMessage) => cTask(apiMessage));
            apiWrap.Setup(t => t.IsSuccessStatusCode).Returns(true);
            var result = GetProviderAudit().performProcess();
            result.Wait();
            var ret = result.Result == null ? "" : result.Result[0];
            Assert.IsTrue(ret.Contains("\"FileName\":\"FileName.txt\""));
        }


        #region Helpers
        private ProviderAudit GetProviderAudit()
        {
            var providerAudit = new ProviderAudit()
            {
                Remote = _remoteHelp,
                ApiResponseWrapper = apiWrap.Object,
                DatabaseAccess = db.Object,
                PropertyNames = new[] { "PracticeID", "ProviderId", "PatientId", "FileName" },
                PropertyValues = new object[] { 42, null, 24, "FileName.txt" }
            };
            return providerAudit;
        }


        #endregion

    }
}
