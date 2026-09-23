using System.IO.Compression;
namespace GodTreeXOwais.Infrastructure;
public sealed class ExportService{
 public string Export(string projectRoot){var outDir=Path.Combine(projectRoot,".godtree","export");Directory.CreateDirectory(outDir);var zip=Path.Combine(outDir,$"GodTree-Handoff-{DateTime.Now:yyyyMMdd-HHmmss}.zip");ZipFile.CreateFromDirectory(Path.Combine(projectRoot,".godtree"),zip,CompressionLevel.Fastest,false);return zip;}
}
