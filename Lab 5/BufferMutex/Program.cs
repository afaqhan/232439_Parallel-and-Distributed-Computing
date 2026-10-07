using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int BufferSize = 8;
    const int Items = 200_000;

    static readonly int[] buffer = new int[BufferSize];

    static int inPos = 0;
    static int outPos = 0;
    static int count = 0;

    static readonly object gate = new object();

    static long producerRetries = 0;
    static long consumerRetries = 0;
    static long consumedSum = 0;
    static long orderErrors = 0;

    static void Producer()
    {
        for (int i = 1; i <= Items; i++)
        {
            while (true)
            {
                lock (gate)
                {
                    if (count < BufferSize)
                    {
                        buffer[inPos] = i;
                        inPos = (inPos + 1) % BufferSize;
                        count++;

                        break;
                    }
                }

                producerRetries++;
                Thread.Yield();
            }
        }
    }

    static void Consumer()
    {
        for (int i = 1; i <= Items; i++)
        {
            int item = 0;

            while (true)
            {
                lock (gate)
                {
                    if (count > 0)
                    {
                        item = buffer[outPos];
                        outPos = (outPos + 1) % BufferSize;
                        count--;

                        break;
                    }
                }

                consumerRetries++;
                Thread.Yield();
            }

            if (item != i)
                orderErrors++;

            consumedSum += item;
        }
    }

    static void Main()
    {
        Thread prod = new Thread(Producer);
        Thread cons = new Thread(Consumer);

        Stopwatch sw = Stopwatch.StartNew();

        prod.Start();
        cons.Start();

        prod.Join();
        cons.Join();

        sw.Stop();

        long expectedSum =
            (long)Items * (Items + 1) / 2;

        Console.WriteLine(
            $"[lock + polling] items={Items} buffer={BufferSize}");

        string ok =
            expectedSum == consumedSum ? "YES" : "NO";

        Console.WriteLine(
            $"expected_sum={expectedSum} " +
            $"actual_sum={consumedSum} " +
            $"checksum_ok={ok} " +
            $"order_errors={orderErrors}");

        Console.WriteLine(
            $"producer_retries={producerRetries} " +
            $"consumer_retries={consumerRetries} " +
            $"time={sw.Elapsed.TotalMilliseconds:F2} ms");
    }
}