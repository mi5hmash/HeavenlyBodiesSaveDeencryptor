using heavenly_bodies_save_deencryptor_cli.Helpers;
using heavenly_bodies_save_deencryptor_cli.Infrastructure;
using heavenly_bodies_save_deencryptor_cli.Models;
using Mi5hmasH.Logger;

namespace heavenly_bodies_save_deencryptor_cli;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    /// <summary>
    /// Creates a new ParallelOptions instance configured with the specified cancellation token and an optimal degree of parallelism for the current environment.
    /// </summary>
    /// <param name="cts">The CancellationTokenSource whose token will be used to support cancellation of parallel operations.</param>
    /// <returns>A ParallelOptions object initialized with the provided cancellation token and a maximum degree of parallelism based on the number of available processors.</returns>
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts)
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    /// <summary>
    /// Retrieves the full paths of all files within the specified directory and its subdirectories that match the
    /// primary file extension or have additional extensions appended.
    /// </summary>
    /// <param name="inputDir">The path to the directory in which to search for files. This directory must exist and cannot be null.</param>
    /// <returns>An array of strings containing the full paths of files that match the specified file extension or have
    /// additional extensions. The array is empty if no matching files are found.</returns>
    private static string[] GetFilesToProcess(string inputDir)
    {
        const SearchOption so = SearchOption.AllDirectories;
        return Directory.GetFiles(inputDir, "*.*", so);
    }

    /// <summary>
    /// Asynchronously decrypts all files in the specified input directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to decrypt.</param>
    /// <param name="password">The password used for decryption, if required.</param>
    /// <param name="cts">A CancellationTokenSource that can be used to cancel the decryption operation.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    public async Task DecryptFilesAsync(string inputDir, string? password, CancellationTokenSource cts)
        => await Task.Run(() => DecryptFiles(inputDir, password, cts));

    /// <summary>
    /// Decrypts all encrypted files in the specified input directory and saves the decrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be decrypted. Only files with the expected encrypted file extension are processed.</param>
    /// <param name="password">The password used for decryption, if required.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the decryption operation. If cancellation is requested, the method will stop processing remaining files.</param>
    public void DecryptFiles(string inputDir, string? password, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = GetFilesToProcess(inputDir);
        if (filesToProcess.Length == 0) return;
        // DECRYPT
        logger.LogInfo($"Decrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("decrypted");
        Directory.CreateDirectory(outputDir);
        // Create output folder structure
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Create a new deencryptor instance with the provided password
        var deencryptor = password is null ? new Es3Deencryptor() : new Es3Deencryptor(password);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var group = $"Task {ctr}";
                    var fileName = Path.GetFileName(filesToProcess[ctr]);

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }

                    // Try to decrypt file data
                    var saveDataFile = new SaveDataFile(deencryptor, data, true);
                    try
                    {
                        logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypting the [{fileName}] file...", group);
                        saveDataFile.DecryptFile();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to decrypt the file: {ex.Message}", group);
                        break; // Skip to the next file
                    }
                    // Try to save the decrypted file data
                    try
                    {
                        var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                        var outputData = saveDataFile.GetFileData();
                        File.WriteAllBytes(outputFilePath, outputData);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Decrypted the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously encrypts all files in the specified directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing the files to encrypt.</param>
    /// <param name="password">The password used for encryption, if required.</param>
    /// <param name="disableCompression">A boolean value indicating whether to disable compression during encryption.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the encryption operation. If cancellation is requested, the operation will terminate early.</param>
    /// <returns>A task that represents the asynchronous encryption operation.</returns>
    public async Task EncryptFilesAsync(string inputDir, string? password, bool disableCompression, CancellationTokenSource cts)
        => await Task.Run(() => EncryptFiles(inputDir, password, disableCompression, cts));

    /// <summary>
    /// Encrypts all files with the specified extension in the given input directory and saves the encrypted files to a new output directory.
    /// </summary>
    /// <param name="inputDir">The path to the directory containing files to be encrypted. Only files matching the required extension are processed.</param>
    /// <param name="password">The password used for encryption, if required.</param>
    /// <param name="disableCompression">A boolean value indicating whether to disable compression during encryption.</param>
    /// <param name="cts">A CancellationTokenSource used to cancel the encryption operation. If cancellation is requested, the process will terminate early.</param>
    public void EncryptFiles(string inputDir, string? password, bool disableCompression, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = GetFilesToProcess(inputDir);
        if (filesToProcess.Length == 0) return;
        // ENCRYPT
        logger.LogInfo($"Encrypting [{filesToProcess.Length}] files...");
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("encrypted");
        Directory.CreateDirectory(outputDir);
        // Create output folder structure
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Create a new deencryptor instance with the provided password
        var deencryptor = password is null ? new Es3Deencryptor(compressionEnabled:!disableCompression) : new Es3Deencryptor(password, !disableCompression);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var group = $"Task {ctr}";
                    var fileName = Path.GetFileName(filesToProcess[ctr]);

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }

                    // Try to encrypt file data
                    var saveDataFile = new SaveDataFile(deencryptor, data);
                    try
                    {
                        logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypting the [{fileName}] file...", group);
                        saveDataFile.EncryptFile();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to encrypt the file: {ex.Message}", group);
                        break; // Skip to the next file
                    }
                    // Try to save the encrypted file data
                    try
                    {
                        var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                        var outputData = saveDataFile.GetFileData();
                        File.WriteAllBytes(outputFilePath, outputData);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Encrypted the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }
}