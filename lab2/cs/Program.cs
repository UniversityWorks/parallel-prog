using System;
using System.Threading;

namespace ThreadMinSharp
{
    class Program
    {
        private static readonly int dim = 10000000;
        private static readonly int threadNum = 4;

        private readonly Thread[] threads = new Thread[threadNum];
        private readonly int[] arr = new int[dim];

        static void Main(string[] args)
        {
            Program p = new Program();
            p.InitArr();

            (long seqMin, long seqIdx) = p.PartMin(0, dim);
            Console.WriteLine($"Sequential min: {seqMin} at index {seqIdx}");

            (long parMin, long parIdx) = p.ParallelMin();
            Console.WriteLine($"Parallel   min: {parMin} at index {parIdx}");

            Console.ReadKey();
        }

        private void InitArr()
        {
            for (int i = 0; i < dim; i++)
                arr[i] = i;
            arr[dim / 3] = -42;
        }

        public (long, long) PartMin(int startIndex, int finishIndex)
        {
            long min = arr[startIndex];
            long minIndex = startIndex;
            for (int i = startIndex + 1; i < finishIndex; i++)
            {
                if (arr[i] < min)
                {
                    min = arr[i];
                    minIndex = i;
                }
            }
            return (min, minIndex);
        }

        private long globalMin = long.MaxValue;
        private long globalMinIndex = -1;
        private readonly object lockerForMin = new object();

        private int threadCount = 0;
        private readonly object lockerForCount = new object();

        private void StarterThread(object param)
        {
            if (param is Bound b)
            {
                (long min, long idx) = PartMin(b.StartIndex, b.FinishIndex);

                lock (lockerForMin)
                {
                    CollectMin(min, idx);
                }

                lock (lockerForCount)
                {
                    threadCount++;
                    Monitor.Pulse(lockerForCount);
                }
            }
        }

        private void CollectMin(long min, long idx)
        {
            if (min < globalMin)
            {
                globalMin = min;
                globalMinIndex = idx;
            }
        }

        public (long, long) ParallelMin()
        {
            int chunkSize = dim / threadNum;
            for (int i = 0; i < threadNum; i++)
            {
                int start = i * chunkSize;
                int end = (i == threadNum - 1) ? dim : start + chunkSize;
                threads[i] = new Thread(StarterThread);
                threads[i].Start(new Bound(start, end));
            }

            lock (lockerForCount)
            {
                while (threadCount < threadNum)
                    Monitor.Wait(lockerForCount);
            }
            return (globalMin, globalMinIndex);
        }

        class Bound
        {
            public int StartIndex { get; }
            public int FinishIndex { get; }
            public Bound(int s, int f) { StartIndex = s; FinishIndex = f; }
        }
    }
}
