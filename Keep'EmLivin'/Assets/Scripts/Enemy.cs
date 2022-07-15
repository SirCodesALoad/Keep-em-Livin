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

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AtZero()
    {
        base.AtZero();

    }
}
