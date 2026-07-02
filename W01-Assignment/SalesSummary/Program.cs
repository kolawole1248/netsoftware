using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// --- Configuration ---
var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");
var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");

// --- Find all .json files ---
var salesFiles = FindFiles(storesDirectory);

// --- Display found files ---
Console.WriteLine($"Found {salesFiles.Count()} JSON files:");
foreach (var file in salesFiles)
{
    Console.WriteLine($"  - {file}");
}
Console.WriteLine();

// --- Calculate totals ---
var salesTotal = CalculateSalesTotal(salesFiles);

// --- Generate sales summary report ---
GenerateSalesSummary(salesFiles, salesTotal, Path.Combine(salesTotalDir, "totals.txt"));

Console.WriteLine($"✅ Report generated at: {Path.Combine(salesTotalDir, "totals.txt")}");
Console.WriteLine($"📊 Total Sales: {salesTotal.ToString("C")}");

// ==============================================
// FUNCTIONS
// ==============================================

IEnumerable<string> FindFiles(string folderName)
{
    List<string> jsonFiles = new List<string>();
    
    if (!Directory.Exists(folderName))
    {
        Console.WriteLine($"❌ Folder not found: {folderName}");
        return jsonFiles;
    }
    
    var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);
    
    foreach (var file in foundFiles)
    {
        if (Path.GetExtension(file) == ".json")
        {
            jsonFiles.Add(file);
        }
    }
    return jsonFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;
    
    foreach (var file in salesFiles)
    {
        try
        {
            string salesJson = File.ReadAllText(file);
            
            // Try to parse as JObject
            var jsonObj = JObject.Parse(salesJson);
            var total = jsonObj["total"];
            
            if (total != null)
            {
                double value = total.Value<double>();
                salesTotal += value;
                Console.WriteLine($"✅ Read {file}: ${value:F2}");
            }
            else
            {
                Console.WriteLine($"⚠️ No 'total' field in {file}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error reading {file}: {ex.Message}");
        }
    }
    
    return salesTotal;
}

void GenerateSalesSummary(IEnumerable<string> salesFiles, double grandTotal, string reportPath)
{
    // Create directory if it doesn't exist
    Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
    
    var details = new StringBuilder();
    
    foreach (var file in salesFiles)
    {
        try
        {
            string salesJson = File.ReadAllText(file);
            var jsonObj = JObject.Parse(salesJson);
            var total = jsonObj["total"];
            
            if (total != null)
            {
                double fileTotal = total.Value<double>();
                details.AppendLine($"  {Path.GetFileName(file)}: {fileTotal.ToString("C")}");
            }
        }
        catch (Exception ex)
        {
            details.AppendLine($"  {Path.GetFileName(file)}: ERROR - {ex.Message}");
        }
    }
    
    // Build the full report with StringBuilder
    var report = new StringBuilder();
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($" Total Sales: {grandTotal.ToString("C")}");
    report.AppendLine();
    report.AppendLine(" Details:");
    report.Append(details.ToString());
    
    // Write to file
    File.WriteAllText(reportPath, report.ToString());
}