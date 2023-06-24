namespace Markdown2Pdf.Options;
public class ModuleOptions {
  public ModuleLocation ModuleLocation { get;}

  public string? ModulePath { get; }

  private ModuleOptions(ModuleLocation moduleLocation, string? modulePath = null) {
    this.ModuleLocation = moduleLocation;
    ModulePath = modulePath;
  }

  /// <summary>
  /// Loads the node_modules over remote http-requests.
  /// </summary>
  /// <remarks>This option requires an internet connection.</remarks>
  public static ModuleOptions Remote => new(ModuleLocation.Remote);

  /// <summary>
  /// Loads the node_modules from the systems global npm node_module directory.
  /// </summary>
  public static ModuleOptions Global => new(ModuleLocation.Global);

  /// <summary>
  /// Loads the node_modules from the given path.
  /// </summary>
  /// <param name="modulePath">The path to the node_module directory.</param>
  public static ModuleOptions FromLocalPath(string modulePath) => new(ModuleLocation.Custom, modulePath);
}

public enum ModuleLocation {
  Remote,
  Global,
  Custom
}