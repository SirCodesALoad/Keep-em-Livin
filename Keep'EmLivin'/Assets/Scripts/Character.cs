using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] protected readonly float healthMax = 100f;
    [SerializeField] protected float health = 100f;
    [SerializeField] protected readonly float armorBase = 20f;
    [SerializeField] protected float armor = 20f;
    public bool isAtZero = false;



    [SerializeField] protected Character whoDamagedMe;

    public bool IsAlive()
    {
        if (health <= 0)
        {
            return false;
        }
        return true;
    }

    public virtual void RecieveHealing(float amount, bool allowOverhealing)
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

    public virtual void RecieveDamage(float amount, bool bypassArmor, Character whoJustDamagedMe)
    {
        whoDamagedMe = whoJustDamagedMe;
        if (armor >= 0 && bypassArmor == false)
        {
            amount = amount * ((100 / (100 + armor)));
        }
        else
        {
            amount = amount * (2 - (100 / (100 - armor)));
        }

        health -= amount;
        if(health < 1)
        {
            health = 0;
            AtZero();
        }
        Debug.Log(whoDamagedMe.name + " just dealt: " + amount + " damage to " + gameObject.name);
    }

    public virtual void RecieveDamage(float amount, float armorPenetration, Character whoJustDamagedMe)
    {
        whoDamagedMe = whoJustDamagedMe;
        float tempArmor = armor - armorPenetration;
        if(tempArmor < 0)
        {
            tempArmor = 0;
        }
        if (tempArmor >= 0)
        {
            amount = amount * ((100 / (100 + tempArmor )));
        }
        else
        {
            amount = amount * (2 - (100 / (100 - tempArmor)));
        }

        health -= amount;
        if (health < 1)
        {
            health = 0;
            AtZero();
        }
        Debug.Log(whoDamagedMe.name + " just dealt: " + amount + " damage to " + gameObject.name);
    }


    public virtual void  AtZero()
    {
        //Here we will ethier play the die animation and destory the object or we'll have them go into a "Downed" state.
        isAtZero = true;
    }

}
