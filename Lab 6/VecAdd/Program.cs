using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VecAdd
{
    class Program
    {
        const int N = 5_000_000;
        const int PASSES = 20;

        static void Main(string[] args)
        {
            int maxDegree = args.Length > 0 ? int.Parse(args[0]) : Environment.ProcessorCount;

            float[] a = new float[N];
            float[] b = new float[N];
            float[] c = new float[N];

            for (int i = 0; i < N; i++)
            {
                a[i] = 1.0f;
                b[i] = 2.0f;
            }

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };
            Stopwatch sw = Stopwatch.StartNew();

            for (int pass = 0; pass < PASSES; pass++)
            {
                VecAddParallel(a, b, c, options);
            }

            sw.Stop();
            double totalTime = sw.Elapsed.TotalMilliseconds;

            bool correct = true;
            for (int i = 0; i < N; i++)
            {
                if (Math.Abs(c[i] - 3.0f) > 1e-5)
                {
                    correct = false;
                    break;
                }
            }

            Console.WriteLine($"Degree: {maxDegree} | TotalTime: {totalTime:F2} ms | Correct: {(correct ? "YES" : "NO")}");
        }

        static void VecAddParallel(float[] a, float[] b, float[] c, ParallelOptions options)
        {
            Parallel.ForEach(Partitioner.Create(0, N), options, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    c[i] = a[i] + b[i];
                }
            });
        }
    }
}