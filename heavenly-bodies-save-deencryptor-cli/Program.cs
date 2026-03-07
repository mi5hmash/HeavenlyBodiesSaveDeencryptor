using heavenly_bodies_save_deencryptor_cli;
using heavenly_bodies_save_deencryptor_cli.Helpers;
using Mi5hmasH.AppInfo;
using Mi5hmasH.ConsoleHelper;
using Mi5hmasH.Logger;
using Mi5hmasH.Logger.Providers;

#region SETUP

// CONSTANTS
const string breakLine = "---";

// Initialize APP_INFO
var appInfo = new MyAppInfo("heavenly_bodies_save_deencryptor_cli");

// Initialize LOGGER
var logger = new SimpleLogger
{
    LoggedAppName = appInfo.Name
};
// Configure ConsoleLogProvider
var consoleLogProvider = new ConsoleLogProvider();
logger.AddProvider(consoleLogProvider);
// Flush log providers on process exit
AppDomain.CurrentDomain.ProcessExit += (_, _) => logger.Flush();

// Initialize ProgressReporter
var progressReporter = new ProgressReporter(new Progress<string>(Console.WriteLine), null);

// Initialize CORE
var core = new Core(logger, progressReporter);

// Print HEADER
ConsoleHelper.PrintHeader(appInfo, breakLine);

// Say HELLO
ConsoleHelper.SayHello(breakLine);

// Get ARGUMENTS from command line
#if DEBUG
// For debugging purposes, you can manually set the arguments...
if (args.Length < 1)
{
    // ...below
    const string localArgs = "-m TEST";
    args = ConsoleHelper.GetArgs(localArgs);
}
#endif
var arguments = ConsoleHelper.ReadArguments(args);
#if DEBUG
// Write the arguments to the console for debugging purposes
ConsoleHelper.WriteArguments(arguments);
Console.WriteLine(breakLine);
#endif

#endregion

#region MAIN

// Show HELP if no arguments are provided or if -h is provided
if (arguments.Count == 0 || arguments.ContainsKey("-h"))
{
    PrintHelp();
    return;
}

// Optional argument: isVerbose
var isVerbose = arguments.ContainsKey("-v");

// Get MODE
arguments.TryGetValue("-m", out var mode);
switch (mode)
{
    case "decrypt" or "d":
        DecryptAll();
        break;
    case "encrypt" or "e":
        EncryptAll();
        break;
    default:
        throw new ArgumentException($"Unknown mode: '{mode}'.");
}

// EXIT the application
Console.WriteLine(breakLine); // print a break line
ConsoleHelper.SayGoodbye(breakLine);
#if DEBUG
ConsoleHelper.PressAnyKeyToExit();
#else
if (isVerbose) ConsoleHelper.PressAnyKeyToExit();
#endif

return;

#endregion

#region HELPERS

static void PrintHelp()
{
    var inputPath = Path.Combine(".", "InputDirectory");
    const string gameCode = "my-custom-password";
    var exeName = Path.Combine(".", Path.GetFileName(Environment.ProcessPath) ?? "ThisExecutableFileName.exe");
    var helpMessage = $"""
                       Usage: {exeName} -m <mode> [options]

                       Modes:
                         -m d  Decrypt SaveData files
                         -m e  Encrypt SaveData files

                       Options:
                         -p <input_folder_path>  Path to folder containing SaveData files
                         -g <code>               Game code to use for decryption/encryption (optional)
                         -nc                     Disables compression when encrypting (optional)
                         -v                      Verbose output
                         -h                      Show this help message

                       Examples:
                         Decrypt:  {exeName} -m d -p "{inputPath}"
                         Encrypt:  {exeName} -m e -p "{inputPath}"
                         Decrypt with custom game code:  {exeName} -m d -p "{inputPath}" -g "{gameCode}"
                         Encrypt without compression:  {exeName} -m e -p "{inputPath}" -nc
                       """;
    Console.WriteLine(helpMessage);
}

string GetValidatedInputRootPath()
{
    arguments.TryGetValue("-p", out var inputRootPath);
    if (File.Exists(inputRootPath)) inputRootPath = Path.GetDirectoryName(inputRootPath);
    return !Directory.Exists(inputRootPath)
        ? throw new DirectoryNotFoundException($"The provided path '{inputRootPath}' is not a valid directory or does not exist.")
        : inputRootPath;
}

#endregion

#region MODES

void DecryptAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    // Optional argument: gameCode
    arguments.TryGetValue("-g", out var gameCode);
    core.DecryptFiles(inputRootPath, gameCode, cts);
    cts.Dispose();
}

void EncryptAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    // Optional argument: disableCompression
    var disableCompression = arguments.ContainsKey("-nc");
    // Optional argument: gameCode
    arguments.TryGetValue("-g", out var gameCode);
    core.EncryptFiles(inputRootPath, gameCode, disableCompression, cts);
    cts.Dispose();
}

#endregion