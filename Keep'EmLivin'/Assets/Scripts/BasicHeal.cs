using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHeal : SpellBehiavour
{

    [SerializeField]private float amountToHeal = 50f;


    public override void FinishCast()
    {
        target.GetCharacterAt(0).RecieveHealing(amountToHeal, false);
        Debug.Log("Basic Heal went off!");

        base.FinishCast();

    }

}
