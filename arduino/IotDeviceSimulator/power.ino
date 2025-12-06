/**
 * BQ27220 Battery Fuel Gauge Test
 * 
 * This code reads battery data from BQ27220YZFR chip via I2C
 * and prints all available information to serial monitor.
 * 
 * Hardware Connections:
 * - BQ27220 SDA -> Arduino Nano A4 (SDA)
 * - BQ27220 SCL -> Arduino Nano A5 (SCL)
 * - BQ27220 VCC -> 3.3V or 5V (check your module)
 * - BQ27220 GND -> GND
 * 
 * Required Library:
 * Install "Kode_BQ27220" from Arduino Library Manager
 */

#include <Wire.h>
#include <BQ27220.h>

// Create BQ27220 instance
BQ27220 batteryMonitor;

// Update interval (milliseconds)
const unsigned long UPDATE_INTERVAL = 2000; // 2 seconds
unsigned long lastUpdate = 0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  while (!Serial) {
    ; // Wait for serial port to connect
  }
  
  Serial.println("=====================================");
  Serial.println("BQ27220 Battery Fuel Gauge Test");
  Serial.println("=====================================");
  Serial.println();
  
  // Initialize I2C
  Wire.begin();
  
  // Initialize BQ27220
  Serial.println("Initializing BQ27220...");
  if (batteryMonitor.begin()) {
    Serial.println("✓ BQ27220 initialized successfully!");
  } else {
    Serial.println("✗ Failed to initialize BQ27220!");
    Serial.println("Check connections:");
    Serial.println("  - SDA to A4");
    Serial.println("  - SCL to A5");
    Serial.println("  - VCC to 3.3V/5V");
    Serial.println("  - GND to GND");
  }
  
  Serial.println();
  delay(1000);
}

void loop() {
  // Update and print data at specified interval
  if (millis() - lastUpdate >= UPDATE_INTERVAL) {
    printBatteryData();
    lastUpdate = millis();
  }
}

void printBatteryData() {
  Serial.println("─────────────────────────────────────");
  Serial.print("Time: ");
  Serial.print(millis() / 1000);
  Serial.println(" seconds");
  Serial.println("─────────────────────────────────────");
  
  // Read and print State of Charge (SOC) - Battery percentage
  int soc = batteryMonitor.readStateOfChargePercent();
  Serial.print("State of Charge (SOC):      ");
  Serial.print(soc);
  Serial.println(" %");
  
  // Read and print Voltage
  int voltage = batteryMonitor.readVoltageMillivolts();
  Serial.print("Battery Voltage:            ");
  Serial.print(voltage);
  Serial.print(" mV (");
  Serial.print(voltage / 1000.0, 3);
  Serial.println(" V)");
  
  // Read and print Current (positive = charging, negative = discharging)
  int current = batteryMonitor.readCurrentMilliamps();
  Serial.print("Battery Current:            ");
  Serial.print(current);
  Serial.print(" mA (");
  if (current > 0) {
    Serial.print("Charging");
  } else if (current < 0) {
    Serial.print("Discharging");
  } else {
    Serial.print("Idle");
  }
  Serial.println(")");
  
  // Read and print Remaining Capacity
  int remainingCapacity = batteryMonitor.readRemainingCapacitymAh();
  Serial.print("Remaining Capacity:         ");
  Serial.print(remainingCapacity);
  Serial.println(" mAh");
  
  // Read and print Full Charge Capacity
  int fullChargeCapacity = batteryMonitor.readFullChargeCapacitymAh();
  Serial.print("Full Charge Capacity:       ");
  Serial.print(fullChargeCapacity);
  Serial.println(" mAh");
  
  // Read and print Temperature (returned in 0.1K units, convert to Celsius)
  int tempRaw = batteryMonitor.readTemperatureCelsius();
  float temperature = tempRaw / 10.0;
  Serial.print("Battery Temperature:        ");
  Serial.print(temperature, 1);
  Serial.print(" °C (");
  Serial.print(temperature * 9.0 / 5.0 + 32.0, 1);
  Serial.println(" °F)");
  
  // Read and print State of Health (SOH)
  int soh = batteryMonitor.readStateOfHealthPercent();
  Serial.print("State of Health (SOH):      ");
  Serial.print(soh);
  Serial.println(" %");
  
  // Calculate and print power consumption
  float power = (voltage / 1000.0) * (abs(current) / 1000.0);
  Serial.print("Power:                      ");
  Serial.print(power, 3);
  Serial.println(" W");
  
  // Estimate time remaining (if discharging)
  if (current < 0 && current != 0) {
    float timeRemaining = (float)remainingCapacity / (float)abs(current);
    Serial.print("Time Remaining:             ");
    Serial.print(timeRemaining, 2);
    Serial.print(" hours (");
    Serial.print(timeRemaining * 60, 0);
    Serial.println(" minutes)");
  } else if (current > 0) {
    float timeToFull = (float)(fullChargeCapacity - remainingCapacity) / (float)current;
    Serial.print("Time to Full Charge:        ");
    Serial.print(timeToFull, 2);
    Serial.print(" hours (");
    Serial.print(timeToFull * 60, 0);
    Serial.println(" minutes)");
  }
  
  // Print battery status indicator
  Serial.print("\nBattery Status: ");
  if (soc > 80) {
    Serial.println("█████████░ Excellent");
  } else if (soc > 60) {
    Serial.println("███████░░░ Good");
  } else if (soc > 40) {
    Serial.println("█████░░░░░ Medium");
  } else if (soc > 20) {
    Serial.println("███░░░░░░░ Low - Please Charge");
  } else {
    Serial.println("█░░░░░░░░░ Critical - Charge Now!");
  }
  
  Serial.println();
}
