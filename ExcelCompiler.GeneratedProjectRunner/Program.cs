using System.Diagnostics;

ExcelCompiler.Generated.Program program = new();

Stopwatch sw = new();
Stopwatch outerSw = new();

double total = 0;

double[] inputs = [0.234d, 0.015d, 0.002567d, 0.30d, 0.003652d, 0.05d];
int count = 1_000_000;

Console.WriteLine("Starting Excel calculation test...");
outerSw.Start();
for (int i = 0; i < count; i++)
{
    var input = inputs[i % inputs.Length];
    sw.Start();
    var output = program.Main();
    sw.Stop();
    
    total += output;
}
outerSw.Stop();

Console.WriteLine($"Total time: {sw.Elapsed.TotalMilliseconds} ms");
Console.WriteLine($"Average time: {sw.Elapsed.TotalMilliseconds / count} ms");
Console.WriteLine($"Total output: {total}");
Console.WriteLine($"Average output: {total / count}");
Console.WriteLine($"Walltime for {count} iterations: {outerSw.Elapsed.TotalSeconds} s / {outerSw.Elapsed.TotalMilliseconds} ms");