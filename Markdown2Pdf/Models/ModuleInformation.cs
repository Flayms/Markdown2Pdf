namespace Markdown2Pdf.Models;

internal class ModuleInformation(string remotePath, string nodePath) {

  public string NodePath { get; } = nodePath;
  public string RemotePath { get; } = remotePath;
}
