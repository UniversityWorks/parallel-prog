#include <iostream>
#include <climits>
#include "omp.h"

using namespace std;

const int ROWS = 10000;
const int COLS = 10000;

int arr[ROWS][COLS];

void init_arr() {
    for (int i = 0; i < ROWS; i++) {
        for (int j = 0; j < COLS; j++) {
            arr[i][j] = i * COLS + j + 1;
        }
    }
    for (int j = 0; j < COLS; j++) {
        arr[ROWS / 3][j] = -1;
    }
}

long long total_sum(int num_threads) {
    long long sum = 0;

    double t1 = omp_get_wtime();

#pragma omp parallel for reduction(+:sum) num_threads(num_threads) collapse(2)
    for (int i = 0; i < ROWS; i++) {
        for (int j = 0; j < COLS; j++) {
            sum += arr[i][j];
        }
    }

    double t2 = omp_get_wtime();

    cout << "  [sum] " << num_threads << " thread(s): "
         << (t2 - t1) << " seconds" << endl;

    return sum;
}

pair<int, long long> min_row_sum(int num_threads) {
    int    min_row = 0;
    long long min_sum = LLONG_MAX;

    double t1 = omp_get_wtime();

#pragma omp parallel num_threads(num_threads)
    {
        int       local_min_row = 0;
        long long local_min_sum = LLONG_MAX;

#pragma omp for
        for (int i = 0; i < ROWS; i++) {
            long long row_sum = 0;
            for (int j = 0; j < COLS; j++) {
                row_sum += arr[i][j];
            }
            if (row_sum < local_min_sum) {
                local_min_sum = row_sum;
                local_min_row = i;
            }
        }

#pragma omp critical
        {
            if (local_min_sum < min_sum) {
                min_sum = local_min_sum;
                min_row = local_min_row;
            }
        }
    }

    double t2 = omp_get_wtime();

    cout << "  [min_row] " << num_threads << " thread(s): "
         << (t2 - t1) << " seconds" << endl;

    return {min_row, min_sum};
}

int main() {
    cout << "Розмір масиву: " << ROWS << " x " << COLS << endl;

    init_arr();

    cout << "\nЗапуск паралельних секцій\n" << endl;

    int thread_counts[] = {1, 2, 3, 4, 8, 10, 16, 32};
    int num_tests = sizeof(thread_counts) / sizeof(thread_counts[0]);

    omp_set_nested(1); 
    double t_global_start = omp_get_wtime();

#pragma omp parallel sections
    {
#pragma omp section
        {
            long long result_sum = 0;
            for (int t = 0; t < num_tests; t++) {
                result_sum = total_sum(thread_counts[t]);
            }
            cout << "Результат суми всіх елементів: " << result_sum << endl;
            cout << endl;
        }

#pragma omp section
        {
            pair<int, long long> result = {0, 0};
            for (int t = 0; t < num_tests; t++) {
                result = min_row_sum(thread_counts[t]);
            }
            cout << "Рядок з мінімальною сумою: " << result.first << endl;
            cout << "Значення мінімальної суми: " << result.second << endl;
            cout << endl;
        }
    }

    double t_global_end = omp_get_wtime();

    cout << "Загальний час виконання: "
         << (t_global_end - t_global_start) << " секунд" << endl;

    return 0;
}
