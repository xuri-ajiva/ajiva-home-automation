#pragma once

// Import required libraries
#include "WiFi.h"
#include "ESPAsyncWebServer.h"
#include "SPIFFS.h"
#include "info.hpp"
#include "log.hpp"
#include "specs.h"


class WebserverWrapper {
    bool m_init;
// Set web server port number to 80
AsyncWebServer server(80);

inline bool readFile(const char *path, String &content)
{
    if (!SPIFFS.exists(path))
        return false;
    // Open file for reading
    File file = SPIFFS.open(path, "r");
    if (!file)
        return false;
    // Read content from file
    content = file.readString();
    // Close file
    file.close();
    return true;
}

inline void setupServer()
{
    // Initialize SPIFFS
    if (!SPIFFS.begin(true))
    {
        logger.println("An Error has occurred while mounting SPIFFS");
        return;
    }

    logger.println("Setup WiFi");
    // load ssid and password from SPIFFS
    String ssid;
    if (!readFile("/wifi/ssid.key", ssid))
    {
        logger.println("SSID file not found");
        return;
    }

    String password;
    if (!readFile("/wifi/password.key", password))
    {
        logger.println("Password file not found");
        return;
    }

    // Connect to Wi-Fi network with SSID and password
    logger.print("Connecting to ");
    logger.println(ssid);
    WiFi.begin(ssid.c_str(), password.c_str());
    while (WiFi.status() != WL_CONNECTED)
    {
        delay(1000);
        Serial.print(WiFi.status());
        for (int i = 0; i < 20 && WiFi.status() != WL_CONNECTED; i++)
        {
            Serial.print(".");
            delay(100);
        }
        WiFi.reconnect();
    }
    // Print local IP address and start web server
    logger.println("");
    logger.println("WiFi connected.");
    logger.print("IP address: http://");
    logger.print(WiFi.localIP());
    logger.print(" (http://");
    logger.print(WiFi.getHostname());
    logger.println(")");

    // Route for root / web page
    server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
              { request->send(SPIFFS, "/index.html"); });

    server.on("/api/info", HTTP_GET, [](AsyncWebServerRequest *request)
              { request->send(200, "application/json", String("{ \"chipId\" : \"") + chipId() + "\", \"version\" : \"" VERSION "\" }"); });

    server.on("/api/device/id", HTTP_GET, [](AsyncWebServerRequest *request)
              { request->send(200, "text/plain", chipId()); });

        server.on("/test", HTTP_GET,
                  [](AsyncWebServerRequest *request) { request->send(200, "text/plain", "working :)"); });

        LoadFromConfig();

        server.begin();
        return m_init = true;
    }
} webserver;