

void loop() {

  SwitchTorsoFront.loop(); // Must call regarding the debouncing of switch
  SwitchTorsoMiddle.loop();
  SwitchTorsoBack.loop();
  SwitchBasePlus.loop();
  SwitchBaseMinus.loop();
//  Display_IO();
//  if(millis() >  targetmillis)
//  {
//    targetmillis = millis() + interval;
//    Serial.println(ams5600.readAngle());
//  }


  
//  int analogReading = analogRead(FORCE_SENSOR_PIN);
  // as5600.update();
  if (Serial.available() > 0)
  {
    datastring = Serial.readString();//voorbeeld: 12000 2200 1230
    
    SerialParse();
    if (c1 == 'h' && c2 == 'o')
      homebool = true;
    if (c1 == 'm')
    {
      moving  = true;
      digitalWrite(50, HIGH);
      move_sequence_index = 0;
      Serial.println(ams5600.readAngle());
    }
  }
//  if ((strs[0][0] == 'h' && strs[0][1] == 'o') && homebool == true)
//    homingf();
  
  VaccuumUitvoer();
  //CheckAngleSensor1();
  if(move_sequence_index == 0)
    StepperJ1.moveTo(stappenJ1);
  if(move_sequence_index == 1)
    StepperJ2.moveTo(stappenJ2);
  if(move_sequence_index == 2)
    StepperJ3.moveTo(stappenJ3);
  if(move_sequence_index == 3)
    StepperJ4.moveTo(stappenJ4);
  if(if_steppers_are_done() && moving == true)
    move_sequence_index++;
  if(move_sequence_index == 4)
  {
    moving = false;
    move_sequence_index = 0;
    Serial.println("Stepper sequence done");
    if(VacuumEind == 0)
      Serial.println("f"); // movement succeded, ready for new serial.read
  }

//  if(SwitchUnexpected())
//  {
//    StepperJ1.moveTo(stappenJ1);
//    StepperJ2.moveTo(stappenJ2);
//    StepperJ3.moveTo(stappenJ3);
//    StepperJ4.moveTo(stappenJ4);
//  }
      
  StepperJ1.run();
  StepperJ2.run();
  StepperJ3.run();
  StepperJ4.run();
  if (if_steppers_are_done())//load ofF motors when finished
  {
    digitalWrite(50, LOW);
//    SERIAL.println(ams5600.readAngle());//blocks run()
  }
}
