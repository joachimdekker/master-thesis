// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

ExcelCompiler.Generated.Program program = new();

Stopwatch sw = new();

double total = 0;
int count = 1;

sw.Start();
for (int i = 0; i < count; i++)
{
    total += program.Main();
}
sw.Stop();

Console.WriteLine(total);
Console.WriteLine(total/count);
Console.WriteLine(sw.Elapsed.TotalSeconds);
