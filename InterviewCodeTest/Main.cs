using InterviewCodeTest;
using System;

class Program
{
    static void Main()
    {
        Console.Write("Serialize_Should_Serialize_Property_When_Property_Is_Null -> ");
        Serialize_Should_Serialize_Property_When_Property_Is_Null();
        Console.Write("Serialize_Should_Not_Serialize_WeaponId_When_Value_Is_Null -> ");
        Serialize_Should_Not_Serialize_WeaponId_When_Value_Is_Null();
        Console.Write("Serialize_Should_Serialize_EnemyId_To_Correct_Value -> ");
        Serialize_Should_Serialize_EnemyId_To_Correct_Value();
        Console.Write("Serialize_FileName_Is_Mapped_Correctly -> ");
        Serialize_FileName_Is_Mapped_Correctly();
        Console.Write("Serialize_Should_Not_Serialize_NemesisName_When_Value_Is_Null -> ");
        Serialize_Should_Not_Serialize_NemesisName_When_Value_Is_Null();

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    /// <summary>
    /// A contractor who has since been fired, has given us some 
    /// code that is broken. We wrote these unit tests to diagnose 
    /// the situation.
    /// 
    /// We need to serialize some data to send to a web api. That
    /// Web api is cranky and doesn't like it sometimes when we 
    /// serialize a property with a value of null. These tests make
    /// sure that we serialize our properties correctly.
    /// </summary>
    private static void Serialize_Should_Serialize_Property_When_Property_Is_Null()
    {
        // Arrange
        Avenger testObj = GetAvenger();

        // Act
        string result = testObj.Serialize();

        // Assert            
        Assert.IsTrue(result.Contains("\"ArmorId\":\"\""), "ArmorId was not serialized when it is supposed to be");
    }

    private static void Serialize_Should_Not_Serialize_WeaponId_When_Value_Is_Null()
    {
        // Arrange
        Avenger testObj = GetAvenger();

        // Act
        string result = testObj.Serialize();

        // Assert
        Assert.IsFalse(result.Contains("WeaponId"), "WeaponId should not be serialized");
    }

    private static void Serialize_Should_Serialize_EnemyId_To_Correct_Value()
    {
        // Arrange
        Avenger testObj = GetAvenger();

        // Act
        string result = testObj.Serialize();

        // Assert
        Assert.IsTrue(result.Contains("\"EnemyId\":\"24\""), "EnemyId was not serialized with the correct value!");
    }

    private static void Serialize_FileName_Is_Mapped_Correctly()
    {
        // Arrange
        Avenger testObj = GetAvenger();

        // Act
        string result = testObj.Serialize();

        // Assert
        Assert.IsTrue(result.Contains("\"FileName\":\"FileName.txt\""), "FileName was not serialized with the correct value!");
    }

    private static void Serialize_Should_Not_Serialize_NemesisName_When_Value_Is_Null()
    {
        // Arrange
        Avenger testObj = GetAvenger();

        // Act
        string result = testObj.Serialize();

        // Assert
        Assert.IsFalse(result.Contains("NemesisName"), "NemesisName should not be serialized");
    }



    #region Helpers
    private static Avenger GetAvenger()
    {
        var providerAudit = new Avenger()
        {
            PropertyNames = new[] { "AvengerId", "ArmorId", "WeaponId", "EnemyId", "FileName", "NemesisName" },
            PropertyValues = new object[] { 42, null, null, 24, "FileName.txt", null }
        };
        return providerAudit;
    }
    #endregion

}
/// <summary>
/// Added to eliminate the need for the Test Framework.
/// </summary>
internal static class Assert
{
    public static void IsFalse(bool test, string msg)
    {
        if (!test)
        {
            Console.WriteLine("Passed.");
        }
        else
        {
            Console.WriteLine("Failed.: {0}", msg);
        }
    }

    public static void IsTrue(bool test, string msg)
    {
        if (test)
        {
            Console.WriteLine("Passed.");
        }
        else
        {
            Console.WriteLine("Failed.: {0}", msg);
        }
    }
}