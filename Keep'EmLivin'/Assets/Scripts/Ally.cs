using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PowerTools;

public class Ally : Character
{

    #region UI Variables
    [SerializeField] private Image healthBar;
    [SerializeField] private CanvasGroup healthBarCanvasgroup;
    #endregion

    //Animation and GFX Stuff.
    public SpriteAnim anim;

    [SerializeField] private AnimationClip[] animations; //0:Idle,1: Attack, 2: Death. 3 etc, is extra.

    #region AI operation functions
    [SerializeField] private float attackRanage = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float lastAttack = 0f;
    [SerializeField] private float attackDamage = 12f;
    private enum State
    {
        Attacking, Idle, Dead
    }
    [SerializeField]
    private State state = State.Idle;
    BoxCollider2D col;
    #endregion

    private void Start()
    {

        col = GetComponent<BoxCollider2D>();
        UpdateHealthBarUI();
        anim = transform.GetChild(0).GetComponent<SpriteAnim>();

    }


    private void Update()
    {
        //For ally, update technically handles AI operation while this isn't an amazing architecture to follow, it would be silly to seperate seperate logic for it.
        //Seeing as, currently I want all allies to behiave in this manner. I.e. They don't move.
        //This logic could well get seperated into an AllyAI script but for now it works just fine here.

        switch (state)
        {
            case State.Idle:
                UpdateAnim();

                if(whoDamagedMe != null)
                {
                    if(Vector2.Distance(transform.position, whoDamagedMe.transform.position) <= attackRanage)
                    {
                        state = State.Attacking;
                    }
                }
                break;
            case State.Attacking:
                if (whoDamagedMe != null)
                {
                    if (Vector2.Distance(transform.position, whoDamagedMe.transform.position) <= attackRanage)
                    {
                        if (whoDamagedMe.GetComponent<Enemy>() != null)
                        {
                            if (whoDamagedMe.GetComponent<Enemy>().IsAlive() == false)
                            {
                                TargetClosestEnemy();
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
                    }
                }


                break;
            case State.Dead:
                break;
        }


    }


    public virtual void Attack()
    {
        if (anim.IsPlaying(animations[1]) != true && anim.IsPlaying(animations[2]) != true)
        {
            anim.Play(animations[1]);
        }

    }


    public virtual void Anim_CauseDamageInstance()
    {
        Enemy toDamage = whoDamagedMe.GetComponent<Enemy>();

        if (toDamage != null)
        {
            toDamage.RecieveDamage(attackDamage, 0f, this);
        }

    }

    public override void AtZero()
    {
        base.AtZero();
        Die();
    }

    public virtual void Die()
    {
        state = State.Dead;
        col.isTrigger = true;

        anim.Play(animations[2]);
    }


    private void TargetClosestEnemy()
    {
        Character closestTarget = whoDamagedMe;
        state = State.Idle;

        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, attackRanage);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].GetComponent<Enemy>() != null)
            {
                if (hit[i].GetComponent<Enemy>().IsAlive() == true)
                {
                    if (Vector2.Distance(transform.position, hit[i].transform.position) < Vector2.Distance(transform.position, closestTarget.transform.position))
                    {
                        //If there is something else closer than the player we can attack, set that to be the new target.
                        closestTarget = hit[i].GetComponent<Character>();
                    }
                }
            }
        }
        whoDamagedMe = closestTarget;
        if(whoDamagedMe == null)
        {
            state = State.Idle;
        }

    }

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
            case State.Dead:
                if (anim.IsPlaying(animations[2]) != true)
                {
                    anim.Play(animations[2]);
                }
                break;
            case State.Attacking:
                if (anim.IsPlaying(animations[0]) != true && anim.IsPlaying(animations[1]) != true)
                {
                    //If we're not currently playing the attack animation and we're not playing the idle animation. Play the idle animation.
                    anim.Play(animations[0]);
                }
                break;
        }
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
        UpdateHealthBarUI();
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
        UpdateHealthBarUI();
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
        Debug.Log(whoDamagedMe.name + " just dealt: " + amount + " damage to " + gameObject.name);
        UpdateHealthBarUI();
    }

    public void UpdateHealthBarUI()
    {
        StopCoroutine("FadeHealthBarOut");
        healthBarCanvasgroup.alpha = 1f;
        if (health == healthMax)
        {
            healthBar.fillAmount = 1;
        }
        else
        {
            healthBar.fillAmount = health / healthMax % 1;
        }

        if (health == healthMax || health <= 0)
        {
            StartCoroutine("FadeHealthBarOut");
        }
    }

    private IEnumerator FadeHealthBarOut()
    {
        float rateOfFade = 1f / 0.5f;

        float progress = 0.0f;

        while (progress <= 1f)
        {
            healthBarCanvasgroup.alpha = Mathf.Lerp(1, 0, progress);

            progress += rateOfFade * Time.deltaTime;

            yield return null;
        }
    }
}
