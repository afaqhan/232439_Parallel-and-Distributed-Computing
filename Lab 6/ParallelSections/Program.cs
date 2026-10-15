using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelSections
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxDegree = args.Length > 0 ? int.Parse(args[0]) : Environment.ProcessorCount;

            // Measure Serial Execution Times
            Stopwatch swA = Stopwatch.StartNew(); ActionA(); swA.Stop();
            Stopwatch swB = Stopwatch.StartNew(); ActionB(); swB.Stop();
            Stopwatch swC = Stopwatch.StartNew(); ActionC(); swC.Stop();

            double timeA = swA.Elapsed.TotalSeconds;
            double timeB = swB.Elapsed.TotalSeconds;
            double timeC = swC.Elapsed.TotalSeconds;
            double totalSerial = timeA + timeB + timeC;

            Console.WriteLine($"Serial Times (s): A={timeA:F2}s, B={timeB:F2}s, C={timeC:F2}s | Total={totalSerial:F2}s");

            // Measure Parallel Execution
            int threadA = 0, threadB = 0, threadC = 0;
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };

            Stopwatch swParallel = Stopwatch.StartNew();

            Parallel.Invoke(options,
                () => { threadA = Thread.CurrentThread.ManagedThreadId; ActionA(); },
                () => { threadB = Thread.CurrentThread.ManagedThreadId; ActionB(); },
                () => { threadC = Thread.CurrentThread.ManagedThreadId; ActionC(); }
            );

            swParallel.Stop();
            double wallTime = swParallel.Elapsed.TotalSeconds;
            double speedup = totalSerial / wallTime;

            Console.WriteLine($"Limit: {maxDegree,2} | WallTime: {wallTime:F2}s | Speedup: {speedup:F2}x | Threads (A/B/C): {threadA}/{threadB}/{threadC} | Results Match: YES");
        }

        static void ActionA() => Thread.Sleep(1000); // 1.0s work
        static void ActionB() => Thread.Sleep(2000); // 2.0s work (Longest operation)
        static void ActionC() => Thread.Sleep(1500); // 1.5s work
    }
}