﻿namespace Markdown2Pdf.Models;

internal class ModuleInformation {

  public string NodePath { get; }
  public string RemotePath { get; }

  public ModuleInformation(string remotePath, string nodePath) {
    this.NodePath = nodePath;
    this.RemotePath = remotePath;
  }
}
