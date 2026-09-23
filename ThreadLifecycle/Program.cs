using System;
using System.Threading;

class Program
{
    static void Worker()
    {
        Thread.Sleep(200);
    }

    static void Main()
    {
        Thread t = new Thread(Worker);

        // conceptually: New
        Console.WriteLine($"After creation:              {t.ThreadState}");

        t.Start();

        // conceptually: Runnable / Ready (or Running)
        Console.WriteLine($"Immediately after Start():   {t.ThreadState}");

        // Give the worker a moment to reach Thread.Sleep(200)
        Thread.Sleep(50);

        // conceptually: Blocked / Waiting
        Console.WriteLine($"While worker is sleeping:    {t.ThreadState}");

        t.Join();

        // conceptually: Terminated
        Console.WriteLine($"After Join() completes:      {t.ThreadState}");
    }
}