using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int Iterations = 50;

    static string childPath =
        @"C:\Users\232439\Desktop\Lab 3\TrivialChild\bin\Release\net9.0\TrivialChild.exe";

    static void Main()
    {
        // --- Process creation ---
        var swProcess = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            var psi = new ProcessStartInfo
            {
                FileName = childPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process p = Process.Start(psi)!)
                p.WaitForExit();
        }
        swProcess.Stop();

        // --- Thread creation ---
        var swThread = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            Thread t = new Thread(() => { });
            t.Start();
            t.Join();
        }
        swThread.Stop();

        // --- Report ---
        double avgProcess = swProcess.Elapsed.TotalMilliseconds / Iterations;
        double avgThread  = swThread.Elapsed.TotalMilliseconds / Iterations;

        Console.WriteLine($"Average process creation: {avgProcess:F3} ms");
        Console.WriteLine($"Average thread creation:  {avgThread:F3} ms");
        Console.WriteLine($"Ratio: {avgProcess / avgThread:F1}x");
    }
}