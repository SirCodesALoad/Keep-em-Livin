using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private float healthMax = 100f;
    [SerializeField] private float health = 100f;

    [SerializeField] private GameObject owner;

    private void Awake()
    {
        KeepEmLivinEvents.healTarget.AddListener(RecieveHealing);
    }

    void RecieveHealing(HealEventData data)
    {
        //We've detected a HealEvent, let's check if we're getting healed.
        if (data.target == owner)
        {

            //We're getting healed, let's increase our health! Or not if we've got too much or too little.
            if (health > healthMax && data.allowOverhealing == true && health > 0){
                health += data.amount;
            }
            else if (health > 0 && health < healthMax)
            {

                switch (data.allowOverhealing)
                { 
                    case true:
                        health += data.amount;
                        break;
                    case false:
                        health += data.amount;
                        if (health > healthMax)
                        {
                            health = healthMax;
                        }
                        break;
                }

            }
        }
    }


}
