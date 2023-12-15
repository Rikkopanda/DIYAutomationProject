
int Display_IO()
{
  int a = 0;

  if (SwitchTorsoFrontState != SwitchTorsoFront.getState() || SwitchTorsoMiddleState  != SwitchTorsoMiddle.getState()
      || SwitchTorsoBackState    != SwitchTorsoBack.getState() || SwitchBasePlusState     != SwitchBasePlus.getState()
      || SwitchBaseMinusState    != SwitchBaseMinus.getState())
    a = 1;
  else
    a = 0;

  if (a)
  {
    SwitchTorsoFrontState  = SwitchTorsoFront.getState();
    SwitchTorsoMiddleState  = SwitchTorsoMiddle.getState();
    SwitchTorsoBackState    = SwitchTorsoBack.getState();
    SwitchBasePlusState     = SwitchBasePlus.getState();
    SwitchBaseMinusState    = SwitchBaseMinus.getState();
        p("I%d%d%d%d\n", SwitchBaseMinusState ,SwitchBasePlusState, SwitchTorsoFrontState, SwitchTorsoMiddleState, SwitchTorsoBackState);
       // p("O%d%d%d%d\n", digitalRead(38), digitalRead(39), digitalRead(40), digitalRead(41));
  }
  return (0);
}
