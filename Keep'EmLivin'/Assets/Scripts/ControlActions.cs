using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ControlActions : PlayerActionSet
{
    public PlayerAction leftClick;
    public PlayerAction abilityOne;
    public PlayerAction abilityTwo;


    public ControlActions()
    {
        leftClick = CreatePlayerAction("Left Click");
        abilityOne = CreatePlayerAction("Action 1");
        abilityTwo = CreatePlayerAction("Action 2");
    }


}
