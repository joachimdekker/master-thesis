using System.Text;

namespace ExcelCompiler.Passes.Helpers;

public static class StringExtensions
{
    public static string ToPascalCase(this string str)
    {
        var lowerPascalCase = ToCamelCase(str);
        
        if (lowerPascalCase.Length == 0) return "";
        if (lowerPascalCase.Length == 1) return lowerPascalCase.ToUpper();
        
        return lowerPascalCase[0].ToString().ToUpper() + lowerPascalCase[1..];
    }
    
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        
        // Replace all invalid characters with spaces
        str = str.Replace("-", " ")
            .Replace(".", " ")
            .Replace("(", " ")
            .Replace(")", " ");
        
        StringBuilder sb = new();
        
        string[] words = str.Split(' ');
        
        var first = words[0];
        if (first.Length == 1) sb.Append(first.ToLower());
        else sb.Append(first[0].ToString().ToLower() + first[1..]);
        
        if (words.Length == 1) return sb.ToString();
        
        foreach (var word in words[1..])
        {
            if (word.Length == 0) continue;
            if (word.Length == 1) sb.Append(word.ToUpper());
            else sb.Append(word[0].ToString().ToUpper() + word[1..]);
        }

        return sb.ToString();
    }
}