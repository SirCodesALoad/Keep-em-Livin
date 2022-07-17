using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ControlActions : PlayerActionSet
{
    public PlayerAction leftClick;
    public PlayerAction abilityOne;
    public PlayerAction abilityTwo;
    public PlayerAction abilityThree;
    public PlayerAction abilityFour;
    public PlayerAction swapAllyPostion;
    public PlayerAction restartGame;

    public ControlActions()
    {
        leftClick = CreatePlayerAction("Left Click");
        abilityOne = CreatePlayerAction("Action 1");
        abilityTwo = CreatePlayerAction("Action 2");
        swapAllyPostion = CreatePlayerAction("Action 5");
        restartGame = CreatePlayerAction("Action10");
    }


}
