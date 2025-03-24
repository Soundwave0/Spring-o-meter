## Spring-O-Meter1000
**Ensure that ESP is configured in Arduino IDE with ESPduino package** <br>
https://arduino.esp8266.com/stable/package_esp8266com_index.json <br> in arudino IDE add the following path <br>
# Introduction

Presented here is the device Spring-O-Meter 1000, created for the class FMDII at the faculty of Mechatronics. The aim of this project was to measure the spring constant in both tension and compression springs by building some device. Being a Mechatronic device it contains Electrical, Software and Mechanical Components that work in synchrony. The following documentation will contain a description of the 3 components as well as the usage of the device. For the the Step and Program Files go to  
https://github.com/Soundwave0/Spring-o-meter.git

# Specifications

| Spec | Value | Unit |
|------|-------|------|
| Range Comp. | 90 | mm |
| Range Ten. | 115 | mm |
| MAX Comp pos | 110 | mm |
| MIN Comp. pos | 20 | mm |
| MAX Ten. pos | 135 | mm |
| MIN Ten. pos | 10 | mm |
| Max Comp. Di | 19 | mm |
| Min Comp. Di | 3 | mm |
| MAX force | 9.806 | N |
| MAX cal. weight | 950 | g |
| Min cal. weight | 3 | g |
| Disp per Rotation | 2 | mm |
| Gear Ratio(lead:enc.) | 1:3 |  |
| Encoder Disp. Resolution | 0.022 | mm |
| Force sensor Accuracy | ≈0.1 | % |
| K Accuracy | ≈0.015 | N/mm |

# Usage

1. Download the Spring-O-Meter GUI code either from the git-hub link or install from the executable (if you face any issues in the git-hub version try turning off signing)
2. Plug in the Micro-USB cable into the Spring-O-Meter and then into the USB port in your computer, see if the blue led begins to shine through the support on the bottom. Remember if your Micro-USB cable does not have data transfer capabilities it will turn on but no serial communication will be possible
3. Go to Device Manager-> Ports and check to see if you can see the Silicon Labs CP210x, remember the COM port it is connected to 
4. Launch the GUI and click on "COM Configuration" and select the COM that you found in the previous step
5. Press Check COM if it informs you that everything is good then continue if not restart the application and recheck the COM in device manager
6. If you would like to Calibrate your sensor then turn on Calibration , wait until the button settles then place your Spring-O-Meter on its head then press "Calibrate Zero", after this enter the weight of your test weight then place it, then press "Complete Calibration". Once this button is changed to "Calibrated" continue
7. If you don't want to Calibrate but instead want to use a previous value of Calibration taken from EEPROM, then press Use Prev Cal instead of doing Step 6
8. Next place your Spring-O-Meter in the testing Position, install your spring and press tare, then reset 
9. If you would like to measure the point at which the spring begins to exert a force on the sensor to set your zero better, turn on the kickpoint by pressing the button next to kickpoint label turning the OFF to ON. Move the Stage to tense or compress the spring until the kickpoint is no longer null. Once this happens, move your stage to kick point, tare and reset
10. Now you can begin sampling, move your stage a bit, wait for the measurement to settle then press sample, repeat for desired number of samples then press Graph Spring
11. To reset everything and repeat another trial click New Trial

# Parts

## Stock-Parts

| NAME | SPEC | QTY |
|------|------|-----|
| LSU |  | 1 |
| ESP8266 | NodeMCU 12-E | 1 |
| Encoder 30IMP | KY-040 | 1 |
| Load-Cell 1Kg | NA27 | 1 |
| Load-Cell Amp | HX711 | 1 |
| M2.5x40 |  | 2 |
| M6x12 | ISO4762 | 4 |
| M2.5 pin |  | 1 |
| micro-USB cable | W/ Data Bus | 1 |
| Breadboard cables |  | 12 |

## 3D printed parts

| NAME | SPEC | QTY |
|------|------|-----|
| Spur Gear1 | 3d printed | 1 |
| Spur Gear Clamp | 3d printed | 1 |
| Spur Gear 2 | 3d printed | 1 |
| Base 1 | 3d printed | 1 |
| Base 2 | 3d printed | 1 |
| Base Lid | 3d printed | 1 |
| Cable Lid | 3d printed | 1 |
| Clamp | 3d printed | 1 |
| Plate Big | 3d printed | 1 |
| Plate Small Gear | 3d printed | 1 |
| Plate Small | 3d printed | 1 |
