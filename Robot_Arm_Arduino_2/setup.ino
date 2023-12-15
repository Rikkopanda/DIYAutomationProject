void setup() {
  Serial.begin(9600);  // Start the Serial monitor with speed of 9600 Bauds
//  Serial.print("Start");
//  ams5600.detectMagnet();

 
  pinMode(38, OUTPUT);  //Relay In1 normally closed
  pinMode(39, OUTPUT);  //Relay In2 normally closed
  pinMode(40, OUTPUT);  //Relay In3 normally closed
  pinMode(41, OUTPUT);  //Relay In4 normally closed
  pinMode(50, OUTPUT);
  digitalWrite(38, HIGH);//because its normally closed
  digitalWrite(39, HIGH);
  digitalWrite(40, HIGH);
  digitalWrite(41, HIGH);
  // initialize the pushbutton pin as an input:
  // pinMode(buttonPin, INPUT);
  // Attach an interrupt to the ISR vector
  // attachInterrupt(0, pin_ISR, CHANGE); // for Emergency Stop
  //  Set Max Speed and Acceleration of each Steppers
  StepperJ1.setMaxSpeed(1100.0);      // Set Max Speed of J1
  StepperJ1.setAcceleration(4000.0);  // Acceleration of J1
  StepperJ2.setMaxSpeed(1100.0);      // Set Max Speed of J1
  StepperJ2.setAcceleration(4000.0);
  StepperJ3.setMaxSpeed(1100.0);      // Set Max Speed of J1
  StepperJ3.setAcceleration(4000.0);  // Acceleration of J1
  SwitchTorsoFront.setDebounceTime(50);
  SwitchTorsoMiddle.setDebounceTime(50);
  SwitchTorsoBack.setDebounceTime(50);
  SwitchBasePlus.setDebounceTime(50);
  SwitchBaseMinus.setDebounceTime(50);
//  float field1;
//  EEPROM.get(0x7F, field1);
//  Serial.print("Value Read from 0x7F: ");
//  Serial.println(field1);
//  float a = ams5600.readAngle();
//  EEPROM.put(0x7F, a);
}
