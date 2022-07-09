using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanneledHeal : SpellBehiavour
{

    [SerializeField] private float amountToHealPerTick = 5f;

    public override void DuringCast()
    {
        target.GetCharacterAt(0).RecieveHealing(amountToHealPerTick, false);


    }


}
