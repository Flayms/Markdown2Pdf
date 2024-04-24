namespace Markdown2Pdf.Options;

/// <summary>
/// Load modules from a local <i>node_module</i> directory.
/// </summary>
/// <param name="modulePath">Path to the node_module directory.</param>
internal class NodeModuleOptions(string modulePath) : ModuleOptions(ModuleLocation.Custom) {

  /// <summary>
  /// The path to the module directory.
  /// </summary>
  public string ModulePath { get; } = modulePath;
}
