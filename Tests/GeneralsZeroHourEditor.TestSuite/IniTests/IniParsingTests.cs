using System.Text.Json;

namespace GeneralsZeroHourEditor.TestSuite.IniTests;

public class IniParsingTests
{
    [Fact]
    public void ParseSimpleObject_ReturnsExpectedJson()
    {
        // Arrange
        var dataService = new DataService();
        var tempFile = Path.GetTempFileName();
        var iniContent = @"
Object AmericaInfantryRanger
  Side = America
  EditorSorting = INFANTRY
  TransportSlotCount = 1
  WeaponSet
    Conditions = None
    Weapon = PRIMARY RangerLaserRifle
  End
End";
        File.WriteAllText(tempFile, iniContent);

        try
        {
            // Act
            var json = dataService.ParseIniToJson(tempFile);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Assert
            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            var obj = root.EnumerateArray().First();
            Assert.Equal("Object", obj.GetProperty("Type").GetString());

            var content = obj.GetProperty("Content");
            Assert.Contains(content.EnumerateArray(), e => e.GetProperty("Key").GetString() == "Side");

            var weaponSet = content.EnumerateArray().First(e => e.TryGetProperty("Type", out var t) && t.GetString() == "WeaponSet");
            Assert.Equal("WeaponSet", weaponSet.GetProperty("Type").GetString());
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
