#include<HX711_ADC.h>


//global variables for postition encoder
const int pinA = 5;// Connected to CLK on KY­040 D1
const int pinB = 4; // Connected to DT on KY­040 D2
//global variables for the force sensor
const int pinSCK = 14;//D5
const int pinDT = 12;//D6
//global variables for position encoder functionality
const double impulses = 30;
const double distance_per_rotation = 2;//change after measurement
const double gear_ratio = 3;
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
// function for position encoder reading
void encoder_position_reader();
void com_functions();
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
//LoadCell.setReverseOutput(); //uncomment to turn a negative output value to positive

}
void loop() 
{
  com_functions();
  encoder_position_reader();
  HX711_Loop();
  
}
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
      //Serial.println(plat_pos);
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
    else if(com_data == "FST")//force rest to 0//basically the tare function
    {
      
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
  //Serial.print("Encoder Position: ");
  //Serial.println(encoderPosCount);
  //Serial.println("Platform Position: ");
  plat_pos = distance_per_impulse*encoderPosCount;
  //Serial.println(plat_pos);
  }
  pinALast = aVal;

}

void HX711_Loop()
{
  LoadCell.update();
  weight_value =  LoadCell.getData();

}

  



