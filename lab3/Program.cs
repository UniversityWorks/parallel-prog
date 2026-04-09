using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerConsumerMulti
{
    class Storage
    {
        private readonly List<int> _items = new List<int>();
        private readonly Semaphore _access = new Semaphore(1, 1);
        private readonly Semaphore _empty;
        private readonly Semaphore _full;

        public Storage(int capacity)
        {
            _empty = new Semaphore(0, capacity);
            _full  = new Semaphore(capacity, capacity);
        }

        public void Add(int item, string producerName)
        {
            _full.WaitOne();
            _access.WaitOne();

            _items.Add(item);
            Console.WriteLine($"[{producerName}] added item {item}. Storage: {_items.Count}");

            _access.Release();
            _empty.Release();
        }

        public int Take(string consumerName)
        {
            _empty.WaitOne();
            _access.WaitOne();

            int item = _items[0];
            _items.RemoveAt(0);
            Console.WriteLine($"[{consumerName}] took item {item}. Storage: {_items.Count}");

            _access.Release();
            _full.Release();

            return item;
        }
    }

    class Producer
    {
        private readonly Storage _storage;
        private readonly int _itemsToProduces;
        private readonly string _name;
        private static int _nextItemId = 0;

        public Producer(string name, Storage storage, int itemsToProduce)
        {
            _name = name;
            _storage = storage;
            _itemsToProduces = itemsToProduce;
        }

        public void Run()
        {
            for (int i = 0; i < _itemsToProduces; i++)
            {
                int id = Interlocked.Increment(ref _nextItemId);
                Thread.Sleep(200);
                _storage.Add(id, _name);
            }
            Console.WriteLine($"[{_name}] finished producing.");
        }
    }

    class Consumer
    {
        private readonly Storage _storage;
        private readonly int _itemsToConsume;
        private readonly string _name;

        public Consumer(string name, Storage storage, int itemsToConsume)
        {
            _name = name;
            _storage = storage;
            _itemsToConsume = itemsToConsume;
        }

        public void Run()
        {
            for (int i = 0; i < _itemsToConsume; i++)
            {
                _storage.Take(_name);
                Thread.Sleep(300);
            }
            Console.WriteLine($"[{_name}] finished consuming.");
        }
    }

    class Program
    {
        static void Main()
        {
            const int storageCapacity = 5;
            const int totalItems      = 12;

            const int producerCount = 3;
            const int consumerCount = 2;

            int itemsPerProducer = totalItems / producerCount;
            int itemsPerConsumer = totalItems / consumerCount;

            var storage = new Storage(storageCapacity);
            var threads = new List<Thread>();

            for (int i = 0; i < producerCount; i++)
            {
                int items = (i == producerCount - 1)
                    ? totalItems - itemsPerProducer * (producerCount - 1)
                    : itemsPerProducer;

                var producer = new Producer($"Producer-{i + 1}", storage, items);
                var t = new Thread(producer.Run);
                threads.Add(t);
            }

            for (int i = 0; i < consumerCount; i++)
            {
                int items = (i == consumerCount - 1)
                    ? totalItems - itemsPerConsumer * (consumerCount - 1)
                    : itemsPerConsumer;

                var consumer = new Consumer($"Consumer-{i + 1}", storage, items);
                var t = new Thread(consumer.Run);
                threads.Add(t);
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Console.WriteLine("\nAll done.");
        }
    }
}
