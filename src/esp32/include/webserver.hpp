#pragma once

// Import required libraries
#include "ESPAsyncWebServer.h"
#include "info.hpp"
#include "log.hpp"
#include "specs.h"
#include "config.hpp"
#include "network.hpp"
#include "ArduinoJson.hpp"
#include "json_generator.h"

using namespace ArduinoJson;

class WebserverWrapper {
    bool m_init;
    // Set web server port number to 80
    AsyncWebServer server = AsyncWebServer(80);


    void LoadFromConfig() {
        for (auto &item: config.ioData) {
            String url = String("/api/io/device/") + item.getName();
            switch (item.getType()) {
                case IoType_Write: {
                    server.on(url.c_str(), HTTP_GET, HandleStateChange(item));
                    break;
                }
                case IoType_Read: {
                    server.on(url.c_str(), HTTP_GET, HandleStateRead(item));
                    break;
                }
                case IoType_Pulse: {
                    server.on(url.c_str(), HTTP_GET, HandlePulse(item));
                    break;
                }
                default:
                    logger.println("Unknown mode: " + String(item.getType()));
                    break;
            }
        }
        server.on("/api/io/list", HTTP_GET, [](AsyncWebServerRequest *request) {
            String urls = "{\"devices\":[";
            bool first = true;
            for (auto &item: config.ioData) {
                if (first) {
                    first = false;
                } else {
                    urls += ",";
                }
                urls += "\"";
                urls += item.getName();
                urls += "\"";
            }
            urls += "]}";
            request->send(200, "application/json", urls);
        });
    }

    static ArRequestHandlerFunction HandleStateChange(IoData &item) {
        return [&item](AsyncWebServerRequest *request) {
            auto state = request->getParam("Write");
            if (state->value() == "HIGH" || state->value() == "1") {
                item.Write(HIGH);
            } else if (state->value() == "LOW" || state->value() == "0") {
                item.Write(LOW);
            } else {
                request->send(400, "text/plain", "Invalid state");
            }
            request->send(200, "text/plain", "OK");
        };
    }

    static ArRequestHandlerFunction HandlePulse(IoData &item) {
        return [&item](AsyncWebServerRequest *request) {
            using namespace ArduinoJson;
            int count;
            if (request->hasParam("count")) {
                count = request->getParam("count")->value().toInt();
                if (count < 1 || count > 32) {
                    request->send(400, "text/plain", "Invalid count");
                    return;
                }
            } else {
                count = 8;
            }
            double mean, stDev;
            std::vector<double> buffer;
            auto valid = count;
            auto value = item.Pulse(valid, buffer, mean, stDev);
            auto converted = item.PulseConvert(value);
            StaticJsonDocument<1024> doc;
            doc["value"] = converted;
            doc["raw"] = value;
            doc["unit"] = item.getUnit();
            doc["count"] = count;
            doc["mean"] = mean;
            doc["stDev"] = stDev;
            doc["valid"] = valid;
            auto jBuffer = doc.createNestedArray("values");
            for (auto &val: buffer) {
                jBuffer.add(val);
            }
            String json;
            serializeJson(doc, json);
            request->send(200, "application/json", json);
        };
    }

    static ArRequestHandlerFunction HandleStateRead(IoData &item) {
        return [&item](AsyncWebServerRequest *request) {
            auto state = item.Read();
            request->send(200, "application/json", R"({"state":")" + String(state) + R"("})");
        };
    }

public:
    inline bool Init() {
        if (m_init)
            return true;
        if (!network.Init()) {
            logger.println("[WEBSERVER] Network not initialized");
            return false;
        }

        logger.print("IP address: http://");
        logger.print(WiFi.localIP());
        logger.print(" (http://");
        logger.print(WiFi.getHostname());
        logger.println(")");

        // Not Found
        server.onNotFound([](AsyncWebServerRequest *request) { request->send(404, "text/plain", "Not found"); });

        // Route for root / web page
        // server.on("/", HTTP_GET, [](AsyncWebServerRequest *request) { request->send(SPIFFS, "/index.html"); });
        server.serveStatic("/", SPIFFS, "/wwwroot", "max-age=86400").setDefaultFile("index.html");

        server.on("/api/info", HTTP_GET, [](AsyncWebServerRequest *request) {
            request->send(200, "application/json",
                          String(R"({"chipId":")") + chipId() + R"(","version":")" VERSION R"("})");
        });

        server.on("/api/device/id", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", chipId()); });

        server.on("/api/logs", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", logger.Data()); });

        server.on("/ping", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", "pong"); });

        LoadFromConfig();

        server.begin();
        return m_init = true;
    }
} webserver;