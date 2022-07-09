using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class AllySelectionManager : MonoBehaviour
{
    private AllyPostionSlot[] postionSlots;

    public GameObject selectedSlot;
    public GameObject selectedAlly;
    public Character player;

    public Spell testSpell;
    public bool isCasting = false;
    public float startedCastingAt = 0f;
    public int tickCounter = 0;
    //public List<delegate> spellQueue = new List<delegate>();
    public float nextActionTime = 0f;
    public float elapsed = 0f;
    UnityMouseProvider mouse;
    ControlActions actions;

    InputDevice device;
    InputControl control;
    // Start is called before the first frame update
    void Start()
    {
        device = InputManager.ActiveDevice;
        control = device.GetControl(InputControlType.Action1);
        actions = new ControlActions();


        actions.leftClick.AddDefaultBinding(Mouse.LeftButton);
        actions.abilityOne.AddDefaultBinding(Key.E);


        postionSlots = new AllyPostionSlot[transform.childCount];

        if (transform.childCount >= 2)
        {
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                postionSlots[i] = transform.GetChild(i).GetComponent<AllyPostionSlot>();
            }
        }

    }

    public AllyPostionSlot GetAllyPostion(int slot)
    {
        if (postionSlots[slot] != null && postionSlots[slot].transform.GetChild(0).GetComponent<AllyPostionSlot>() != null)
        {
            return postionSlots[slot].transform.GetChild(0).GetComponent<AllyPostionSlot>();
        }
        return null;


    }

    public void SwapAllysPostion(Character swapThis, Character withThis)
    {
        //Swaps the Ally's postional slot with Another ally.
        AllyPostionSlot toBeSwapped = swapThis.slotOccupied;
        AllyPostionSlot toBeSwappedWith = swapThis.withThis;




    }

    public AllyPostionSlot FindAllysPostionalSlot (Character ally)
    {
        return ally.slotOccupied;

    }


    // Update is called once per frame
    void Update()
    {

        if (isCasting == true)
        {
            //We need to know what spell we are casting.. Maybe have it store that here.
            elapsed += Time.deltaTime;
            if (elapsed >= 1f && Time.deltaTime < startedCastingAt)
            {
                //We're still casting do the thing we're supposed to do during casting.
                startedCastingAt = startedCastingAt - elapsed;
                tickCounter++;
                elapsed = elapsed % 1f;
                
                Debug.Log("Trigger!");
                testSpell.DuringCasting(new TargetData(selectedAlly.GetComponent<Character>(), player));
            }
            else if (Time.deltaTime > startedCastingAt)
            {
                //we're no longer casting. Do everything we're supposed to do and clean up.
                Debug.Log("There were: " + tickCounter + " ticks!");
                isCasting = false;
                nextActionTime = 0f;
                elapsed = 0f;
                tickCounter = 0;
                startedCastingAt = 0;
                testSpell.FinishCasting(new TargetData(selectedAlly.GetComponent<Character>(), player));
            }


        }

        if (actions.leftClick.WasPressed)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Debug.Log("Mouse Pos X: " + mousePos.x.ToString() + " ");
            Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2 (Worldpos.x, Worldpos.y), Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Clicked on " + hit.transform.name);
                if (hit.collider.gameObject.GetComponent<Character>() != null)
                {
                    selectedAlly = hit.collider.gameObject;
                    Debug.Log("Selected Ally" + selectedAlly.transform.name);
                }
            }
            else
            {
                Debug.Log("Nothing hit");
                selectedAlly = null;
            }

        }


        if (actions.abilityOne.WasPressed && isCasting != true)
        {
            //Begin casting.
            if (testSpell != null)
            {
                if (selectedAlly != null && selectedAlly.GetComponent<Character>() != null)
                {
                    startedCastingAt = testSpell.castTime + Time.deltaTime;
                    nextActionTime = Time.deltaTime;
                    testSpell.StartCasting(new TargetData(selectedAlly.GetComponent<Character>(), player));
                    isCasting = true;
                    tickCounter = 0;
                    
                }
            }
        }

        

        
    }
}
