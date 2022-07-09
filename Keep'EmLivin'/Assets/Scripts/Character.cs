using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private readonly float healthMax = 100f;
    [SerializeField] private float health = 100f;
    public bool isAlly = false;


    [SerializeField] private GameObject owner;

    public AllyPostionSlot slotOccupied;


    public void RecieveHealing(float amount, bool allowOverhealing)
    {
        //We're getting healed, let's increase our health! Or not if we've got too much or too little.
        if (health > healthMax && allowOverhealing == true && health > 0)
        {
            health += amount;
        }
        else if (health > 0 && health < healthMax)
        {

            switch (allowOverhealing)
            {
                case true:
                    health += amount;
                    break;
                case false:
                    health += amount;
                    if (health > healthMax)
                    {
                        health = healthMax;
                    }
                    break;
            }

        }

        Debug.Log("Attempted to heal for: " + amount);

    }
}
