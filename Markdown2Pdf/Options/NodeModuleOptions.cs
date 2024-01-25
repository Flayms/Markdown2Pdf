using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Markdown2Pdf.Services;

namespace Markdown2Pdf.Options;

/// <summary>
/// Load modules from a local node_module directory.
/// </summary>
/// <param name="modulePath">Path to the node_module directory.</param>
internal class NodeModuleOptions(string modulePath) : ModuleOptions(ModuleLocation.Custom) {

  /// <summary>
  /// The path to the module directory.
  /// </summary>
  public string ModulePath { get; } = modulePath;

  public static new NodeModuleOptions Global => new(_LoadGlobalModulePath());

  private static string _LoadGlobalModulePath() {
    // TODO: better error handling for cmd command
    var result = CommandLineHelper.RunCommand("npm list -g");
    var globalModulePath = Path.Combine(Regex.Split(result, "\r\n|\r|\n").First(), "node_modules");

    return !Directory.Exists(globalModulePath)
      ? throw new DirectoryNotFoundException($"Could not locate node_modules at \"{globalModulePath}\" - Directory doesn't exist.")
      : globalModulePath;
  }
}
