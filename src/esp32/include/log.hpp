#pragma once

#include <Arduino.h>

class Logger : public Stream {
    String *_data;
    bool init = false;
public:
    bool Init(int baud = 115200) {
        if (init) return true;
        Serial.begin(baud);
        init = true;
        println("[LOGGER] Started");
        return true;
    }

    Logger() {
        _data = new String();
    }

    ~Logger() {
        delete _data;
    }

    size_t write(uint8_t c) override {
        if (!Init())return 0;
        _data->concat((char) c);
        return 1;
    }

    size_t write(const uint8_t *buffer, size_t size) override {
        if(!Init())return 0;
        _data->concat(buffer, size);
        return Serial.write(buffer, size);
    }

    String Data() {
        if(!Init())return "";
        return *_data;
    }

    int available() override {
        if(!Init())return 0;
        return Serial.available();
    }

    int read() override {
        if(!Init())return 0;
        return Serial.read();
    }

    int peek() override {
        if(!Init())return 0;
        return Serial.peek();
    }
} logger;
