using System.IO;
using System.Linq;
using System.Reflection;

namespace Markdown2Pdf.Services;

internal class EmbeddedResourceService {

  private Assembly _currentAssembly = Assembly.GetAssembly(typeof(Markdown2PdfConverter));

  /// <summary>
  /// Loads the text content of an embedded resource in this <see cref="Assembly"/>.
  /// </summary>
  /// <param name="resourceName">The filename of the resource to load.</param>
  /// <returns>The text content of the resource.</returns>
  internal string GetResourceContent(string resourceName) {
    //todo: check if there's a better way to do this
    var searchPath = $".{resourceName}";
    var resourcePath = this._currentAssembly.GetManifestResourceNames().Single(n => n.EndsWith(searchPath));

    using var stream = this._currentAssembly.GetManifestResourceStream(resourcePath);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }

}
