using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeleeWeapon : Weapon
{
    [Header("Melee Weapon Settings")]
    public AnimationClip[] attackAnimations;

    [SerializeField] private float attackDuration;
    [SerializeField] private float secondaryAttackDuration;
    [SerializeField] private int damage, damageSecondary;
    [SerializeField] private ParticleSystem bloodFX;
    private bool attacking = false;
    private bool attackingSecondary = false;
    private bool canAttack = true;
    private bool mirroredNext = false; // Alternate with normal and mirrored slash
    private string magString = "Melee";
    private string totalAmmoString = "Unlimited ammo";
    private Animator animator;
    private Enemy enemyScript;
    private TextMeshProUGUI magazineText, totalAmmoText;
    private List<Enemy> attackedEnemies = new List<Enemy>();

    [Header("Audio")]
    public AudioClip[] stabSounds;
    public AudioClip[] swingSounds;
    public AudioClip hitFloorSound;

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
        EquipWeapon();
    }

    protected override void Update()
    {
        base.Update();
        HandleInputs();

        if (canAttack == true && attacking == true && equipped)
        {
            int randomSwingClip = Random.Range(0, swingSounds.Length);
            audioSource.PlayOneShot(swingSounds[randomSwingClip]);
            StartCoroutine(Attack(false));
        }

        if (canAttack == true && attackingSecondary == true && equipped)
        {
            int randomSwingClip = Random.Range(0, swingSounds.Length);
            audioSource.PlayOneShot(swingSounds[randomSwingClip]);
            StartCoroutine(Attack(true));
        }

        if (equipped && !unequipping && !attacking && !attackingSecondary)
        {
            transform.position = Vector3.Lerp(transform.position, weaponSpot.transform.position, 1f * Time.deltaTime);
            // transform.rotation = Quaternion.Lerp(transform.rotation, weaponSpot.transform.rotation, 5f * Time.deltaTime);
        }

    }

    public void HandleInputs()
    {
        if (GrenadeThrow.instance.selectingGrenade) return;

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
        animator.SetFloat("StabSpeedMultiplier", attackAnimations[0].length / attackDuration);
        animator.SetFloat("SlashSpeedMultiplier", attackAnimations[1].length / secondaryAttackDuration);
    }

    public override void UnequipWeapon()
    {
        base.UnequipWeapon();
    }

    IEnumerator Attack(bool secondaryAttack)
    {
        if (!secondaryAttack)
        {
            animator.Play(attackAnimations[0].name);
            canAttack = false;
            yield return new WaitForSeconds(attackDuration);
            attackedEnemies.Clear();
            canAttack = true;
        }
        else
        {
            if (!mirroredNext)
            {
                animator.Play(attackAnimations[1].name);
                mirroredNext = true;
            }
            else
            {
                animator.Play(attackAnimations[2].name);
                mirroredNext = false;
            }

            canAttack = false;
            yield return new WaitForSeconds(secondaryAttackDuration);
            attackedEnemies.Clear();
            canAttack = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Stab
        if (IsAnimationPlaying(attackAnimations[0].name))
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

        // Slashes
        if (IsAnimationPlaying(attackAnimations[1].name))
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

        if (IsAnimationPlaying(attackAnimations[2].name))
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
