#include<HX711_ADC.h>
#include <EEPROM.h>

//global variables for postition encoder
const int pinA = 5;// Connected to CLK on KY­040 D1
const int pinB = 4; // Connected to DT on KY­040 D2
//global variables for the force sensor
const int pinSCK = 14;//D5
const int pinDT = 12;//D6
//global variables for position encoder functionality
const double impulses = 30;
const double distance_per_rotation = 3.2;//change after measurement
const double gear_ratio = 4;
int encoderPosCount = 0;
int pinALast;
int aVal;
boolean bCW;
const double distance_per_impulse = (1/impulses)*(distance_per_rotation)/gear_ratio;
float plat_pos = 0;
//global variables for force sensor
HX711_ADC LoadCell(pinDT,pinSCK);
boolean _tare = true;
unsigned long t =0;
float calibrationValue =100;
unsigned long stabilizingtime = 3000;//increasing the accuracy of the tare operation by increasing the value
float weight_value = 0;
const int samples = 10;//sampling during filtering and averaging for returning the signal
//global varaibles for communication protocol
String com_data;
// for the EEPROm part
double calibrationmap10X5 = 1;   //Variable to store data read from EEPROM.
int eepromAddress = 0; //adress in EEPROM to read and write 

void encoder_position_reader();
void com_functions();
void HX711_Loop();


void setup() 
{
pinMode (pinA,INPUT);
pinMode (pinB,INPUT);
pinMode(LED_BUILTIN, OUTPUT);

pinALast = digitalRead(pinA);
Serial.begin (9600);
delay(10);
LoadCell.begin();
LoadCell.start(stabilizingtime, _tare);
LoadCell.setSamplesInUse(samples);
// EEPROM calibration storage 

EEPROM.begin(512); // will round horribly fix this error
}


void loop() 
{
  com_functions();
  encoder_position_reader();
  HX711_Loop();
  
}

void HX711_Loop()
{
  LoadCell.update();
  weight_value =  LoadCell.getData();

}

//Helper Functions

void com_functions()
{
    if(Serial.available())
   {
    com_data = Serial.readStringUntil('\n');
    
    if(com_data=="ONN")
    {
      digitalWrite(LED_BUILTIN,LOW);
       
    }
    else if(com_data == "OFF")
    {
      digitalWrite(LED_BUILTIN, HIGH);
    }
    else if(com_data == "COM")
    {
      Serial.write("OK\n");
    }
    else if(com_data == "XRC")
    {

    }
    else if(com_data == "POS") // function to get the position of the platform
    {
      String myString = String(plat_pos);
      //Serial.println(plat_pos);
      char* buf1 = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf1, myString.length()+1);
      Serial.write(buf1);
      free(buf1);
      Serial.write('\n');
      
    }
   
    else if(com_data == "IMP")
    {
      String myString = String(encoderPosCount);
      char* buf2 = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf2, myString.length()+1);
      Serial.write(buf2);
      free(buf2);
      Serial.write('\n');
      
    }
    else if(com_data == "RST")//resets the position on the encoder to 0
    {
      plat_pos = 0;
      encoderPosCount = 0;
      Serial.write("SUC");
      Serial.write('\n');
    }
    else if(com_data == "CALVAL")//pulls the calibration constant from the gui and sets it in the application part
    {
        Serial.write("CALRQS\n");//implement in c# part
        //Serial.write('\n');
        String calibration_value_string = Serial.readStringUntil('\n');
        float calibration_value = calibration_value_string.toFloat();
        calibrationValue = calibration_value; 
    }
    else if(com_data == "TRE")//more accuarate slower tare //restarts the 
    {
      LoadCell.tare();
      Serial.write("SUC\n");
    }
    else if(com_data == "CAL")//preforms the calibration operation
    {
      LoadCell.setCalFactor(calibrationValue);
      Serial.write("SUC");
      Serial.write('\n');
    }
    else if (com_data == "TREF")//tare fast
    {
      LoadCell.tareNoDelay();
    }
    else if(com_data == "WGH")//send the weight
    {
      String myString = String(weight_value);
      //Serial.println(weight_value);
      char* buf3 = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf3, myString.length()+1);
      Serial.write(buf3);
      free(buf3);
      Serial.write('\n');   
    }
    else if(com_data == "MPUL")//Micro-controller pulls data from application
    {
      //send request to application
      Serial.write("MAPREQ");//send calibrationmap request to application
      Serial.write('\n'); 
      //receive value
      String calibration_value_string = Serial.readStringUntil('\n');
      calibrationmap10X5 = calibration_value_string.toDouble();
      //write value to memory
      EEPROM.put(eepromAddress,calibrationmap10X5);
      EEPROM.commit();

    }
    else if(com_data == "APUL")//Application pulls data from microcontroller
    {
      double calmap;//write to eeprom when power is on but when off will chnage vlaue to 16711680 (weird)
      EEPROM.get(eepromAddress,calmap);
      String myString = String(calmap);
      char* buf = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf, myString.length()+1);
      Serial.write(buf);
      free(buf);
      Serial.write('\n');
    }
    else
    {
      Serial.write("ERR\n");
    }
   }   
}


void encoder_position_reader()
{ 
  aVal = digitalRead(pinA);
if (aVal != pinALast )
{ 
if (digitalRead(pinB) != aVal)
  { 
    encoderPosCount ++;
    bCW = true;
  } 
  else 
  {
  bCW = false;
  encoderPosCount--;
  }
  if (bCW)
  {
 
  }
  else
  {
 
  }
  plat_pos = distance_per_impulse*encoderPosCount;
 
  }
  pinALast = aVal;

}
  



