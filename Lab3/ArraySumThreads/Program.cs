using System;
using System.Threading;

class Program
{
    // Large shared array — all worker threads read from this.
    static long[] data = new long[10_000_000];

    // One slot per worker — each thread writes ONLY to its own slot.
    static long[] partialSums;

    // Number of worker threads = number of logical processors.
    static int numWorkers;

    // Worker method: sums the slice assigned to worker index `idx`.
    static void SumSlice(object? arg)
    {
        int idx = (int)arg!;
        int sliceSize = data.Length / numWorkers;

        int start = idx * sliceSize;

        // The LAST worker takes whatever remains, so no elements are dropped
        // when data.Length is not evenly divisible by numWorkers.
        int end = (idx == numWorkers - 1) ? data.Length : start + sliceSize;

        long sum = 0;
        for (int i = start; i < end; i++)
        {
            sum += data[i];
        }

        partialSums[idx] = sum;   // exclusive slot — no lock needed
    }

    static void Main()
    {
        // 1. Fill the array with 1, 2, 3, ..., 10_000_000
        for (int i = 0; i < data.Length; i++)
            data[i] = i + 1;

        // 2. Decide on the number of workers.
        numWorkers = Environment.ProcessorCount;
        partialSums = new long[numWorkers];
        Thread[] threads = new Thread[numWorkers];

        // 3. Spawn one worker thread per slice.
        for (int i = 0; i < numWorkers; i++)
        {
            int idx = i;   // capture loop variable
            threads[i] = new Thread(() => SumSlice(idx));
            threads[i].Start();
        }

        // 4. Join every thread before reading partialSums.
        for (int i = 0; i < numWorkers; i++)
        {
            threads[i].Join();
        }

        // 5. Combine partial sums into the threaded total.
        long threadedTotal = 0;
        foreach (long partial in partialSums)
            threadedTotal += partial;

        // 6. Compute the same total sequentially for comparison.
        long sequentialTotal = 0;
        foreach (long value in data)
            sequentialTotal += value;

        // 7. Report results.
        Console.WriteLine($"Array size:        {data.Length:N0}");
        Console.WriteLine($"Worker threads:    {numWorkers}");
        Console.WriteLine($"Threaded total:    {threadedTotal}");
        Console.WriteLine($"Sequential total:  {sequentialTotal}");
        Console.WriteLine($"Match:             {threadedTotal == sequentialTotal}");
    }
}