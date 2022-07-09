using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBehiavourManager : MonoBehaviour
{


    public void Canseethis(Spell sp, TargetData data)
    {
        Debug.Log(sp.name);
    }

    public void SingleTargetHeal(Spell sp, TargetData data)
    {

        data.target.RecieveHealing(sp.amount, sp.allowOverheal);

    }

    public void HealNearby(Spell sp, TargetData data)
    {
        
       



    }


}
