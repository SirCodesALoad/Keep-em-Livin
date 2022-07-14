using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerTools;

public class EnemyAnimHandler : MonoBehaviour
{

    public SpriteAnim anim;
    public EnemyAI ai;

    [SerializeField] private AnimationClip[] animations; //0:Idle,1: Move,2: Attack, 3: Death. 4 etc, is extra.

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<SpriteAnim>();
        ai = transform.parent.GetComponent<EnemyAI>();

    }

    // Update is called once per frame
    void Update()
    {



    }
}
