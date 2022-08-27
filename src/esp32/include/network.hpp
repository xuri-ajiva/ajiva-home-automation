#pragma once

#include "WiFi.h"
#include "log.hpp"
#include "config.hpp"

class NetworkWrapper {
    bool m_init = false;
public:
    bool Init() {
        if (m_init) return true;
        if (!config.Init()) {
            logger.println("[NETWORK] Configuration not initialized");
            return false;
        }

        logger.println("Setup WiFi");
        logger.print("Connecting to ");
        logger.println(String(config.wifi.getSsid()) + "...");
        WiFi.begin(config.wifi.getSsid(), config.wifi.getPassword());
        while (WiFi.status() != WL_CONNECTED) {
            delay(1000);
            Serial.print(WiFi.status());
            for (int i = 0; i < 20 && WiFi.status() != WL_CONNECTED; i++) {
                Serial.print(".");
                delay(100);
            }
            WiFi.reconnect();
        }
        logger.println("");
        logger.print("WiFi connected. IP address: ");
        logger.println(WiFi.localIP());
        return m_init = true;
    }
} network;