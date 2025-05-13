using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Centaur")]
public class Centaur : Ability
{
	// This passive ability allows the player to shoot while running

	private void Awake()
	{
		// ScritableObject Awake is called when editor is launched rather than runtime
		//  Debug.Log("Added Centaur");
		//  // If we have a gun, reset the rotation
		//  if (GameManager.GM.GetCurrentGun() != null) GameManager.GM.GetCurrentGun().ResetRotation();
	}
}
