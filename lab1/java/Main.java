import java.util.Random;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.atomic.AtomicBoolean;

public class Main {

    private static final int THREAD_COUNT = 7;
    private static final double STEP = 2;

    public static void main(String[] args) {
        Worker[] workers = new Worker[THREAD_COUNT];
        Thread[] threads = new Thread[THREAD_COUNT];
        AtomicBoolean[] stopSignals = new AtomicBoolean[THREAD_COUNT];
        int[] workDurations = new int[THREAD_COUNT];

        CountDownLatch startSignal = new CountDownLatch(1);
        Random random = new Random();

        for (int i = 0; i < THREAD_COUNT; i++) {
            stopSignals[i] = new AtomicBoolean(false);
            workDurations[i] = 2000 + random.nextInt(5000);

            workers[i] = new Worker(i, STEP, startSignal, stopSignals[i]);
            threads[i] = new Thread(workers[i]);
            threads[i].start();
        }

        startSignal.countDown();

        TimerController controller = new TimerController(stopSignals, workDurations);
        controller.launch();
    }
}
class Worker implements Runnable {

    private final int id;
    private final double step;
    private final CountDownLatch startSignal;
    private final AtomicBoolean stopSignal;

    public Worker(int id, double step, CountDownLatch startSignal, AtomicBoolean stopSignal) {
        this.id = id;
        this.step = step;
        this.startSignal = startSignal;
        this.stopSignal = stopSignal;
    }

    @Override
    public void run() {
        try {
            startSignal.await(); 
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            return;
        }

        double sum = 0;
        double current = 0;
        int count = 0;

        while (!stopSignal.get()) {
            sum += current;
            current += step;
            count++;
        }

        System.out.printf("Потік %d. Сума: %.2f, Використано елементів: %d%n", id + 1, sum, count);
    }
}


class TimerController {
    private final AtomicBoolean[] stopSignals;
    private final int[] durations;

    public TimerController(AtomicBoolean[] stopSignals, int[] durations) {
        this.stopSignals = stopSignals;
        this.durations = durations;
    }

    public void launch() {
        new Thread(() -> {
            long startTime = System.currentTimeMillis();
            boolean[] isStopped = new boolean[stopSignals.length];
            int stoppedCount = 0;

            while (stoppedCount < stopSignals.length) {
                long elapsed = System.currentTimeMillis() - startTime;

                for (int i = 0; i < stopSignals.length; i++) {
                    if (!isStopped[i] && elapsed >= durations[i]) {
                        stopSignals[i].set(true);
                        isStopped[i] = true;
                        stoppedCount++;
                    }
                }
                try {
                    Thread.sleep(100);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    return;
                }
            }
        }).start();
    }
}
