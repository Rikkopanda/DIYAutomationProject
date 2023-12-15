//#include "functions.h"
#include <EEPROM.h>
#include <Wire.h>
#include <stdarg.h>
#include <ezButton.h>
#include <loopTimer.h>
#include <millisDelay.h>
#include <ezButton.h>
#include <SimpleFOC.h>
#include "AccelStepper.h"
#include <stdarg.h>
#include <Wire.h>
#include <AS5600.h>
#ifdef ARDUINO_SAMD_VARIANT_COMPLIANCE
  #define SERIAL SerialUSB
  #define SYS_VOL   3.3
#else
  #define SERIAL Serial
  #define SYS_VOL   5
#endif

AS5600 ams5600;

// AccelStepper Setup
int pulseperrev = 400;
//int pulsepersec =
//int revpersec = pulsepersec/pulseperrev;
int gbratio = 40;
//int rpm = revpersec * 60;
//int angularvelocityoutput =
const int stepPin1 = 22;
const int directionPin1 = 23;
const int enablePin1 = 5;

#define FORCE_SENSOR_PIN A0

AccelStepper StepperJ1(AccelStepper::FULL4WIRE, 22, 23, 24, 25);
AccelStepper StepperJ2(AccelStepper::FULL4WIRE, 29, 28, 27, 26);
AccelStepper StepperJ3(AccelStepper::FULL4WIRE, 30, 31, 32, 33);
AccelStepper StepperJ4(AccelStepper::FULL4WIRE, 35, 36, 37, 38);

int StappenPositie;  // Used to store the X value entered in the Serial Monitor
bool move_finished = true;  // Used to check if move is completed
bool moving = false;
int currentstep = 0;
String datastring = "";
//char DataCharArray[20];
String dataJ1;
String dataJ2;
String dataJ3;
String dataJ4;
int stappenJ1;
int stappenJ2;
int stappenJ3;
int stappenJ4;
int Huidigestap;
int indexhoming = 0;
char c1;
char c2;
char c3;
char c4;
char c5;
String AngleString;
bool HitSwitchBaseMinusBool = false;
bool HitSwitchBasePlusBool = false;
bool HitSwitchTorsoBackBool = false;
bool HitSwitchTorsoMiddleBool = false;
bool HitSwitchTorsoFrontBool = false;
String strs[20];
int StringCount = 0;
bool homebool = false;
bool VaccuumBool = false;
int VaccuumDuratie = 0;
unsigned long VacuumEind = 0;
long interval = 2000;
long targetmillis;
int move_sequence_index = 0;
//volatile int buttonState = 0;         // variable for reading the pushbutton status
int SwitchTorsoFrontState;
int SwitchTorsoMiddleState;
int SwitchTorsoBackState;
int SwitchBasePlusState;
int SwitchBaseMinusState;
int read_from_address_value;
ezButton SwitchTorsoFront(46);
ezButton SwitchTorsoMiddle(42);
ezButton EmptySwitch(43);
ezButton SwitchTorsoBack(47);
ezButton SwitchBasePlus(44);
ezButton SwitchBaseMinus(45);




int if_steppers_are_done()
{
  if (StepperJ1.distanceToGo() == 0 && StepperJ2.distanceToGo() == 0 && StepperJ3.distanceToGo() == 0 && StepperJ4.distanceToGo() == 0)
  {
    return 1;
  }
  return 0;
}
void VaccuumUitvoer()
{
  if (VaccuumBool == true)
  {
    VacuumEind = millis() + (VaccuumDuratie * 1000);
    VaccuumBool = false;
  }
  if (VacuumEind >= millis())
  {
    digitalWrite(38, LOW);
    digitalWrite(39, LOW);
  }
  if (VacuumEind < millis())
    VacuumEind = 0;
}

