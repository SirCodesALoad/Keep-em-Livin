using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.UI;

public class PlayerSpellCasting : MonoBehaviour
{

    [SerializeField] private GameObject spawnablesObject;

    #region UI Variables
    [SerializeField] private Text castingBarSpellName;
    [SerializeField] private Text castingBarCastingTime;
    [SerializeField] private Image castingBar;
    [SerializeField] private Image castingBarSpellIcon;
    [SerializeField] private CanvasGroup castingBarCanvasGroup;
    #endregion


    [SerializeField] private bool isCasting = false;
    [SerializeField] private float mana = 100f;
    [SerializeField] private float totalMana = 100f;
    [SerializeField] private SpellBehiavour[] spellList;
    [SerializeField] private GameObject currentSpellBeingCast;
    public float castCounter = 0f;
    public GameObject selectedAlly;

    ControlActions actions;

    InputDevice device;
    InputControl control;

    private void Start()
    {
        device = InputManager.ActiveDevice;
        control = device.GetControl(InputControlType.Action1);
        actions = new ControlActions();


        actions.leftClick.AddDefaultBinding(Mouse.LeftButton);
        actions.abilityOne.AddDefaultBinding(Key.E);
        actions.abilityTwo.AddDefaultBinding(Key.R);
    }

    private void Update()
    {
        if (actions.leftClick.WasPressed)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Debug.Log("Mouse Pos X: " + mousePos.x.ToString() + " ");
            Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Worldpos.x, Worldpos.y), Vector2.zero);

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
            if (spellList[0] != null)
            {
                CastSpell(spellList[0]);
            }
        }

        if (actions.abilityTwo.WasPressed && isCasting != true)
        {
            //Begin casting.
            if (spellList[1] != null)
            {
                CastSpell(spellList[1]);
            }
        }


    }


    private void CastSpell(SpellBehiavour sp)
    {
        StopCoroutine("FadeCastBarOut");
        if (selectedAlly != null && selectedAlly.GetComponent<Character>() != null)
        {
            if (sp.targettingRadius > 0)
            {
                Collider2D[] hit = Physics2D.OverlapCircleAll(new Vector2(selectedAlly.transform.position.x, selectedAlly.transform.position.y), spellList[0].targettingRadius);
                Character[] tempList = new Character[hit.Length];
                int tempListCounter = 0;
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].gameObject.GetComponent<Character>() != null)
                    {
                        tempList[tempListCounter] = hit[i].gameObject.GetComponent<Character>();
                        tempListCounter++;
                    }
                }

                Character[] targetList = new Character[tempListCounter + 1];
                targetList[0] = selectedAlly.GetComponent<Character>();
                for (int i = 0; i < tempListCounter; i++)
                {
                    if (tempList[i] == null || i > targetList.Length)
                    {
                        break;
                    }
                    targetList[i + 1] = tempList[i];
                }


                currentSpellBeingCast = Instantiate(sp.gameObject, spawnablesObject.transform);
                currentSpellBeingCast.GetComponent<SpellBehiavour>().player = this;
                currentSpellBeingCast.GetComponent<SpellBehiavour>().target = new TargetData(targetList);
            }
            else
            {
                currentSpellBeingCast = Instantiate(sp.gameObject, spawnablesObject.transform);
                currentSpellBeingCast.GetComponent<SpellBehiavour>().player = this;
                Character[] targetList = new Character[1];
                targetList[0] = selectedAlly.GetComponent<Character>();
                currentSpellBeingCast.GetComponent<SpellBehiavour>().target = new TargetData(targetList);
            }


            isCasting = true;

            castingBarCanvasGroup.alpha = 1f;
            castingBar.color = sp.castingBarColour;
            castingBarSpellName.text = sp.name;
            castingBar.fillAmount = 0f;
            castingBarCastingTime.text = "" + sp.castTime;
            castingBarSpellIcon.sprite = sp.icon;
        }

    }


    public void UpdateCastingBarUI(float timeLeftToSpellFinishes, float overallTimer, float castTime)
    {

        float barRate = (1f / castTime) * overallTimer;
        castingBarCastingTime.text = timeLeftToSpellFinishes.ToString("F2");
        castingBar.fillAmount = Mathf.Lerp(0, 1, barRate);
    }


    public void CastingReset()
    {
        isCasting = false;
        currentSpellBeingCast = null;
        StartCoroutine("FadeCastBarOut");

    }

    private IEnumerator FadeCastBarOut()
    {
        float rateOfFade = 1f / 0.5f;

        float progress = 0.0f;

        while (progress <= 1f)
        {
            castingBarCanvasGroup.alpha = Mathf.Lerp(1, 0, progress);

            progress += rateOfFade * Time.deltaTime;

            yield return null;
        }
    }

}
