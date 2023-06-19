namespace Markdown2Pdf; 

public class Markdown2PdfSettings {
  //todo: font-name
  //todo: font-size
  //todo: option for generating table of contents

  public string? HeaderUrl { get; set; }
  public string? FooterUrl { get; set; }
  public MarginOptions? MarginOptions { get; set; }
  public string? ChromePath { get; set; }
}