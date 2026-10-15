using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScopingBug
{
    class Program
    {
        const int N = 100_000;

        static void Main(string[] args)
        {
            string mode = args.Length > 0 ? args[0].ToLower() : "bug";
            int maxDegree = args.Length > 1 ? int.Parse(args[1]) : 4;

            int[] input = new int[N];
            int[] output = new int[N];

            for (int i = 0; i < N; i++) input[i] = i;

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };
            int wrongElements = 0;

            if (mode == "bug")
            {
                // BUG: Shared variable 'tmp' captured across threads
                int tmp = 0;
                Parallel.For(0, N, options, i =>
                {
                    tmp = input[i] * 3;
                    Thread.SpinWait(20); // Widens race condition window
                    output[i] = tmp + 7;
                });
            }
            else if (mode == "local")
            {
                // FIX 1: Thread-private local variable
                Parallel.For(0, N, options, i =>
                {
                    int tmp = input[i] * 3; // Stack local per iteration
                    Thread.SpinWait(20);
                    output[i] = tmp + 7;
                });
            }
            else if (mode == "threadlocal")
            {
                // FIX 2: TPL Thread-Local State Overload
                Parallel.For<int>(
                    0, N, options,
                    () => 0, // Thread-local initialization
                    (i, loopState, tls) =>
                    {
                        tls = input[i] * 3;
                        Thread.SpinWait(20);
                        output[i] = tls + 7;
                        return tls;
                    },
                    localFinally: tls => { }
                );
            }

            // Verification
            for (int i = 0; i < N; i++)
            {
                int expected = input[i] * 3 + 7;
                if (output[i] != expected) wrongElements++;
            }

            Console.WriteLine($"Mode: {mode,-11} | Limit: {maxDegree} | Wrong Elements: {wrongElements}");
        }
    }
}