using System;
using System.Threading;

class Table
{
    public SemaphoreSlim[] forks = new SemaphoreSlim[5];

    public Table()
    {
        for (int i = 0; i < 5; i++)
        {
            forks[i] = new SemaphoreSlim(1, 1);
        }
    }

    public void GetFork(int id)
    {
        forks[id].Wait();
    }

    public void PutFork(int id)
    {
        forks[id].Release();
    }
}

class Philosopher
{
    private Table table;
    private int id;
    private int leftFork;
    private int rightFork;
    private Thread thread;

    public Philosopher(int id, Table table)
    {
        this.id = id;
        this.table = table;

        rightFork = id;
        leftFork = (id + 1) % 5;

        if (id == 4)
        {
            int temp = leftFork;
            leftFork = rightFork;
            rightFork = temp;
        }

        thread = new Thread(Run);
        thread.Start();
    }

    private void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"Філософ {id} думає (раз {i + 1})");
            Thread.Sleep(100); 
            table.GetFork(rightFork);
            Console.WriteLine($"Філософ {id} взяв виделку {rightFork}");

            table.GetFork(leftFork);
            Console.WriteLine($"Філософ {id} взяв виделку {leftFork}");

            Console.WriteLine($"Філософ {id} їсть (раз {i + 1})");
            Thread.Sleep(100);

            table.PutFork(rightFork);
            Console.WriteLine($"Філософ {id} поклав виделки");
        }

        Console.WriteLine($"Філософ {id} закінчив їжу.");
    }

    public void Join()
    {
        thread.Join();
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Задача про філософів, що обідають");
        Console.WriteLine("=========================================");

        Table table = new Table();
        Philosopher[] philosophers = new Philosopher[5];

        for (int i = 0; i < 5; i++)
        {
            philosophers[i] = new Philosopher(i, table);
        }

        for (int i = 0; i < 5; i++)
        {
            philosophers[i].Join();
        }

        Console.WriteLine("=========================================");
    }
}
