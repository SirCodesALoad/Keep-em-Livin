using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    //Effects could link to a effecthandler which handles how they display on the GUI. Likely something that character can handle however.
    #region Visuals
    public Sprite icon;
    public Color color;
    [SerializeField]private GameObject vfx;
    #endregion
    private TargetData afflicting; 
    //whoever we're afflicting. Using TargetData for the possibility of a single buff or debuff affecting multiple targets. Yes I'm aware the logic can already
    //Handle this by adding a copy of the same status effect as a child to the object it's afflicting. But say if we wanted two status to talk to eachter we can now do that.
    //Mostly we're just going be to using afflicting.GetCharacterAt(0);
    [SerializeField]private float duration = 0f;
    [SerializeField] private float tickRate = 1f; //How long is a Tick?
    private float expiryTime = 0f;
    private float currentTickTimer = 0f;
    private float overallTimer = 0f;


    public virtual void Start()
    {
        expiryTime = duration;
        StartEffect();
    }

    public virtual void Update()
    {
        //Override this to be blank should we have a effect that doesn't tick!
        currentTickTimer += Time.deltaTime;
        overallTimer += Time.deltaTime;
        if (currentTickTimer >= tickRate && Time.deltaTime < expiryTime)
        {
            //We're still casting do the thing we're supposed to do during casting.
            expiryTime = expiryTime - currentTickTimer;
            currentTickTimer = currentTickTimer % 1f;

            HandleEffect();
        }
        else if (Time.deltaTime > expiryTime)
        {
            //Our effect has run out. Do everything we're supposed to do and clean up.
            FinishedEffect();
        }
    }

    public virtual void StartEffect()
    {
        //Apply any changes that need to be applied postive or negative.
    }

    public virtual void HandleEffect()
    {
        //If this effect afflicts it's target over a duration it's logic or otherwise
        //is handled here.


    }

    public virtual void FinishedEffect()
    {
        //We do anything we'd like to do on the way out.
        RemoveEffect();
    }

    public virtual void RemoveEffect()
    {
        //We clean the object up of our effects. Remove any penalities or bonuses.
        //Note, the reason this is seperate from Finished effect is because if we
        //have our effect removed by a spell before it can fully take affect we don't
        //want to do the final effects from Finished effect.
        Destroy(this.gameObject);
    }
}
