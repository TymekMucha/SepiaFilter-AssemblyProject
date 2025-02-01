#include "ImageProcessingLibrary.h"
#include "pch.h"
#include <cmath>
#include <stdexcept>
#include <cstdint>
#include <iostream>
#include <thread>
#include <vector>
#include <algorithm>

// Eksportowana funkcja w DLL
extern "C" __declspec(dllexport) void ConvertToSepia(unsigned char* pixelData, int width, int height, int stride, int numThreads, int depth) {
    if (!pixelData || width <= 0 || height <= 0 || stride < width * 4 || numThreads <= 0 || depth < 0) {
        throw std::invalid_argument("Z�e argumenty wywo�ania funkcji konwersji obrazu");
    }

    std::cout << "Number of threads used: " << numThreads << std::endl;

    // Funkcja przetwarzaj�ca okre�lon� cz�� obrazu
    auto processChunk = [&](int startRow, int endRow) {
        for (int y = startRow; y < endRow; ++y) {
            unsigned char* row = pixelData + y * stride;
            for (int x = 0; x < width; ++x) {
                unsigned char* pixel = row + x * 4; // Ka�dy piksel to 4 bajty: BGRA
                unsigned char b = pixel[0];
                unsigned char g = pixel[1];
                unsigned char r = pixel[2];

                // Konwersja na odcienie szaro�ci
                unsigned char gray = static_cast<unsigned char>((r + g + b) / 3);

                // Konwersja do sepii
                unsigned char rr = min(255, gray + depth * 2);
                unsigned char gg = min(255, gray + depth);
                unsigned char bb = gray;

                // Aktualizacja piksela
                pixel[0] = bb; // B
                pixel[1] = gg; // G
                pixel[2] = rr; // R
            }
        }
        };

    // Dzielenie pracy na w�tki
    std::vector<std::thread> threads;
    int rowsPerThread = height / numThreads;

    for (int t = 0; t < numThreads; ++t) {
        int startRow = t * rowsPerThread;
        int endRow = (t == numThreads - 1) ? height : startRow + rowsPerThread;

        threads.emplace_back(processChunk, startRow, endRow);
    }

    // Czekanie na zako�czenie wszystkich w�tk�w
    for (auto& thread : threads) {
        thread.join();
    }
}
