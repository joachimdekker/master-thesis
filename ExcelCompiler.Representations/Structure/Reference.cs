namespace ExcelCompiler.Domain.Structure;

public abstract record Reference
{
    public bool IsSingleReference => this is Location;

    public static Reference Parse(string reference, List<Spreadsheet>? spreadsheets = null)
    {
        // Get the spreadsheet
        string[] parts = reference.Split('!');
        string referenceWithoutSheet = parts[0];
        Spreadsheet? spreadsheet = null!;
        if (parts.Length == 2)
        {
            string spreadsheetName = parts[0];
            string referenceWithoutSpreadsheet = parts[1];
            spreadsheet = spreadsheets?.FirstOrDefault(s => s.Name == spreadsheetName);
        }
        
        // Very rough approximation for checking if the reference is a range or a single location
        if (referenceWithoutSheet.Contains(':'))
        {
            return Range.FromString(referenceWithoutSheet, spreadsheet);
        }
        else
        {
            return Location.FromA1(referenceWithoutSheet, spreadsheet);
        }
    }
}