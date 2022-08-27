#pragma once

#include "file.hpp"
#include "log.hpp"
#include "json_generator.h"
#include <ArduinoJson.hpp>
#include <utility>

using namespace ArduinoJson;

class WifiData {
public:
    WifiData(String ssid, String password) :
            ssid(std::move(ssid)), password(std::move(password)) {}

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

class IoData {
    String name;
    uint8_t pin;
    uint8_t mode;

    uint8_t m_state;
public:
    IoData(String name, uint8_t pin, uint8_t mode) :
            name(std::move(name)), pin(pin), mode(mode), m_state(LOW) {}

    const String &getName() const {
        return name;
    }

    uint8_t getPin() const {
        return pin;
    }

    uint8_t getMode() const {
        return mode;
    }

    void Write(uint8_t val) {
        m_state = val;
        digitalWrite(pin, val);
    }

    void Configure() {
        pinMode(pin, mode);
        digitalWrite(pin, m_state);
        logger.println("[CONFIG] IoData: " + name + ": on PIN: " + String(pin) + " with Mode " + String(mode));
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
        //        { "name": "led1", "pin": 2, "mode": 3 },
        //        { "name": "led2", "pin": 0, "mode": 3 }
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
            ioData.emplace_back(
                    i["name"],
                    int8_t(i["pin"]),
                    int8_t(i["mode"])
            );
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
        if (!file.Init()) return false;
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