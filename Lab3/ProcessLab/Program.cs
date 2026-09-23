using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--child")
        {
            RunAsChild();
        }
        else
        {
            RunAsParent();
        }
    }

    static void RunAsChild()
    {
        Console.WriteLine($"[Child] PID = {Environment.ProcessId}");

        int counter = 100;
        counter += 50;
        Console.WriteLine($"[Child] final counter = {counter}");
    }

    static void RunAsParent()
    {
        Console.WriteLine($"[Parent] PID = {Environment.ProcessId}");

        int counter = 100;
        counter += 1;

        // Build a ProcessStartInfo pointing at this same executable,
        // with ArgumentList containing "--child"
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath,   // path to this same .exe / .dll
            ArgumentList = { "--child" },
            UseShellExecute = false
        };

        // Launch the child and wait for it to finish
        using (Process child = Process.Start(startInfo))
        {
            child.WaitForExit();
        }

        Console.WriteLine($"[Parent] final counter = {counter}");
        Console.WriteLine("[Parent] Parent and child counters were modified independently (separate address spaces).");
    }
}