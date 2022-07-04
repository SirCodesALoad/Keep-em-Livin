using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ControlActions : PlayerActionSet
{
    public PlayerAction leftClick;
    public PlayerAction abilityOne;


    public ControlActions()
    {
        leftClick = CreatePlayerAction("Left Click");
        abilityOne = CreatePlayerAction("Action 1");
    }


}
