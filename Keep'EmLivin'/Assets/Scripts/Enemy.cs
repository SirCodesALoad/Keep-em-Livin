using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{

    public float primaryDamage = 10f;
    public float specialDamage = 20f;

    private EnemyAI enemyAi;

    // Start is called before the first frame update
    void Start()
    {
        enemyAi = GetComponent<EnemyAI>();
    }


    public override void AtZero()
    {
        base.AtZero();
        Debug.Log("Enemy is dead!");
        KeepEmLivinEvents.enemyDeath.Invoke();
        enemyAi.Die();

    }



    public override void RecieveDamage(float amount, bool bypassArmor, Character whoJustDamagedMe)
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
        if (health < 1)
        {
            health = 0;
            AtZero();
        }
        Debug.Log(whoDamagedMe.name + " just dealt: " + amount + " damage to " + gameObject.name);
        enemyAi.RecievedDamage(whoDamagedMe);
    }


    public override void RecieveHealing(float amount, bool allowOverhealing)
    {
        //We're getting healed, let's increase our health! Or not if we've got too much or too little.
        if (isAtZero == true) { return; }

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

    public override void RecieveDamage(float amount, float armorPenetration, Character whoJustDamagedMe)
    {
        whoDamagedMe = whoJustDamagedMe;
        float tempArmor = armor - armorPenetration;
        if (tempArmor < 0)
        {
            tempArmor = 0;
        }
        if (tempArmor >= 0)
        {
            amount = amount * ((100 / (100 + tempArmor)));
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
        enemyAi.RecievedDamage(whoDamagedMe);
        Debug.Log(whoDamagedMe.name + " just dealt: " + amount + " damage to " + gameObject.name);
    }

}
