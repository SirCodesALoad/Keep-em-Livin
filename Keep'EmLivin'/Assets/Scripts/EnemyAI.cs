using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{

    private enum State
    {
        MovingToTarget,Attacking,Idle
    }
    [SerializeField] private Transform player;
    public Transform target;
    [SerializeField] private State state = State.Idle;

    [SerializeField] private float moveSpeed = 150f;
    [SerializeField] private float attackRanage = 3f;
    [SerializeField]  private float nextWayPointDistance = 2f;

    private Seeker seeker;
    private Path path;
    [SerializeField] private int currentWayPoint = 0;
    [SerializeField] private bool reachedEndOfPath = false;
        
    private CircleCollider2D col;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        PickNewTarget();
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            p = path;
            currentWayPoint = 0;
            if (state != State.Attacking)
            {
                state = State.MovingToTarget;
            }
        }


    }

    private void PickNewTarget()
    {
        Transform closestTarget = player;

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
        state = State.Idle;
        seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void CheckIfThereIsACloserTarget()
    {
        Transform closestTarget = target;

        Collider2D[] hit = Physics2D.OverlapCircleAll(rb.position, attackRanage);

        for (int i = 0; i < hit.Length; i++)
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
        target = closestTarget;
        state = State.Idle;
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
                    return;
                }

                if(currentWayPoint >= path.vectorPath.Count || Vector2.Distance(rb.position, target.position) <= attackRanage)
                {
                    //We've reached the end of our path.
                    reachedEndOfPath = true;
                    if(transform.GetComponent<Character>().isAlly == true)
                    {
                        if(transform.GetComponent<Character>().IsAlive() == true)
                        {
                            state = State.Attacking;
                            break;
                        }
                        //Our target is an enemy of ours but it's not currently alive! Pick a new target.
                        PickNewTarget();
                        break;
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
                rb.AddForce(force);

                float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);
                if(distance < nextWayPointDistance)
                {
                    currentWayPoint++;
                }
                break;
            case State.Attacking:
                if (Vector2.Distance(rb.position, target.position) > attackRanage)
                {
                    //If for some reason we're out of range. Switch to moving to the target again.
                    //But first check if there is a closer target.
                    CheckIfThereIsACloserTarget();
                    state = State.MovingToTarget;
                    break;
                }

                if (transform.GetComponent<Character>().IsAlive() == false)
                {

                    PickNewTarget();
                    break;
                }

                //Attack logic.
                //Likely code this as a virtual attack function.
                break;
            case State.Idle:
                //if(target != null && Vector2.Distance(rb.position,target.position) > nextWayPointDistance)
                //{
                //    seeker.StartPath(rb.position, target.position, OnPathComplete);
                    
                //}
                break;

        }
    }
}
