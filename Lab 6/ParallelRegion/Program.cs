
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        int degree = args.Length > 0
            ? int.Parse(args[0])
            : Environment.ProcessorCount;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = degree
        };

        var threadIds = new ConcurrentDictionary<int, byte>();

        Stopwatch stopwatch = Stopwatch.StartNew();

        Parallel.For(0, 20, options, i =>
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            threadIds.TryAdd(threadId, 0);

            Console.WriteLine(
                $"Iteration {i}: Thread ID = {threadId}");

            Thread.Sleep(100);
        });

        stopwatch.Stop();

        double predictedTime = Math.Ceiling(20.0 / degree) * 100;

        Console.WriteLine("\n=== Task 1 Results ===");
        Console.WriteLine($"Processor count: {Environment.ProcessorCount}");
        Console.WriteLine($"Maximum degree: {degree}");
        Console.WriteLine($"Distinct threads used: {threadIds.Count}");
        Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
        Console.WriteLine($"Predicted time: {predictedTime:F2} ms");
        Console.WriteLine("Thread IDs seen: " +
            string.Join(", ", threadIds.Keys.OrderBy(id => id)));
    }
}