void homingf()
{
  stappenJ1 = 500;
  StepperJ1.move(stappenJ1);
  StepperJ1.run();
  homebool = true;
}
int SwitchUnexpected()
{
  homebool = false;
  if (SwitchBaseMinusState == HIGH && HitSwitchBaseMinusBool == false) // does this only once becuase bool = true
  {
    Serial.println("SwitchBaseMinusState: TOUCHED");
    if (StepperJ1.targetPosition() > StepperJ1.currentPosition()) // moving in plus direction
      stappenJ1 = StepperJ1.currentPosition() - 100 ;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ1.targetPosition() < StepperJ1.currentPosition()) // moving in minus direction
      stappenJ1 = StepperJ1.currentPosition() + 100;//last moveTo in loop() gets replaced by currenPos
    HitSwitchBaseMinusBool = true;
    return (1);
  }
  if (SwitchBaseMinusState == LOW)
    HitSwitchBaseMinusBool = false;

  if (SwitchBasePlusState == HIGH && HitSwitchBasePlusBool == false) // does this only once becuase bool = true
  {
    Serial.println("SwitchBasePlusState: TOUCHED");
    if (StepperJ1.targetPosition() > StepperJ1.currentPosition()) // moving in plus direction
      stappenJ3 = StepperJ1.currentPosition() - 100;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ1.targetPosition() < StepperJ1.currentPosition()) // moving in minus direction
      stappenJ3 = StepperJ1.currentPosition() + 100;//last moveTo in loop() gets replaced by currenPos
    HitSwitchBasePlusBool = true;
    return (1);
  }
  if (SwitchBasePlusState == LOW)
    HitSwitchBasePlusBool = false;

  if (SwitchTorsoBackState == HIGH && HitSwitchTorsoBackBool == false) // does this only once becuase bool = true
  {
    Serial.println("SwitchTorsoBackState: TOUCHED");
    if (StepperJ2.targetPosition() > StepperJ2.currentPosition()) // moving in plus direction
      stappenJ2 = StepperJ1.currentPosition() - 100;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ2.targetPosition() < StepperJ2.currentPosition()) // moving in minus direction
      stappenJ2 = StepperJ1.currentPosition() + 100;//last moveTo in loop() gets replaced by currenPos
    HitSwitchTorsoBackBool = true;
    return (1);
  }
  if (SwitchTorsoBackState == LOW)
    HitSwitchTorsoBackBool = false;


  if (SwitchTorsoMiddleState == HIGH && HitSwitchTorsoMiddleBool == false) // does this only once becuase bool = true
  {
    Serial.println("SwitchTorsoMiddleState: TOUCHED");
    if (StepperJ2.targetPosition() > StepperJ2.currentPosition()) // moving in plus direction
      stappenJ2 = StepperJ1.currentPosition() - 100;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ2.targetPosition() < StepperJ2.currentPosition()) // moving in minus direction
      stappenJ2 = StepperJ1.currentPosition() + 100;//last moveTo in loop() gets replaced by currenPos
    HitSwitchTorsoMiddleBool = true;
    return (1);
  }
  if (SwitchTorsoMiddleState == LOW)
    HitSwitchTorsoMiddleBool = false;


  if (SwitchTorsoFrontState == HIGH && HitSwitchTorsoFrontBool == false) // does this only once becuase bool = true
  {
    Serial.println("SwitchTorsoFrontState: TOUCHED");
    if (StepperJ3.targetPosition() > StepperJ3.currentPosition()) // moving in plus direction
      stappenJ3 = StepperJ3.currentPosition() - 100;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ3.targetPosition() < StepperJ3.currentPosition()) // moving in minus direction
      stappenJ3 = StepperJ3.currentPosition() + 100;//last moveTo m in loop() gets replaced by currenPos
    HitSwitchTorsoFrontBool = true;
    return (1);
  }
  if (SwitchTorsoFrontState == LOW)
    HitSwitchTorsoFrontBool = false;

  //StepperJ1.stop();// sets new target position and stops to it, using acceleration and speed set
  //U can also stop(not excecute) the run() function in the loop, this cause imediate stop.
  return (0);
}

