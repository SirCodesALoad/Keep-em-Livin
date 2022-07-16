using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.UI;
using PowerTools;

public class PlayerSpellCasting : MonoBehaviour
{

    [SerializeField] private GameObject spawnablesObject;

    #region UI Variables
    [SerializeField] private Text castingBarSpellName;
    [SerializeField] private Text castingBarCastingTime;
    [SerializeField] private Image castingBar;
    [SerializeField] private Image castingBarSpellIcon;
    [SerializeField] private CanvasGroup castingBarCanvasGroup;

    [SerializeField] private Image manaBar;
    #endregion


    [SerializeField] private bool isCasting = false;
    [SerializeField] private float mana = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 2f;
    [SerializeField] private float currentTickTimer = 0f;

    #region Animation Variables
    [SerializeField] private SpriteAnim anim;
    [SerializeField] private AnimationClip[] animations;
    #endregion

    [SerializeField] private SpellBehiavour[] spellList;
    [SerializeField] private GameObject currentSpellBeingCast;
    public float castCounter = 0f;
    public GameObject selection;
    public GameObject altSelection;
    public SlotManager sM;

    ControlActions actions;

    InputDevice device;
    InputControl control;

    private void Start()
    {
        anim = transform.GetChild(0).GetComponent<SpriteAnim>();

        device = InputManager.ActiveDevice;
        control = device.GetControl(InputControlType.Action1);
        actions = new ControlActions();
        selection = null;
        altSelection = null;
        if (mana == maxMana)
        {
            manaBar.fillAmount = 1;
        }
        else
        {
            manaBar.fillAmount = mana / maxMana % 1;
        }

        actions.leftClick.AddDefaultBinding(Mouse.LeftButton);
        actions.abilityOne.AddDefaultBinding(Key.E);
        actions.abilityTwo.AddDefaultBinding(Key.R);
        actions.swapAllyPostion.AddDefaultBinding(Key.H);
    }

    private void Update()
    {
        if (mana < maxMana)
        {
            currentTickTimer += Time.deltaTime;
            if (currentTickTimer >= 0.5f)
            {
                //regen some Mana.
                currentTickTimer = 0f;
                mana += manaRegenRate;
                if(mana > maxMana)
                {
                    mana = maxMana;
                }
                UpdateManaBar();
            }
        }

        if (actions.leftClick.WasPressed)
        {

            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //Debug.Log("Mouse Pos X: " + mousePos.x.ToString() + " ");
            Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Worldpos.x, Worldpos.y), Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Clicked on " + hit.transform.name);
                selection = hit.transform.gameObject;
            }
            else
            {
                Debug.Log("Nothing hit");
                selection = null;
            }
            if (sM.slotsVisible == true)
            {
                //slide this into a different function.
                if(altSelection != null && selection != null)
                {
                    if (altSelection.GetComponent<Character>() != null)
                    {
                        Debug.Log("Swapping!");
                        if (altSelection.GetComponent<Slottable>() != null && selection.GetComponent<Slottable>() != null)
                        {
                            sM.SwapSlottables(selection.GetComponent<Slottable>(), altSelection.GetComponent<Slottable>());
                        }
                        else
                        {
                            if (selection.GetComponent<Slot>() != null)
                            {
                                Slot slotSelection = selection.GetComponent<Slot>();
                                if (slotSelection.occupier != null)
                                {
                                    sM.SwapSlottables(slotSelection.occupier, altSelection.GetComponent<Slottable>());
                                }
                                else
                                {
                                    altSelection.GetComponent<Slottable>().ChangeSlot(slotSelection.postion, slotSelection);
                                }
                            }
                        }
                    }
                    selection = null;
                    altSelection = null;
                }
                //Even if we selected a slot this frame we can hide the slots interface.
                sM.HideSlots();
            }

        }

        if (actions.abilityOne.WasPressed && isCasting != true)
        {
            if (sM.slotsVisible == true)
            {
                sM.HideSlots();
            }
            //Begin casting.
            if (spellList[0] != null)
            {
                CastSpell(spellList[0]);
            }
        }

        if (actions.abilityTwo.WasPressed && isCasting != true)
        {
            if (sM.slotsVisible == true)
            {
                sM.HideSlots();
            }
            //Begin casting.
            if (spellList[1] != null)
            {
                CastSpell(spellList[1]);
            }
        }

        if (actions.swapAllyPostion.WasPressed && sM != null)
        {

            if(sM.slotsVisible != true)
            {
                sM.ShowSlots();
            }

            if (selection != null && altSelection == null && selection.GetComponent<Slottable>() != null)
            {
                Debug.Log("Now select the target to swap with.");
                altSelection = selection;
                selection = null;

            }
            else if (selection != null && altSelection != null && selection != altSelection)
            {
                Debug.Log("Swapping!");
                if (altSelection.GetComponent<Slottable>() != null && selection.GetComponent<Slottable>() != null)
                {
                    sM.SwapSlottables(selection.GetComponent<Slottable>(), altSelection.GetComponent<Slottable>());
                }
                else
                {
                    if(selection.GetComponent<Slot>() != null)
                    {
                        Slot slotSelection = selection.GetComponent<Slot>();
                        if (slotSelection.occupier != null)
                        {
                            sM.SwapSlottables(slotSelection.occupier, altSelection.GetComponent<Slottable>());
                        }
                        else
                        {
                            altSelection.GetComponent<Slottable>().ChangeSlot(slotSelection.postion, slotSelection);
                        }
                    }
                }
                selection = null;
                altSelection = null;
                sM.HideSlots();
            }
            else if (selection == null && altSelection != null)
            {
                altSelection = null;
                Debug.Log("Canceled Swap");
            }

        }


    }


    private void CastSpell(SpellBehiavour sp)
    {
        StopCoroutine("FadeCastBarOut");
        if (selection != null && selection.GetComponent<Character>() != null && mana > sp.manacost)
        {
            if (sp.targettingRadius > 0)
            {
                Collider2D[] hit = Physics2D.OverlapCircleAll(new Vector2(selection.transform.position.x, selection.transform.position.y), spellList[0].targettingRadius);
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
                targetList[0] = selection.GetComponent<Character>();
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
                targetList[0] = selection.GetComponent<Character>();
                currentSpellBeingCast.GetComponent<SpellBehiavour>().target = new TargetData(targetList);
            }


            isCasting = true;
            mana -= sp.manacost;
            UpdateManaBar();

            castingBarCanvasGroup.alpha = 1f;
            castingBar.color = sp.castingBarColour;
            castingBarSpellName.text = sp.name;
            castingBar.fillAmount = 0f;
            castingBarCastingTime.text = "" + sp.castTime;
            castingBarSpellIcon.sprite = sp.icon;
            UpdateAnim();
        }
        else
        {
            //Code for selecting a target here.
        }

    }

    public void UpdateAnim()
    {
        if(isCasting == true && anim.IsPlaying(animations[1]) != true && anim.IsPlaying(animations[2]) != true)
        {
            anim.Play(animations[1]);
            return;
        }

        anim.Play(animations[0]);

    }

    public void Anim_FinshedWindup()
    {
        anim.Play(animations[2]);
    }

    public void UpdateManaBar()
    {
        if (mana == maxMana)
        {
            manaBar.fillAmount = 1;
        }
        else
        {
            manaBar.fillAmount = mana / maxMana % 1;
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
        UpdateAnim();
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
