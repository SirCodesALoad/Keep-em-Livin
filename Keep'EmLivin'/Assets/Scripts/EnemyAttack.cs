using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    [SerializeField] private int numOfAttacks = 0;
    [SerializeField] private int numOfAttacksToTriggerSpecial = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void Attack()
    {



        numOfAttacks++;

    }
}
