using System;
using System.Diagnostics;
using System.Threading;

class PhaseBarrier
{
    private readonly object m = new object();

    private readonly int parties;

    private int waiting;

    private long generation;

    public PhaseBarrier(int parties)
    {
        this.parties = parties;
    }

    public void SignalAndWait()
    {
        lock (m)
        {
            long myGeneration = generation;

            waiting++;

            if (waiting == parties)
            {
                waiting = 0;

                generation++;

                Monitor.PulseAll(m);
            }
            else
            {
                while (myGeneration == generation)
                {
                    Monitor.Wait(m);
                }
            }
        }
    }
}

class Program
{
    const int Rounds = 100_000;

    static int numWorkers = 4;

    static bool useBarrier = true;

    static ulong[] cells;

    static PhaseBarrier bar;

    static void Sync()
    {
        if (useBarrier)
            bar.SignalAndWait();
    }

    static void Worker(int id)
    {
        for (int r = 0; r < Rounds; r++)
        {
            ulong next =
                cells[id] +
                cells[(id + 1) % numWorkers];

            // All workers finish reading
            Sync();

            // Write result
            cells[id] = next;

            // All workers finish writing
            Sync();
        }
    }

    static void Main(string[] args)
    {
        foreach (string a in args)
        {
            if (a == "--no-barrier")
                useBarrier = false;
            else
                numWorkers = int.Parse(a);
        }

        cells = new ulong[numWorkers];

        ulong[] expected =
            new ulong[numWorkers];

        ulong[] tmp =
            new ulong[numWorkers];

        for (int i = 0; i < numWorkers; i++)
        {
            cells[i] = (ulong)(i + 1);
            expected[i] = (ulong)(i + 1);
        }

        bar = new PhaseBarrier(numWorkers);

        Thread[] threads =
            new Thread[numWorkers];

        Stopwatch sw =
            Stopwatch.StartNew();

        for (int i = 0; i < numWorkers; i++)
        {
            int id = i;

            threads[i] =
                new Thread(() => Worker(id));

            threads[i].Start();
        }

        foreach (Thread t in threads)
            t.Join();

        sw.Stop();

        // Sequential reference
        for (int r = 0; r < Rounds; r++)
        {
            for (int i = 0; i < numWorkers; i++)
            {
                tmp[i] =
                    expected[i] +
                    expected[(i + 1) % numWorkers];
            }

            for (int i = 0; i < numWorkers; i++)
            {
                expected[i] = tmp[i];
            }
        }

        int mismatches = 0;

        for (int i = 0; i < numWorkers; i++)
        {
            if (cells[i] != expected[i])
                mismatches++;
        }

        string label =
            useBarrier ? "barrier" : "no barrier";

        string ok =
            mismatches == 0 ? "YES" : "NO";

        Console.WriteLine(
            $"[{label}] workers={numWorkers} " +
            $"rounds={Rounds} " +
            $"matches_reference={ok} " +
            $"mismatching_cells={mismatches} " +
            $"time={sw.Elapsed.TotalMilliseconds:F2} ms");
    }
}