using heavenly_bodies_save_deencryptor_cli;
using heavenly_bodies_save_deencryptor_cli.Helpers;
using heavenly_bodies_save_deencryptor_cli.Models;
using Mi5hmasH.Logger;

namespace QualityControl.xUnit;

public sealed class CoreTests : IDisposable
{
    private readonly Core _core;
    private readonly ITestOutputHelper _output;

    public CoreTests(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("SETUP");

        // Setup
        var logger = new SimpleLogger();
        var progressReporter = new ProgressReporter(null, null);
        _core = new Core(logger, progressReporter);
    }

    public void Dispose()
    {
        _output.WriteLine("CLEANUP");
    }

    [Fact]
    public void DecryptFiles_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            _core.DecryptFiles(tempDir, null, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void EncryptFiles_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            _core.EncryptFiles(tempDir, null, true, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }
    
    [Fact]
    public void DecryptFiles_DoesDecrypt()
    {
        // Arrange
        var deencryptor = new Es3Deencryptor();
        var saveDataFile = new SaveDataFile(deencryptor, Properties.Resources.encryptedFile, true);

        // Act
        saveDataFile.DecryptFile();
        var resultData = saveDataFile.GetFileData();

        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)resultData);
    }

    [Fact]
    public void EncryptFiles_DoesEncrypt()
    {
        // Arrange
        var deencryptor = new Es3Deencryptor();
        var saveDataFile = new SaveDataFile(deencryptor, Properties.Resources.decryptedFile);

        // Act
        saveDataFile.EncryptFile();
        saveDataFile.DecryptFile();
        var resultData = saveDataFile.GetFileData();

        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)resultData);
    }

    [Fact]
    public void DecryptFiles_UserProvidedWrongCode_DoesThrow()
    {
        // Arrange
        var deencryptor = new Es3Deencryptor("wrong-code");
        var saveDataFile = new SaveDataFile(deencryptor, Properties.Resources.encryptedFile, true);
        var testResult = false;
        
        // Act
        try
        {
            saveDataFile.DecryptFile();
        }
        catch
        {
            testResult = true;
        }
        
        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void DecryptFiles_UserProvidedRightCode_DoesDecrypt()
    {
        // Arrange
        var deencryptor = new Es3Deencryptor("ooh-heaven-is-a-place-on-earth");
        var saveDataFile = new SaveDataFile(deencryptor, Properties.Resources.encryptedFile, true);
        
        // Act
        saveDataFile.DecryptFile();
        var resultData = saveDataFile.GetFileData();

        // Assert
        Assert.Equal(Properties.Resources.decryptedFile, (ReadOnlySpan<byte>)resultData);
    }
}