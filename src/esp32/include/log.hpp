#pragma once

#include <Arduino.h>

class Logger : public Stream
{
    String* _data;

public:
    Logger(){
        _data = new String();
    }
    ~Logger(){
        delete _data;
    }
    size_t write(uint8_t c) override
    {
        _data->concat((char)c);
        return 1;
    }
    size_t write(const uint8_t *buffer, size_t size) override
    {
        _data->concat(buffer, size);
        return Serial.write(buffer, size);
    }
    String Data()
    {
        return *_data;
    }

    int available() override
    {
        return Serial.available();
    }
    int read() override
    {
        return Serial.read();
    }
    int peek() override
    {
        return Serial.peek();
    }
} logger;
