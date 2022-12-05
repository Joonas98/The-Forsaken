using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHolder : MonoBehaviour
{
    // Ability systeemi https://www.youtube.com/watch?v=ry4I6QyPw4E

    public Ability ability;
    public Image abilityImage, backgroundImage;
    float cooldownTime;
    float activeTime;

    enum AbilityState
    {
        ready,
        active,
        cooldown,
        passive
    }

    AbilityState state;
    public KeyCode key;
    private bool passiveInitiated = false;

    private void Start()
    {
        if (ability.passive == true)
        {
            state = AbilityState.passive;
        }
        else
        {
            state = AbilityState.ready;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case AbilityState.ready:
                abilityImage.fillAmount = 0;
                if (Input.GetKeyDown(key))
                {
                    ability.Activate(gameObject);
                    state = AbilityState.active;
                    activeTime = ability.activeTime;
                    backgroundImage.color = Color.yellow;
                }
                break;
            case AbilityState.active:
                if (activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                }
                else
                {
                    ability.BeginCooldown(gameObject);
                    state = AbilityState.cooldown;
                    cooldownTime = ability.cooldownTime;
                    abilityImage.fillAmount = 1;
                    backgroundImage.color = Color.white;
                }
                break;
            case AbilityState.cooldown:
                if (cooldownTime > 0)
                {
                    cooldownTime -= Time.deltaTime;
                    abilityImage.fillAmount -= 1 / ability.cooldownTime * Time.deltaTime;
                }
                else
                {
                    state = AbilityState.ready;
                }
                break;
            case AbilityState.passive:
                if (!passiveInitiated)
                {
                    Debug.Log("Passive ability added");
                    backgroundImage.color = Color.blue;
                    abilityImage.fillAmount = 0;
                    passiveInitiated = true;
                }
                break;
        }
    }



}
