namespace ExcelCompiler.Representations.Helpers;

public static class ListMatrixExtensions
{
    public static List<List<T>> Transpose<T>(this List<List<T>> list)
    {
        var rows = list.Count;
        var cols = list[0].Count;
        var transposed = new T[cols * rows];
        
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                transposed[j * rows + i] = list[i][j];
        
        // Return back to Matrix form
        return transposed
            .Chunk(rows)
            .Select(x => x.ToList())
            .ToList();
    }
}