void CheckAngleSensor1() {
  static unsigned long timer = 0;
  unsigned long interval = 1000;
  if (millis() - timer >= interval)
  {
    timer = millis();
    Serial.println(StepperJ1.currentPosition());
    //Serial.println(as5600.getAngle());
  }
}
void CheckFinished()
{
  //If move is completed display message on Serial Monitor
  //if () {
  Serial.println("COMPLETED! ");
  Serial.println("stappositie:");
  Serial.println(stappenJ1);
  Serial.println("Sensorpositie:");
  //Serial.println(as5600.getAngle());
  Serial.println("Enter Next Move Values (0,0 for reset): ");  // Get ready for new Serial monitor values
  //move_finished = true;  // Reset move variable
  //StepperJ1.disableOutputs();
  //moving = false;
  //}
}
void SerialParse()
{
  move_sequence_index = 0;
  int str_len = datastring.length() + 1;
  char DataCharArray[datastring.length() + 1]; // Add +1 for null terminator
  datastring.toCharArray(DataCharArray, sizeof(DataCharArray));
  c1 = DataCharArray[0];
  c2 = DataCharArray[1];
  int StringCount = 0;//maakt telling weer 0 wanneer nieuwe read,
  while (datastring.length() > 0)
  {
    int index = datastring.indexOf(' ');//vind plek waar ' ' is
    if (index == -1) // No space found
    {
      strs[StringCount++] = datastring;//wijst toe aan huidig stringcount en verhoogt stringcount erna
      break;
    }
    else
    {
      strs[StringCount++] = datastring.substring(0, index);//wijst substring toe aan huidig stringcountpositie en verhoogt stringcount erna
      //input 1st: 5000' '8000' '3000  -> strs[0]=5000(' '),datastring = 8000' '3000
      //2: 8000' '3000  -> strs[1]=8000(' '),datastring = 3000
      //3: 3000  -> strs[2]=3000(index == -1),datastring = 3000
      //4: 3000  -> strs[3]=3000(index == -1),datastring = 3000
      //etc
      datastring = datastring.substring(index + 1); //vervangt originele string door originele startende bij index+1. En dus voert hierna nog eens uit maar dan zonder eerste gedeelte
    }
  }
  dataJ1 = strs[1];
  dataJ2 = strs[2];
  dataJ3 = strs[3];
  dataJ4 = strs[4];
  Serial.println("Parsing..");
  if (strs[5] == "V")
    VaccuumBool = true;
  VaccuumDuratie = strs[6].toInt();

  // Show the resulting substrings
  Serial.println(dataJ1);
  Serial.println(dataJ2);
  Serial.println(dataJ3);
  Serial.println(dataJ4);
  Serial.println(strs[4]);
  Serial.println(strs[5]);
  stappenJ1 = dataJ1.toInt();
  stappenJ2 = dataJ2.toInt();
  stappenJ3 = dataJ3.toInt();
  stappenJ4 = dataJ4.toInt();
  Serial.println("running");
  //2 123 222 23 V 23 0 x x
  //J1  J2  J3  J4 Vacuum?  DuratieVac? Conditie1 Conditie2
  //How code enters serialbuffer
}

int HomingFunction()
{
  int initialhoming = -1;
  stappenJ1 = 0;
  //while(SwitchBaseMinusState == LOW)// while is HIGH, activate = LOW
  //{
  stappenJ1 = stappenJ1 - 100;
  StepperJ1.move(-100);
  initialhoming = initialhoming - 100;
  if (SwitchBaseMinusState == HIGH) // while is HIGH, activate = LOW
  {
    Serial.println("HomedJ1");
    StepperJ1.setCurrentPosition(0);
    if (StepperJ1.targetPosition() > StepperJ1.currentPosition()) // moving in plus direction
      stappenJ1 = StepperJ1.currentPosition() - 100 ;//last moveTo in loop() gets replaced by currenPos
    if (StepperJ1.targetPosition() < StepperJ1.currentPosition()) // moving in minus direction
      stappenJ1 = StepperJ1.currentPosition() + 100;//last moveTo in loop() gets replaced by currenPos
    // offset from home
  }
  return (0);
}
