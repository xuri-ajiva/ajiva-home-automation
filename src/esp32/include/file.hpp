#pragma once
#include "SPIFFS.h"

class FileWrapper {
    bool m_init;
public:
    bool Read(const char *path, const char *error, String &content) {
        logger.println("[FILESYSTEM] Reading file: " + String(path));
        if (!Init()) {
            return false;
        }
        File file = SPIFFS.open(path, "r");
        if (!file) {
            logger.println(error);
            return false;
        }
        content = file.readString();
        file.close();
        return true;
    }
    bool Init() {
        if(m_init) return true;
        // Initialize SPIFFS
        if (!SPIFFS.begin(true)) {
            logger.println("[FILESYSTEM] An Error has occurred while mounting SPIFFS");
            return false;
        }
        return m_init = true;
    }
} file;
