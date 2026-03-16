package com.company;

class ArrClass {
    private final int dim;
    private final int threadNum;
    public final int[] arr;

    public ArrClass(int dim, int threadNum) {
        this.dim = dim;
        this.threadNum = threadNum;
        arr = new int[dim];
        for (int i = 0; i < dim; i++) {
            arr[i] = i;
        }
        arr[dim / 3] = -42;
    }

    public long[] partMin(int startIndex, int finishIndex) {
        long min = arr[startIndex];
        long minIndex = startIndex;
        for (int i = startIndex + 1; i < finishIndex; i++) {
            if (arr[i] < min) {
                min = arr[i];
                minIndex = i;
            }
        }
        return new long[]{min, minIndex};
    }

    private long globalMin = Long.MAX_VALUE;
    private long globalMinIndex = -1;

    synchronized public void collectMin(long min, long index) {
        if (min < globalMin) {
            globalMin = min;
            globalMinIndex = index;
        }
    }

    private int threadCount = 0;

    synchronized public void incThreadCount() {
        threadCount++;
        notifyAll();
    }

    synchronized private long[] getResult() {
        while (threadCount < threadNum) {
            try {
                wait();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
        return new long[]{globalMin, globalMinIndex};
    }

    public long[] threadMin() {
        int chunkSize = dim / threadNum;
        ThreadMin[] threads = new ThreadMin[threadNum];
        for (int i = 0; i < threadNum; i++) {
            int start = i * chunkSize;
            int end = (i == threadNum - 1) ? dim : start + chunkSize;
            threads[i] = new ThreadMin(start, end, this);
            threads[i].start();
        }
        return getResult();
    }
}

class ThreadMin extends Thread {
    private final int startIndex;
    private final int finishIndex;
    private final ArrClass arrClass;

    public ThreadMin(int startIndex, int finishIndex, ArrClass arrClass) {
        this.startIndex = startIndex;
        this.finishIndex = finishIndex;
        this.arrClass = arrClass;
    }

    @Override
    public void run() {
        long[] result = arrClass.partMin(startIndex, finishIndex);
        arrClass.collectMin(result[0], result[1]);
        arrClass.incThreadCount();
    }
}

public class Main {
    public static void main(String[] args) {
        int dim = 10000000;
        int threadNum = 4;

        ArrClass arrClass = new ArrClass(dim, threadNum);

        long[] seq = arrClass.partMin(0, dim);
        System.out.println("Sequential min: " + seq[0] + " at index " + seq[1]);

        long[] par = arrClass.threadMin();
        System.out.println("Parallel   min: " + par[0] + " at index " + par[1]);
    }
}
