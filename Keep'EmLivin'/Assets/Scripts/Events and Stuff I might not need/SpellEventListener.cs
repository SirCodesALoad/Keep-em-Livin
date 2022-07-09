// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

public class SpellEventListener : MonoBehaviour
{
    [Tooltip("Event to register with.")]
    public SpellEvent Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public UnityEvent<Spell> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public virtual void OnEventRaised(Spell spell)
    {
        Response.Invoke(spell);
    }
}