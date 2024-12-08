using UnityEngine;
using UnityEngine.UI;

// This class exists to handle runtime functionality from MonoBehaviour
// Ability.cs for example inherits ScritableObject, which is more of a data container
public class AbilityHolder : MonoBehaviour
{
	public Ability ability;
	public Image abilityImage;
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

	private void Start()
	{
		if (ability.abilityType == Ability.AbilityType.Passive)
		{
			state = AbilityState.passive;
			abilityImage.fillAmount = 0;
		}
		else
		{
			state = AbilityState.ready;
		}
	}

	private void Update()
	{
		// Passive does not have cooldown. Active and ActivePassive abilities use CD handling
		if (state == AbilityState.passive) return;

		switch (state)
		{
			case AbilityState.ready:
				abilityImage.fillAmount = 0;
				if (Input.GetKeyDown(key))
				{
					ability.Activate(gameObject);
					state = AbilityState.active;
					activeTime = ability.activeTime;
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
		}
	}
}
