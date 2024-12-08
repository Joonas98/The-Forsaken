using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
	public float dashSpeed;
	public float dashDuration;

	public override void Activate(GameObject parent)
	{
		// Debug.Log("Dash activated");
		base.Activate(parent);
		PlayerMovement movementScript = parent.GetComponent<PlayerMovement>();
		movementScript.ApplySpeedEffect(dashSpeed, dashDuration);
	}

	public override void BeginCooldown(GameObject parent)
	{
		// Debug.Log("Dash ended");
		base.BeginCooldown(parent);
	}
}
