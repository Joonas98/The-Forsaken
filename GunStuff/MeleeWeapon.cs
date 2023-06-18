using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeleeWeapon : Weapon
{
    public AudioClip[] stabSounds;
    public AudioClip[] swingSounds;
    public AudioClip hitFloorSound;

    [SerializeField] private float AttackCooldown;
    [SerializeField] private int Damage;
    [SerializeField] private string[] attackAnimations;

    [SerializeField] private ParticleSystem bloodFX;
    [SerializeField] private GameObject trailFX;
    [SerializeField] private GameObject trailParticleFX;

    private bool attacking = false;
    private bool canAttack = true;

    private Animator animator;
    private Enemy enemyScript;

    private TextMeshProUGUI magazineText, totalAmmoText;
    private string magString = "Melee";
    private string totalAmmoString = "Unlimited ammo";

    private List<Enemy> attackedEnemies = new List<Enemy>();

    private void Awake()
    {
        magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
        totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();

        GameObject CrosshairCanvas = GameObject.Find("CrossHairCanvas");
    }

    private void OnEnable()
    {
        magazineText.text = magString;
        totalAmmoText.text = totalAmmoString;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && canAttack == true)
        {
            attacking = true;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            attacking = false;
        }

        if (canAttack == true && attacking == true)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        int raIndex = Random.Range(0, swingSounds.Length);
        audioSource.PlayOneShot(swingSounds[raIndex]);

        int raIndexAnim = Random.Range(0, attackAnimations.Length);
        animator.Play(attackAnimations[raIndexAnim]);

        canAttack = false;
        yield return new WaitForSeconds(AttackCooldown);
        attackedEnemies.Clear();
        canAttack = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attacking)
        {
            enemyScript = other.GetComponentInParent<Enemy>();
            if (enemyScript != null)
            {
                if (!attackedEnemies.Contains(enemyScript))
                {
                    // Debug.Log("Applying damage to: " + enemyScript);
                    enemyScript.TakeDamage(Damage);

                    ParticleSystem bloodFXGO = Instantiate(bloodFX, other.transform.position, Quaternion.identity);
                    audioSource.PlayOneShot(stabSounds[0]);
                    Destroy(bloodFXGO.gameObject, 2f);

                    if (!attackedEnemies.Contains(enemyScript))
                        attackedEnemies.Add(enemyScript);
                }
            }
            else
            {
                // audioSource.PlayOneShot(hitFloorSound);
            }
        }
    }

}
