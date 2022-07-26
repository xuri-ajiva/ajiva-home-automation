// Import required libraries
#include "WiFi.h"
#include "ESPAsyncWebServer.h"
#include "SPIFFS.h"

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
        Serial.println("An Error has occurred while mounting SPIFFS");
        return;
    }

    Serial.println("Setup WiFi");
    // load ssid and password from SPIFFS
    String ssid;
    if (!readFile("/wifi/ssid.key", ssid))
    {
        Serial.println("SSID file not found");
        return;
    }

    String password;
    if (!readFile("/wifi/password.key", password))
    {
        Serial.println("Password file not found");
        return;
    }

    // Connect to Wi-Fi network with SSID and password
    Serial.print("Connecting to ");
    Serial.println(ssid);
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
    Serial.println("");
    Serial.println("WiFi connected.");
    Serial.print("IP address: http://");
    Serial.print(WiFi.localIP());
    Serial.print(" (http://");
    Serial.print(WiFi.getHostname());
    Serial.println(")");

    // Route for root / web page
    server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
              { request->send(SPIFFS, "/index.html"); });

    server.begin();
}

inline void runServer()
{
}