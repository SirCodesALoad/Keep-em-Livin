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

    private void Start()
    {


        healthBar.fillAmount = health / healthMax % 1;

    }


    public void UpdateHealthBarUI()
    {
        StopAllCoroutines();
        if (health == healthMax)
        {
            healthBar.fillAmount = 1;
        }
        else
        {
            healthBar.fillAmount = health / healthMax % 1;
        }

        if (health == healthMax)
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
