using System.Reflection;

namespace Markdown2Pdf.Services;

/// <summary>
/// Helper service for properties.
/// </summary>
public static class PropertyService {

  /// <summary>
  /// Gets a static property from a type by name.
  /// </summary>
  /// <typeparam name="T">Type of the property as well as the type that contains it.</typeparam>
  /// <param name="propertyName">The name of the property to get.</param>
  /// <param name="propertyValue">The property value.</param>
  /// <returns>Wether the conversion was successful.</returns>
  public static bool TryGetPropertyValue<T>(string propertyName, out T propertyValue) {
    var property = typeof(T).GetProperty(propertyName,
    BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);

    if (property == null) {
      propertyValue = default!;
      return false;
    }

    propertyValue = (T)property.GetValue(null, null)!;
    return true;
  }
}
