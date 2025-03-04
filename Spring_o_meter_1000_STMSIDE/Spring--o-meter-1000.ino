//global variables for postition encoder
const int pinA = 2;// Connected to CLK on KY­040
const int pinB = 4; // Connected to DT on KY­040 

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
}
void loop() 
{
  
  encoder_position_reader();
  com_functions();
  
}
void com_functions()
{
    if(Serial.available())
   {
    com_data = Serial.readStringUntil('\n');
    
    if(com_data=="ONN")
    {
      digitalWrite(LED_BUILTIN,HIGH);
       
    }
    else if(com_data == "OFF")
    {
      digitalWrite(LED_BUILTIN, LOW);
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
      char* buf = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf, myString.length()+1);
      Serial.write(buf);
      Serial.write('\n');
      free(buf);


    
    }
    else if(com_data == "FRC")
    {
      
    }
    else if(com_data = "IMP")
    {
       String myString = String(encoderPosCount);
      //Serial.println(plat_pos);
      char* buf = (char*) malloc(sizeof(char)*myString.length()+1);
      myString.toCharArray(buf, myString.length()+1);
      Serial.write(buf);
      Serial.write('\n');
      free(buf);
      
      
    }
    else if(com_data = "RST")
    {
      encoderPosCount = 0;
      Serial.write("SUC");
      Serial.write('\n');
    }
   
   }   
}


void encoder_position_reader()
{


  
  aVal = digitalRead(pinA);
if (aVal != pinALast )
{ // Means the knob is rotating
// if the knob is rotating, we need to determine direction
// We do that by reading pin B.


if (digitalRead(pinB) != aVal)
  { // Means pin A Changed first ­ We're Rotating Clockwise
    encoderPosCount ++;
    bCW = true;
  } 
  else 
  {// Otherwise B changed first and we're moving CCW
  bCW = false;
  encoderPosCount--;
  }
 // Serial.print ("Rotated: ");
  if (bCW)
  {
  //Serial.println ("clockwise");
  }
  else
  {
 // Serial.println("counterclockwise");
  }

  //Serial.print("Encoder Position: ");
  //Serial.println(encoderPosCount);
  //Serial.println("Platform Position: ");
  plat_pos = distance_per_impulse*encoderPosCount;
  //Serial.println(plat_pos);
 
  }
  pinALast = aVal;

}