#include <Arduino.h>
#include "webserver.hpp"
#include "info.hpp"
#include "config.hpp"
#include "log.hpp"

#define LED 2

void setup() {
    // put your setup code here, to run once:
    logger.Init(115200);
    config.Load("/config");
    printInfo();
    webserver.Init();
}

void loop() {
    delay(1000);
}