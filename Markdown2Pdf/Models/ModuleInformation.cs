using System.Collections.Generic;
using System.IO;

namespace Markdown2Pdf.Models;

internal class ModuleInformation(string remotePath, string nodePath) {

  public string NodePath { get; } = nodePath;
  public string RemotePath { get; } = remotePath;

  public static IReadOnlyDictionary<TKey, ModuleInformation> UpdateDic<TKey>(IReadOnlyDictionary<TKey, ModuleInformation> dicToUpdate, string path) {
    var updatedLocationMapping = new Dictionary<TKey, ModuleInformation>();

    foreach (var kvp in dicToUpdate) {
      var key = kvp.Key;
      var absoluteNodePath = Path.Combine(path, kvp.Value.NodePath);
      updatedLocationMapping[key] = new(kvp.Value.RemotePath, absoluteNodePath);
    }

    return updatedLocationMapping;
  }
}
