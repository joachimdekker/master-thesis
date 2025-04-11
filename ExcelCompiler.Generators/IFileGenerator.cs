using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Generators;

public interface IFileGenerator
{
    /// <summary>
    /// Generates a file based on the provided support graph and writes it to the specified output stream.
    /// </summary>
    /// <param name="graph">The support graph containing the computation structure.</param>
    /// <param name="outputStream">The stream to which the generated file will be written.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Generate(SupportGraph graph, Stream outputStream, CancellationToken cancellationToken = default);
}