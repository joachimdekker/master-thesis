namespace ExcelCompiler.Representations.Helpers;

public static class ArrayExtensions
{
    public static T[,] Transpose<T>(this T[,] array)
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var transposed = new T[cols, rows];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                transposed[j, i] = array[i, j];
            }
        }

        return transposed;
    }

    public static T[,] Transpose<T>(this IEnumerable<IEnumerable<T>> array)
    {
        List<List<T>> list = array.Select(x => x.ToList()).ToList();
        
        var rows = list.Count;
        var cols = list.First().Count;
        var transposed = new T[cols, rows];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                transposed[j, i] = list[j][i];
            }
        }

        return transposed;
    }

    public static TRes[,] Map<T, TRes>(this T[,] array, Func<T, TRes> func)
    {
        int height = array.GetLength(0);
        int width = array.GetLength(1);
        TRes[,] result = new TRes[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                result[i, j] = func(array[i, j]);
            }
        }

        return result;
    }

    // Adapted from https://stackoverflow.com/a/51241629
    public static T[] GetColumn<T>(this T[,] matrix, int columnNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(0))
            .Select(x => matrix[x, columnNumber])
            .ToArray();
    }

    public static T[] GetRow<T>(this T[,] matrix, int rowNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(1))
            .Select(x => matrix[rowNumber, x])
            .ToArray();
    }

    public static T[,] Copy<T>(this T[,] matrix, (int Start, int Stop)? selectedRows = null,  (int Start, int Stop)? selectedColumns = null)
    {
        int originalHeight = matrix.GetLength(0);
        int originalWidth = matrix.GetLength(1);
        
        (int Start, int Stop) rows = selectedRows.GetValueOrDefault((0, originalHeight));
        (int Start, int Stop) columns = selectedColumns.GetValueOrDefault((0, originalWidth));

        int height = rows.Stop - rows.Start;
        int width = columns.Stop - columns.Start;
        var result = new T[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                result[i, j] = matrix[rows.Start + i, columns.Start + j];
            }
        }

        return result;
    }
}