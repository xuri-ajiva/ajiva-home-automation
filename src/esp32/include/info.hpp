#pragma once

#include <Arduino.h>
#include "log.hpp"

#include "specs.h"

String chipId()
{
    String chipId = String((uint32_t)ESP.getEfuseMac(), HEX);
    chipId.toUpperCase();
    return chipId;
}

void printInfo()
{
    esp_chip_info_t chip_info;
    esp_chip_info(&chip_info);

    logger.println("[INFO] Hardware info");
    logger.printf("[INFO] %d cores Wifi %s%s\n", chip_info.cores, (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
                  (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");
    logger.printf("[INFO] Silicon revision: %d\n", chip_info.revision);
    logger.printf("[INFO] %dMB %s flash\n", spi_flash_get_chip_size() / (1024 * 1024),
                  (chip_info.features & CHIP_FEATURE_EMB_FLASH) ? "embeded" : "external");

    // get chip id
    logger.printf("[INFO] Chip id: %s\n", chipId().c_str());
    logger.println("[INFO] Version: " VERSION);
}
