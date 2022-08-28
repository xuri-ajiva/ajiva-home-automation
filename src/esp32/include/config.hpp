#pragma once

#include "file.hpp"
#include "log.hpp"
#include "json_generator.h"
#include <ArduinoJson.hpp>
#include <utility>
#include <numeric>

using namespace ArduinoJson;

class WifiData {
public:
    WifiData(String ssid, String password) : ssid(std::move(ssid)), password(std::move(password)) {}

private:
    String ssid;
    String password;

public:
    const char *getSsid() const {
        return ssid.c_str();
    }

    const char *getPassword() const {
        return password.c_str();
    }
};

typedef enum {
    IoType_Write = 1,
    IoType_Read = 2,
    IoType_Pulse = 3,
} IoType;

class IoData {
    String name;

    struct pinStateMode {
        uint8_t mode;
        uint8_t pin;
        uint8_t state;

        pinStateMode(uint8_t mode, uint8_t pin, uint8_t state) : mode(mode), pin(pin), state(state) {}
    };

    union {
        pinStateMode pin;
        pinStateMode trigger;
    };
    union {
        pinStateMode pin2;
        pinStateMode echo;
    };
    IoType type;
    long pulseLength;
    double convert;
    String unit;

    double m_Pulse() {
        if (type == IoType_Pulse) {
            digitalWrite(trigger.pin, LOW);
            delay(pulseLength);
            digitalWrite(trigger.pin, HIGH);
            delayMicroseconds(pulseLength);
            digitalWrite(trigger.pin, LOW);
            auto duration = pulseIn(echo.pin, HIGH);
            return duration * convert;
        } else {
            logger.println(String("[CONFIG] Waring: Pulse called on non-pulse type: ") + String(type));
            return 0;
        }
    }

public:
    explicit IoData(JsonObject &json) {
        String n = json["name"];
        name = std::move(n);
        type = static_cast<IoType>(json["type"]);

        switch (type) {
            case IoType_Write: {
                pin = pinStateMode(OUTPUT, json["pin"], json["state"]);
                break;
            }
            case IoType_Read: {
                pin = pinStateMode(INPUT, json["pin"], json["state"]);
                break;
            }
            case IoType_Pulse: {
                echo = pinStateMode(INPUT, json["echo"], LOW);
                trigger = pinStateMode(OUTPUT, json["trigger"], LOW);
                pulseLength = json["pulseLength"];
                convert = json["convert"];
                String u = json["unit"];
                unit = std::move(u);
                break;
            }
            default: {
                logger.println("[CONFIG] Warning: Unknown type: " + String(type));
                break;
            }
        }
    }

    double Pulse(int &count, std::vector<double> &buffer, double &mean, double &stDev) {
        //perform the pulse count times
        //use normal distribution to smooth out the results
        for (int i = 0; i < count; i++) {
            auto value = m_Pulse();
            //if the value is < 0.01 then it is invalid
            if (value < 0.01) {
                continue;
            }
            //if the value is > 500 then it is invalid
            if (value > 500) {
                continue;
            }
            buffer.push_back(value);
        }
        //discard values that are 95% of the mean
        mean = std::accumulate(buffer.begin(), buffer.end(), 0.0) / buffer.size();
        stDev = std::accumulate(buffer.begin(), buffer.end(), 0.0, [mean](double sum, double val) {
            return sum + std::pow(val - mean, 2);
        }) / count;
        stDev = std::sqrt(stDev);
        double result = 0;
        int countValid = 0;
        for (auto &val : buffer) {
            if (val >= mean - stDev * 1.5 && val <= mean + stDev * 1.5) {
                result += val;
                countValid++;
            }
        }
        result /= countValid;
        count = countValid;
        return result;
    }


    const String &getName() const {
        return name;
    }

    IoType getType() const {
        return type;
    }

    void Write(uint8_t val) {
        if (type == IoType_Write) {
            pin.state = val;
            digitalWrite(pin.pin, val);
            logger.println(String("[CONFIG] Write: ") + String(val) + String(" to pin: ") + String(pin.pin));
        } else {
            logger.println(String("[WARNING] Write called on non-write type: ") + String(type));
        }
    }

    uint8_t Read() {
        if (type == IoType_Read) {
            return digitalRead(pin.pin);
        } else {
            return pin.state;
        }
    }


    String PulseConvert(double duration) {
        if (type == IoType_Pulse) {
            return String(duration) + " (" + unit + ")";
        } else {
            logger.println(String("[CONFIG] Waring: Pulse called on non-pulse type: ") + String(type));
            return "WRING CONFIG";
        }
    }

    void Configure() {
        switch (type) {
            case IoType_Write:
            case IoType_Read: {
                pinMode(pin.pin, pin.mode);
                digitalWrite(pin.pin, pin.state);
                logger.println("[CONFIGURING] '" + name + "' on pin " + String(pin.pin));
                break;
            }
            case IoType_Pulse: {
                pinMode(echo.pin, echo.mode);
                digitalWrite(echo.pin, echo.state);
                pinMode(trigger.pin, trigger.mode);
                digitalWrite(trigger.pin, trigger.state);
                logger.println("[CONFIGURING] '" + name + "' as pulse on echo pin " + String(echo.pin) +
                               " and trigger pin " + String(trigger.pin));
                break;
            }
            default: {
                logger.println("[CONFIGURING] Warning: Unknown type: " + String(type));
                break;
            }
        }
    }

    const String &getUnit() const {
        return unit;
    }
};

class Config {
    bool Parse(const String &pString) {
        // Parse json string
        // expecting:
        // {
        //    "wifi": {
        //        "ssid": "MyWiFi",
        //        "password": "MyPassword"
        //    },
        //    "io": [
        //        { "type": 1, "name": "led1", "pin": 2 },
        //        { "type": 3, "name": "sensor1", "echo": 14, "trigger": 12, "pulseLength": 10, "convert": 0.017, "unit": "cm" }
        //    ],
        //    "specs": {
        //        "max_temp": 30,
        //    }
        // }

        StaticJsonDocument<1024> doc;
        DeserializationError error = deserializeJson(doc, pString);
        if (error) {
            logger.println("[CONFIG] Failed to parse json");
            return false;
        }
        // Parse wifi data
        JsonObject w = doc["wifi"];
        wifi = WifiData(w["ssid"], w["password"]);
        // Parse io data
        JsonArray io = doc["io"];
        for (JsonObject i: io) {
            ioData.emplace_back(i);
        }

        for (IoData i: ioData) {
            i.Configure();
        }

        // Parse specs data
        JsonObject s = doc["specs"];
        int maxTemp = s["max_temp"];
        logger.println("[CONFIG] Max temp: " + String(maxTemp));
        return true;
    }

    bool m_init = false;

public:
    WifiData wifi = WifiData("", "");
    std::vector<IoData> ioData;

    bool Load(const char *path) {
        logger.println("[CONFIG] Loading config from " + String(path));
        if (!file.Init())
            return false;
        String content;

        if (!file.Read((String(path) + ("/config.json")).c_str(),
                       "[CONFIG] Failed to read config file", content))
            return false;

        logger.println("[CONFIG] Parsing config file");
        if (!Parse(content))
            return false;

        m_init = true;
        return true;
    }

    bool Init() {
        return m_init;
    }
} config;