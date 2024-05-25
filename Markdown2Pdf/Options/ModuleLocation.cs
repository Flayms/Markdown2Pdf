namespace Markdown2Pdf.Options;

/// <inheritdoc cref="ModuleOptions.ModuleLocation"/>
public enum ModuleLocation {
  /// <inheritdoc cref="ModuleOptions.None"/>
  None = 0,

  /// <inheritdoc cref="ModuleOptions.Remote"/>
  Remote,

  /// <inheritdoc cref="ModuleOptions.FromLocalPath(string)"/>
  Custom
}
