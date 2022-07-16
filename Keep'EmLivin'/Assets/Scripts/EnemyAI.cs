using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using PowerTools;

public class EnemyAI : MonoBehaviour
{

    private enum State
    {
        MovingToTarget,Attacking,Idle,Dead
    }
    [SerializeField] private Transform player;
    public Transform target;
    [SerializeField] private State state = State.Idle;

    [SerializeField] private float moveSpeed = 150f;
    [SerializeField] private float attackRanage = 3f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float lastAttack = 0f;
    [SerializeField]  private float nextWayPointDistance = 2f;
    [SerializeField] private float attemptToFindCloserTargetTimer = 0f;

    [SerializeField] private int numOfAttacks = 0;
    [SerializeField] private int numOfAttacksToTriggerSpecial = 0;

    private Seeker seeker;
    private Path path;
    [SerializeField] private int currentWayPoint = 0;
    [SerializeField] private bool reachedEndOfPath = false;
        
    private CircleCollider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform damageLocation;
    private Enemy enemy;

    //Animation and GFX Stuff.
    public SpriteAnim anim;

    [SerializeField] private AnimationClip[] animations; //0:Idle,1: Move,2: Attack, 3: Death. 4: SpecialAttack.. 5 etc, is extra.


    void Start()
    {
        anim = transform.GetChild(0).GetComponent<SpriteAnim>();
      

        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        damageLocation = transform.GetChild(1).transform;
        seeker = GetComponent<Seeker>();
        PickNewTarget();

    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
            if (state != State.Attacking)
            {
                state = State.MovingToTarget;
            }
        }
        else
        {
            Debug.Log("There was an Error in requested path: " + p.error);
        }

    }

    private void PickNewTarget()
    {
        //Debug.Log("Picking new target");
        Transform closestTarget = player;
        state = State.Idle;

        Collider2D[] hit = Physics2D.OverlapCircleAll(rb.position, 5f);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].GetComponent<Ally>() != null)
            {
                if (hit[i].GetComponent<Ally>().IsAlive() == true)
                {
                    Debug.Log(hit[i].name);
                    if (Vector2.Distance(rb.position, hit[i].transform.position) < Vector2.Distance(rb.position, closestTarget.position))
                    {
                        //If there is something else closer than the player we can attack, set that to be the new target.
                        closestTarget = hit[i].transform;
                    }
                }
            }
        }
        target = closestTarget;
        path = null;
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void CheckIfThereIsACloserTarget()
    {
        Debug.Log("Check if there is a closer target.");
        Transform closestTarget = target;
        state = State.Idle;

        Collider2D[] hit = Physics2D.OverlapCircleAll(rb.position, attackRanage);

        for (int i = 0; i < hit.Length; i++)
        {
            if(hit[i].GetComponent<Ally>() != null)
            {
                if (hit[i].GetComponent<Ally>().IsAlive() == true)
                {
                    if (Vector2.Distance(rb.position, hit[i].transform.position) < Vector2.Distance(rb.position, closestTarget.position))
                    {
                        //If there is something else closer than the player we can attack, set that to be the new target.
                        closestTarget = hit[i].transform;
                    }
                }
            }
        }
        target = closestTarget;
        path = null;
        seeker.StartPath(rb.position, target.position, OnPathComplete);

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case State.MovingToTarget:
                if(path == null)
                {
                    Debug.Log("Path is null or we're still moving when we've reached the end of path.");
                    return;
                }
                UpdateAnim();
                if (currentWayPoint >= path.vectorPath.Count || Vector2.Distance(rb.position, target.position) <= attackRanage)
                {
                    //We've reached the end of our path.
                    reachedEndOfPath = true;
                    if (Vector2.Distance(rb.position, target.position) > attackRanage)
                    {
                        //if we're at the end of our path and our target is still outside our attack range.. Our target has moved and we need to
                        //update our path.

                        CheckIfThereIsACloserTarget();
                        return;
                    }
                    else {
                        if(target == player)
                        {
                            state = State.Attacking;
                            return;
                        }
                        if (target.GetComponent<Ally>() != null)
                        {
                            if (target.GetComponent<Ally>().IsAlive() == true)
                            {
                                state = State.Attacking;
                                return;
                            }
                            //Our target is an enemy of ours but it's not currently alive! Pick a new target.
                            PickNewTarget();
                            return;

                        }
                        else
                        {
                            state = State.Idle;
                            return;
                        }
                    }
                    
                }
                else
                {
                    reachedEndOfPath = false;
                }


                Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;

                Vector2 force = direction * moveSpeed * Time.deltaTime;


                if (reachedEndOfPath != true)
                {
                    rb.velocity = force;
                    float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);
                    if (distance < nextWayPointDistance)
                    {

                        currentWayPoint++;
                    }

                    attemptToFindCloserTargetTimer += Time.deltaTime;
                    if(attemptToFindCloserTargetTimer > 3f)
                    {
                        attemptToFindCloserTargetTimer = 0f;
                        CheckIfThereIsACloserTarget();
                        return;
                    }

                }
                else
                {
                    //If we've somehow gotten all the way down here, set speed to 0 and move to idle because w're not moving!
                    rb.velocity = Vector2.zero;
                    state = State.Idle;
                }
                
                break;
            case State.Attacking:
                UpdateAnim();
                if (Vector2.Distance(rb.position, target.position) > attackRanage)
                {
                    //If for some reason we're out of range. Switch to moving to the target again.
                    //But first check if there is a closer target.
                    //Debug.Log("Target is out of range!");
                    CheckIfThereIsACloserTarget();
                    state = State.MovingToTarget;
                    return;
                }
                if (target.GetComponent<Ally>() != null)
                {
                    if (target.GetComponent<Ally>().IsAlive() == false)
                    {
                        Debug.Log("Target is not alive!");

                        PickNewTarget();
                        return;
                    }
                }
                //Attack logic
                if (Time.time > lastAttack)
                {
                    lastAttack = Time.time + attackCooldown;
                    //Play our attack animation.
                    Attack();
                }

                return;
            case State.Idle:
                //if(target != null && Vector2.Distance(rb.position,target.position) > nextWayPointDistance)
                //{
                //    seeker.StartPath(rb.position, target.position, OnPathComplete);

                //}
                UpdateAnim();
                break;
            case State.Dead:
                break;

        }
    }

    public virtual void RecievedDamage(Character thingThatHitMe)
    {
        //called when the unit takes damage and compares the distance between the thing that hit it and it's current target.
        if(target == null || target == thingThatHitMe.transform)
        {
            target = thingThatHitMe.transform;
            return;
        }

        if (Vector2.Distance(rb.position, thingThatHitMe.transform.position) < Vector2.Distance(rb.position, target.position))
        {
            //If there is something else closer than our current target we can attack, set that to be the new target.
            target = thingThatHitMe.transform;
            return;
        }


    }

    public virtual void Attack()
    {

        if(numOfAttacks > numOfAttacksToTriggerSpecial && animations.Length > 3)
        {
            anim.Play(animations[4]);
        }
        else
        {
            anim.Play(animations[2]);
        }

    }

    public virtual void Anim_CauseDamageInstance()
    {
        //Triggered by the attack animation to cause an instance of damage.
        //For an imp, we don't actually care the expact postioning of the imp to ally.
        //As it's a single target strike and as a condition to tirgger the animation we need to be in attack ranage
        //but we can code whatever we want here including spawning in a colldir.
        //Or simply, making an attack that piereces multiple lines of eneimes. Or targets the entire column.
        if(target == player)
        {
            //game over.
        }

        Ally toDamage = target.GetComponent<Ally>();

        if (toDamage != null)
        {
            //Debug.Log("DamageCall!");
            toDamage.RecieveDamage(enemy.primaryDamage,0f,enemy);
        }



    }

    public virtual void Die()
    {
        state = State.Dead;
        anim.Play(animations[3]);
        col.enabled = false;
    }

    #region Animation Functions

    public virtual void UpdateAnim()
    {
        switch (state)
        {
            case State.Idle:
                if (anim.IsPlaying(animations[0]) != true)
                {
                    anim.Play(animations[0]);
                }
                break;
            case State.MovingToTarget:
                if (anim.IsPlaying(animations[1]) != true)
                {
                    anim.Play(animations[1]);
                }
                break;
            case State.Attacking:
                if (anim.IsPlaying(animations[0]) != true  && anim.IsPlaying(animations[2]) != true)
                {
                    //If we're not currently playing the attack animation and we're not playing the idle animation. Play the idle animation.
                    anim.Play(animations[0]);
                }
                break;
            case State.Dead:
                if (anim.IsPlaying(animations[3]) != true)
                {
                    //If we're not currently playing the attack animation and we're not playing the idle animation. Play the idle animation.
                    anim.Play(animations[3]);
                }
                break;
        }
    }

    public virtual void Anim_Destroy()
    {
        Destroy(this.gameObject);
    }

    #endregion
}
