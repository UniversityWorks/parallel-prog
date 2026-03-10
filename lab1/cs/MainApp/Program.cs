using System;
using System.Threading;

 class Program
    {
        private const int ThreadCount = 4;
        private const double Step = 1;

        static void Main()
        {
            Worker[] workers = new Worker[ThreadCount];
            ManualResetEvent[] stopSignals = new ManualResetEvent[ThreadCount];
            int[] workDurations = new int[ThreadCount];
            ManualResetEvent startSignal = new ManualResetEvent(false);
            Random random = new Random();

            for (int i = 0; i < ThreadCount; i++)
            {
                stopSignals[i] = new ManualResetEvent(false);
                workDurations[i] = random.Next(2000, 7000);

                workers[i] = new Worker(i, Step, startSignal, stopSignals[i]);
                workers[i].Start();
            }

            startSignal.Set();

            TimerController controller = new TimerController(stopSignals, workDurations);
            controller.Launch();
        }
    }

    class Worker
    {
        private readonly int _id;
        private readonly double _step;
        private readonly ManualResetEvent _startSignal;
        private readonly ManualResetEvent _stopSignal;
        private readonly Thread _thread;

        public Worker(int id, double step, ManualResetEvent startSignal, ManualResetEvent stopSignal)
        {
            _id = id;
            _step = step;
            _startSignal = startSignal;
            _stopSignal = stopSignal;
            _thread = new Thread(DoWork);
        }

        public void Start() => _thread.Start();

        private void DoWork()
        {
            _startSignal.WaitOne();

            double sum = 0;
            double current = 0;
            int count = 0;

            while (!_stopSignal.WaitOne(0))
            {
                sum += current;
                current += _step;
                count++;
            }

            Console.WriteLine($"Потік {_id + 1}. Сума: {sum}, Використано елементів: {count}");
        }
    }

    class TimerController
    {
        private readonly ManualResetEvent[] _stopSignals;
        private readonly int[] _durations;

        public TimerController(ManualResetEvent[] stopSignals, int[] durations)
        {
            _stopSignals = stopSignals;
            _durations = durations;
        }

        public void Launch()
        {
            for (int i = 0; i < _stopSignals.Length; i++)
            {
                int index = i;
                int delay = _durations[i];

                new Thread(() =>
                {
                    Thread.Sleep(delay);
                    _stopSignals[index].Set();
                }).Start();
            }
        }
    }   
