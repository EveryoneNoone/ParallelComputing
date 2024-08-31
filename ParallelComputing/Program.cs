using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        int[] sizes = { 100000, 1000000, 10000000 };
        foreach (int size in sizes) 
        {
            var array = CreateFillArray(size);
            Console.WriteLine($"Array size: {size}");
            var watch = new Stopwatch();
            watch.Start();
            long sum = SequentialSum(array);
            watch.Stop();
            Console.WriteLine($"Sequential sum: {sum}, time: {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            sum = ParallelLinqSum(array);
            watch.Stop();
            Console.WriteLine($"Parallel sum linq: {sum}, time: {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            sum = ParallelSumWithThreads(array);
            watch.Stop();
            Console.WriteLine($"Parallel sum threads: {sum}, time: {watch.ElapsedMilliseconds} ms");
        }
    }

    private static int[] CreateFillArray(int size)
    {
        var random = new Random();
        var array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = random.Next(0, 100);
        }
        return array;
    }

    private static long SequentialSum(int[] array)
    {
        long sum = 0;
        for(var i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }
        return sum;
    }

    private static long ParallelLinqSum(int[] array)
    {
        return array.AsParallel().Sum();
    }

    private static long ParallelSumWithThreads(int[] array)
    {
        long sum = 0;
        int threadCount = Environment.ProcessorCount;
        var threads = new List<Thread>();
        int chunkSize = array.Length / threadCount;
        object lockObject = new object();
        for (int i = 0; i < threadCount; i++)
        {
            int start = i * chunkSize;
            int end = (i == threadCount - 1) ? array.Length : start + chunkSize;
            threads.Add(new Thread(() =>
            {
                long partialSum = 0;
                for (int j = start; j < end; j++)
                {
                    partialSum += array[j];
                }
                lock (lockObject)
                {
                    sum += partialSum;
                }
            }));
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach(var thread in threads)
        {
            thread.Join();
        }
        return sum;
    }
}