using System;
using System.Collections.Generic;
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

        /// <summary>
        /// We need to serialize some data to send to a web api. That
        /// Web api is cranky and doesn't like it sometimes when we 
        /// serialize a property with a value of null. These tests make
        /// sure that we serialize our properties correctly.
        /// </summary>

        [TestMethod]
        public void Serialize_Should_Serialize_Property_When_Property_Is_Null()
        {
            // Arrange
            Avenger testObj = GetAvenger();

            // Act
            List<string> result = testObj.Serialize();

            // Assert
            string firstResult = result == null ? "" : result[0];
            Assert.IsTrue(firstResult.Contains("\"ArmorId\":\"null\""));
        }

        [TestMethod]
        public void Serialize_Should_Not_Serialize_WeaponId_When_Value_Is_Null()
        {
            // Arrange
            Avenger testObj = GetAvenger();

            // Act
            List<string> result = testObj.Serialize();

            // Assert
            string firstResult = result == null ? "" : result[0];
            Assert.IsFalse(firstResult.Contains("WeaponId"));
        }

        [TestMethod]
        public void Serialize_PatientId_Is_Mapped_Correctly()
        {
            // Arrange
            Avenger testObj = GetAvenger();

            // Act
            List<string> result = testObj.Serialize();

            // Assert
            string firstResult = result == null ? "" : result[0];
            Assert.IsTrue(firstResult.Contains("\"EnemyId\":\"24\""));
        }

        [TestMethod]
        public void Serialize_FileName_Is_Mapped_Correctly()
        {
            // Arrange
            Avenger testObj = GetAvenger();

            // Act
            List<string> result = testObj.Serialize();

            // Assert
            string firstResult = result == null ? "" : result[0];
            Assert.IsTrue(firstResult.Contains("\"FileName\":\"FileName.txt\""));
        }


        #region Helpers
        private Avenger GetAvenger()
        {
            var providerAudit = new Avenger()
            {
                Remote = new Mock<RemoteHelper>().Object,
                ApiResponseWrapper = new ApiResponseWrapper(),
                DatabaseAccess = new MingleWebDatabaseAccess(),
                PropertyNames = new[] { "AvengerId", "ArmorId", "WeaponId", "EnemyId", "FileName" },
                PropertyValues = new object[] { 42, null, null, 24, "FileName.txt" }
            };
            return providerAudit;
        }


        #endregion

    }
}
