#pragma once

// Import required libraries
#include "ESPAsyncWebServer.h"
#include "info.hpp"
#include "log.hpp"
#include "specs.h"
#include "config.hpp"
#include "network.hpp"

class WebserverWrapper {
    bool m_init;
    // Set web server port number to 80
    AsyncWebServer server = AsyncWebServer(80);

    void LoadFromConfig() {
        for (auto &item: config.ioData) {
            switch (item.getMode()) {
                case OUTPUT: {
                    String url = String("/api/io/device/") + item.getName();
                    server.on(url.c_str(), HTTP_POST, HandleStateChange(item));
                    break;
                }
                case INPUT:
                    logger.println("Input not implemented yet");
                    break;
                default:
                    logger.println("Unknown mode: " + String(item.getMode()));
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
                          String(R"({ "chipId" : ")") + chipId() + "\", \"version\" : \"" VERSION "\" }");
        });

        server.on("/api/device/id", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", chipId()); });

        server.on("/api/logs", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", logger.Data()); });

        server.on("/test", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", "working :)"); });

        LoadFromConfig();

        server.begin();
        return m_init = true;
    }
} webserver;