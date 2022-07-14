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

    private Seeker seeker;
    private Path path;
    [SerializeField] private int currentWayPoint = 0;
    [SerializeField] private bool reachedEndOfPath = false;
        
    private CircleCollider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    //Animation and GFX Stuff.
    public SpriteAnim anim;

    [SerializeField] private AnimationClip[] animations; //0:Idle,1: Move,2: Attack, 3: Death. 4 etc, is extra.


    void Start()
    {
        anim = transform.GetChild(0).GetComponent<SpriteAnim>();
      

        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
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
        Debug.Log("Picking new target");
        Transform closestTarget = player;
        state = State.Idle;

        Collider2D[] hit = Physics2D.OverlapCircleAll(rb.position, 5f);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].GetComponent<Character>() != null)
            {
                if (hit[i].GetComponent<Character>().isAlly == true && hit[i].GetComponent<Character>().IsAlive() == true)
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
            if(hit[i].GetComponent<Character>() != null)
            {
                if (hit[i].GetComponent<Character>().isAlly == true && hit[i].GetComponent<Character>().IsAlive() == true)
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
                    Debug.Log("Path is null");
                    return;
                }
                UpdateAnim();
                if (currentWayPoint >= path.vectorPath.Count || Vector2.Distance(rb.position, target.position) <= attackRanage)
                {
                    Debug.Log("We've reached the end of our Path!"); 
                    //We've reached the end of our path.
                    reachedEndOfPath = true;
                    if(Vector2.Distance(rb.position, target.position) > attackRanage)
                    {
                        //if we're at the end of our path and our target is still outside our attack range.. Our target has moved and we need to
                        //update our path.
                        Debug.Log("Target is out of range!");
                        CheckIfThereIsACloserTarget();
                    }
                    if (target.GetComponent<Character>() != null)
                    {
                        if (target.GetComponent<Character>().isAlly == true)
                        {
                            if (target.GetComponent<Character>().IsAlive() == true)
                            {
                                state = State.Attacking;
                                break;
                            }
                            //Our target is an enemy of ours but it's not currently alive! Pick a new target.
                            PickNewTarget();
                            break;
                        }
                    }
                    else
                    {
                        state = State.Idle;
                    }
                }
                else
                {
                    reachedEndOfPath = false;
                }

                //Actually apply movement.
                Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;

                Vector2 force = direction * moveSpeed * Time.deltaTime;
                rb.velocity = force;

                float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);
                if(distance < nextWayPointDistance)
                {
                    
                    currentWayPoint++;
                }
                break;
            case State.Attacking:
                UpdateAnim();
                if (Vector2.Distance(rb.position, target.position) > attackRanage)
                {
                    //If for some reason we're out of range. Switch to moving to the target again.
                    //But first check if there is a closer target.
                    Debug.Log("Target is out of range!");
                    CheckIfThereIsACloserTarget();
                    state = State.MovingToTarget;
                    return;
                }
                if (target.GetComponent<Character>() != null)
                {
                    if (target.GetComponent<Character>().IsAlive() == false)
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
                }

                break;
            case State.Idle:
                //if(target != null && Vector2.Distance(rb.position,target.position) > nextWayPointDistance)
                //{
                //    seeker.StartPath(rb.position, target.position, OnPathComplete);

                //}
                UpdateAnim();
                break;

        }
    }

    #region Animation Functions

    private void UpdateAnim()
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
        }

    }

    #endregion
}
