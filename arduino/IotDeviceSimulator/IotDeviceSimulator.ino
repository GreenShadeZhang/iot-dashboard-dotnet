/**
 * IoT Device Simulator for Arduino Nano
 * 
 * This firmware simulates an IoT device with the following features:
 * - Temperature and humidity sensor simulation
 * - Light control (LED)
 * - Fan speed control (PWM)
 * - Battery level monitoring
 * - Serial communication using custom protocol
 * 
 * Hardware connections:
 * - LED on pin 13 (built-in LED) or external LED on pin 8
 * - Fan (or LED to simulate fan) on pin 9 (PWM)
 * - Optional: DHT22 sensor for real temperature/humidity
 * 
 * Serial Protocol Format:
 * [StartByte][Length][Type][Data...][Checksum][EndByte]
 * StartByte: 0xAA
 * EndByte: 0x55
 * Checksum: XOR of Length, Type, and Data bytes
 */

// Pin definitions
const int LED_PIN = 8;          // Light control pin
const int FAN_PIN = 9;          // Fan control pin (PWM)
const int BUILTIN_LED_PIN = 13; // Built-in LED for status

// Protocol constants
const byte START_BYTE = 0xAA;
const byte END_BYTE = 0x55;

// Command types
enum CommandType {
  CMD_GET_STATE = 0x01,
  CMD_SET_LIGHT = 0x02,
  CMD_SET_FAN_SPEED = 0x03,
  CMD_GET_SENSOR_DATA = 0x04,
  CMD_RESET = 0x05,
  CMD_HEARTBEAT = 0x06
};

// Device state
struct DeviceState {
  float temperature;
  float humidity;
  bool lightOn;
  int fanSpeed;      // 0-100
  bool isOnline;
  int batteryLevel;  // 0-100
} state;

// Receive buffer
const int BUFFER_SIZE = 64;
byte receiveBuffer[BUFFER_SIZE];
int bufferIndex = 0;

// Timing
unsigned long lastUpdate = 0;
const unsigned long UPDATE_INTERVAL = 1000; // Update sensors every second

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  
  // Initialize pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(FAN_PIN, OUTPUT);
  pinMode(BUILTIN_LED_PIN, OUTPUT);
  
  // Initialize device state
  state.temperature = 25.0;
  state.humidity = 50.0;
  state.lightOn = false;
  state.fanSpeed = 0;
  state.isOnline = true;
  state.batteryLevel = 100;
  
  // Set initial outputs
  digitalWrite(LED_PIN, LOW);
  analogWrite(FAN_PIN, 0);
  
  // Blink built-in LED to indicate startup
  for (int i = 0; i < 3; i++) {
    digitalWrite(BUILTIN_LED_PIN, HIGH);
    delay(200);
    digitalWrite(BUILTIN_LED_PIN, LOW);
    delay(200);
  }
}

void loop() {
  // Update simulated sensor data
  if (millis() - lastUpdate >= UPDATE_INTERVAL) {
    updateSensors();
    lastUpdate = millis();
  }
  
  // Process incoming serial data
  if (Serial.available() > 0) {
    processSerialData();
  }
  
  // Blink built-in LED when online
  digitalWrite(BUILTIN_LED_PIN, (millis() / 1000) % 2);
}

void updateSensors() {
  // Simulate temperature changes (random walk)
  state.temperature += (random(-10, 11) / 10.0);
  if (state.temperature < 15.0) state.temperature = 15.0;
  if (state.temperature > 35.0) state.temperature = 35.0;
  
  // Simulate humidity changes
  state.humidity += (random(-5, 6) / 10.0);
  if (state.humidity < 30.0) state.humidity = 30.0;
  if (state.humidity > 70.0) state.humidity = 70.0;
  
  // Simulate battery drain
  state.batteryLevel -= random(0, 2);
  if (state.batteryLevel < 20) state.batteryLevel = 100; // Reset for demo
}

void processSerialData() {
  while (Serial.available() > 0) {
    byte b = Serial.read();
    
    // Look for start byte
    if (bufferIndex == 0 && b != START_BYTE) {
      continue;
    }
    
    receiveBuffer[bufferIndex++] = b;
    
    // Check if we have at least minimum packet size
    if (bufferIndex >= 5) {
      byte length = receiveBuffer[1];
      int totalLength = length + 4; // Start + Length + Data + Checksum + End
      
      // Check if we have complete packet
      if (bufferIndex >= totalLength) {
        if (receiveBuffer[totalLength - 1] == END_BYTE) {
          // Verify checksum
          byte checksum = 0;
          for (int i = 1; i < totalLength - 2; i++) {
            checksum ^= receiveBuffer[i];
          }
          
          if (checksum == receiveBuffer[totalLength - 2]) {
            // Valid packet, process command
            processCommand(receiveBuffer[2], &receiveBuffer[3], length - 1);
          }
        }
        
        // Reset buffer
        bufferIndex = 0;
      }
    }
    
    // Prevent buffer overflow
    if (bufferIndex >= BUFFER_SIZE) {
      bufferIndex = 0;
    }
  }
}

void processCommand(byte cmdType, byte* data, int dataLength) {
  switch (cmdType) {
    case CMD_GET_STATE:
      sendState();
      break;
      
    case CMD_SET_LIGHT:
      if (dataLength >= 1) {
        state.lightOn = (data[0] != 0);
        digitalWrite(LED_PIN, state.lightOn ? HIGH : LOW);
        sendState();
      }
      break;
      
    case CMD_SET_FAN_SPEED:
      if (dataLength >= 1) {
        state.fanSpeed = constrain(data[0], 0, 100);
        analogWrite(FAN_PIN, map(state.fanSpeed, 0, 100, 0, 255));
        sendState();
      }
      break;
      
    case CMD_GET_SENSOR_DATA:
      sendSensorData();
      break;
      
    case CMD_RESET:
      // Reset device state
      state.lightOn = false;
      state.fanSpeed = 0;
      digitalWrite(LED_PIN, LOW);
      analogWrite(FAN_PIN, 0);
      sendState();
      break;
      
    case CMD_HEARTBEAT:
      sendHeartbeat();
      break;
  }
}

void sendState() {
  byte data[6];
  data[0] = (byte)state.temperature;
  data[1] = (byte)state.humidity;
  data[2] = state.lightOn ? 1 : 0;
  data[3] = (byte)state.fanSpeed;
  data[4] = state.isOnline ? 1 : 0;
  data[5] = (byte)state.batteryLevel;
  
  sendPacket(CMD_GET_STATE, data, 6);
}

void sendSensorData() {
  byte data[2];
  data[0] = (byte)state.temperature;
  data[1] = (byte)state.humidity;
  
  sendPacket(CMD_GET_SENSOR_DATA, data, 2);
}

void sendHeartbeat() {
  sendPacket(CMD_HEARTBEAT, NULL, 0);
}

void sendPacket(byte cmdType, byte* data, int dataLength) {
  byte length = dataLength + 1; // Type + Data
  
  // Calculate checksum
  byte checksum = length ^ cmdType;
  for (int i = 0; i < dataLength; i++) {
    checksum ^= data[i];
  }
  
  // Send packet
  Serial.write(START_BYTE);
  Serial.write(length);
  Serial.write(cmdType);
  
  if (data != NULL && dataLength > 0) {
    Serial.write(data, dataLength);
  }
  
  Serial.write(checksum);
  Serial.write(END_BYTE);
}
