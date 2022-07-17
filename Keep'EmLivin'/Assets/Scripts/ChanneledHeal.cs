using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChanneledHeal : SpellBehiavour
{

    [SerializeField] private float amountToHealPerTick = 5f;

    public override void DuringCast()
    {
        if (target.GetCharacterAt(0).GetComponent<Slot>())
        {
            Slot[] slots = target.GetCharacterAt(0).GetComponent<Slot>().postion.GetAllSlots();
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].occupier != null)
                {
                    if (slots[i].occupier.GetComponent<Ally>() != null)
                    {
                        slots[i].occupier.GetComponent<Ally>().RecieveHealing(amountToHealPerTick, false);
                        Debug.Log("Succesfully Healed Slot buddy: " + slots[i].occupier.name);
                    }

                }
            }
        }
        else
        {
            target.GetCharacterAt(0).GetComponent<Ally>().RecieveHealing(amountToHealPerTick, false);
        }

       
        



    }


}
