using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeleeWeapon : Weapon
{
    public AudioClip[] stabSounds;
    public AudioClip[] swingSounds;
    public AudioClip hitFloorSound;

    [SerializeField] private float attackCooldown, secondaryAttackCooldown;
    [SerializeField] private int damage, damageSecondary;
    [SerializeField] private string[] attackAnimations;

    [SerializeField] private ParticleSystem bloodFX;
    [SerializeField] private GameObject trailFX;
    [SerializeField] private GameObject trailParticleFX;

    private bool attacking = false;
    private bool attackingSecondary = false;
    private bool canAttack = true;

    private Animator animator;
    private Enemy enemyScript;

    private TextMeshProUGUI magazineText, totalAmmoText;
    private string magString = "Melee";
    private string totalAmmoString = "Unlimited ammo";

    private List<Enemy> attackedEnemies = new List<Enemy>();

    protected override void Awake()
    {
        base.Awake();
        magazineText = GameObject.Find("MagazineNumbers").GetComponent<TextMeshProUGUI>();
        totalAmmoText = GameObject.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();

        GameObject CrosshairCanvas = GameObject.Find("CrossHairCanvas");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        magazineText.text = magString;
        totalAmmoText.text = totalAmmoString;
    }

    protected override void Update()
    {
        base.Update();

        HandleInputs();

        if (canAttack == true && attacking == true)
        {
            int randomSwingClip = Random.Range(0, swingSounds.Length);
            audioSource.PlayOneShot(swingSounds[randomSwingClip]);
            StartCoroutine(Attack(false));
        }

        if (canAttack == true && attackingSecondary == true)
        {
            int randomSwingClip = Random.Range(0, swingSounds.Length);
            audioSource.PlayOneShot(swingSounds[randomSwingClip]);
            StartCoroutine(Attack(true));
        }
    }

    public void HandleInputs()
    {
        if (Input.GetButtonDown("Fire1") && Time.timeScale > 0 && canAttack == true)
        {
            attacking = true;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            attacking = false;
        }

        if (Input.GetButtonDown("Fire2") && Time.timeScale > 0 && canAttack == true)
        {
            attackingSecondary = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            attackingSecondary = false;
        }
    }

    public override void EquipWeapon()
    {
        base.EquipWeapon();
    }

    public override void UnequipWeapon()
    {
        base.UnequipWeapon();
    }

    IEnumerator Attack(bool secondaryAttack)
    {
        if (!secondaryAttack)
        {
            animator.Play(attackAnimations[0]);
            canAttack = false;
            yield return new WaitForSeconds(attackCooldown);
            attackedEnemies.Clear();
            canAttack = true;
        }
        else
        {
            animator.Play(attackAnimations[1]);
            canAttack = false;
            yield return new WaitForSeconds(secondaryAttackCooldown);
            attackedEnemies.Clear();
            canAttack = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAnimationPlaying(attackAnimations[0]))
        {
            enemyScript = other.GetComponentInParent<Enemy>();
            if (enemyScript == null) return;

            if (!attackedEnemies.Contains(enemyScript))
            {
                enemyScript.TakeDamage(damage);
                ParticleSystem bloodFXGO = Instantiate(bloodFX, other.transform.position, Quaternion.identity);
                audioSource.PlayOneShot(stabSounds[0]);
                Destroy(bloodFXGO.gameObject, 2f);

                if (!attackedEnemies.Contains(enemyScript))
                    attackedEnemies.Add(enemyScript);
            }
        }

        if (IsAnimationPlaying(attackAnimations[1]))
        {
            enemyScript = other.GetComponentInParent<Enemy>();
            if (enemyScript == null) return;

            if (!attackedEnemies.Contains(enemyScript))
            {
                enemyScript.TakeDamage(damageSecondary);
                ParticleSystem bloodFXGO = Instantiate(bloodFX, other.transform.position, Quaternion.identity);
                audioSource.PlayOneShot(stabSounds[1]);
                Destroy(bloodFXGO.gameObject, 2f);

                if (!attackedEnemies.Contains(enemyScript))
                    attackedEnemies.Add(enemyScript);
            }
        }
    }

    private bool IsAnimationPlaying(string animationName)
    {
        // Get the hash of the animation state using its name
        int animationHash = Animator.StringToHash(animationName);

        // Check if the Animator is currently playing the animation state
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == animationHash;
    }

}
