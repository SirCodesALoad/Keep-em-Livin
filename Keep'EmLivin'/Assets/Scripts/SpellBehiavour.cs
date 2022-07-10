using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBehiavour : MonoBehaviour
{
    /// <summary>
    /// The SpellBehiavour class dictates how each spell works, Every spell inherits from Spell behiavour and has it's own Spell Gameobject
    /// which is instiated at runtime when the spell is cast.
    /// </summary>

    public float cooldown = 0;
    public float castTime = 0; //If a spell has no casttime it goes off instantly.
    public float manacost = 0;
    public float targettingRadius = 0; //If a spell is single target set this to 0.
    public Animation[] animations;
    public PlayerSpellCasting player;
    public Sprite icon;
    public Color castingBarColour;
    public TargetData target;
    public enum TargetingPreference { Ally,Enemy,Both } public TargetingPreference targetPrefs = TargetingPreference.Ally;
    private float timeLeftToSpellFinishes = 0f;
    private float currentTickTimer = 0f;
    private float overallTimer = 0f;
    //Note, class could contain a gameobject with its own prefab in it.

    public virtual void Start()
    {
        timeLeftToSpellFinishes = castTime;
         StartCast();
    }

    public virtual void Update()
    {
        currentTickTimer += Time.deltaTime;
        overallTimer += Time.deltaTime;
        player.UpdateCastingBarUI(castTime-overallTimer, overallTimer, castTime);
        if (currentTickTimer >= 1f && Time.deltaTime < timeLeftToSpellFinishes)
        {
            //We're still casting do the thing we're supposed to do during casting.
            timeLeftToSpellFinishes = timeLeftToSpellFinishes - currentTickTimer;
            currentTickTimer = currentTickTimer % 1f;
            
            DuringCast();
        }
        else if (Time.deltaTime > timeLeftToSpellFinishes)
        {
            //we're no longer casting. Do everything we're supposed to do and clean up.
            FinishCast();
        }
    }

    public virtual void StartCast()
    {

    }

    public virtual void DuringCast()
    {
        
    }

    public virtual void InteruptCast()
    {
        player.CastingReset();
        Destroy(this.gameObject);
    }

    public virtual void FinishCast()
    {
        player.CastingReset();
        Destroy(this.gameObject);
    }

    public virtual void CancelCast()
    {
        player.CastingReset();
        Destroy(this.gameObject);
    }
}
