using System.Diagnostics;
using System.IO;
using System;

namespace Markdown2Pdf.Helper;

public class CommandLineHelper {

  public static string RunCommand(string commandToRun, string? workingDirectory = null) {
    if (string.IsNullOrEmpty(workingDirectory))
      workingDirectory = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

    var processStartInfo = new ProcessStartInfo() {
      FileName = "cmd",
      RedirectStandardOutput = true,
      RedirectStandardInput = true,
      WorkingDirectory = workingDirectory
    };

    var process = Process.Start(processStartInfo);

    if (process == null)
      throw new Exception("Process should not be null.");

    process.StandardInput.WriteLine($"{commandToRun} & exit");
    process.WaitForExit();

    var output = process.StandardOutput.ReadToEnd();
    return output;
  }
}
