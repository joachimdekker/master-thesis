namespace ExcelCompiler.Passes;

public interface ICompilerPass
{
    object Transform(object input);
}

public interface ICompilerPass<in TIn, out TOut> : ICompilerPass
{
    TOut Transform(TIn input);
}