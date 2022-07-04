using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class AllySelectionManager : MonoBehaviour
{
    private AllyPostionSlot[] postionSlots;

    public GameObject selectedSlot;
    public GameObject selectedAlly;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (actions.leftClick.WasPressed)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Debug.Log("Mouse Pos X: " + mousePos.x.ToString() + " ");
            Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2 (Worldpos.x, Worldpos.y), Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Clicked on " + hit.transform.name);
                if (hit.collider.gameObject.GetComponent<HealthManager>() != null)
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

        if (actions.abilityOne.WasPressed)
        {
            //Begin casting.

        }

        
    }
}
