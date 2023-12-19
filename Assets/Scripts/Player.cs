using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Camera & Movement")]
    public GameObject cam;
    //public bool inverseCamera;
    //public float cameraSmoothTime = 0.5f;

    public float moveSpeed = 5.0f;
    public float sprintSpeed = 10.0f;
    public Vector2 lookSpeed = new Vector2(5.0f, 5.0f);
    public Vector2 lookLimitVertical = new Vector2(-90.0f, 90.0f);

    [Space]
    public float groundedCheckBuffer = 0.05f;
    public float jumpHeight = 3;
    public float gravity = -10;

    CharacterController characterController;
    Animator anim;
    Vector3 cameraVelocity;
    Vector2 angleLook;
    Vector3 velocity;

    int indoorsTriggersEntered = 0;

    public bool sprinting { get; private set; }
    public bool grounded { get; private set; }
    public bool isIndoors { get { return indoorsTriggersEntered > 0; } }


    [Header("Shooting")]
    public Bullet bulletPrefab;
    public Transform bulletSpawn;
    public float shootCooldown;
    float shootTimer;
    public float reloadDuration = 4.0f;
    float reloadTime;
    bool aimDownSights = false;

    [Space]
    public bool fullAuto = true;
    public int bulletCount = 1;
    public float bulletSpeed = 15.0f;
    public float bulletDamage = 10.0f;
    public Vector2 bulletSpread = new Vector2(1.0f, 1.0f);
    public Vector2 adsBulletSpread = new Vector2(0.0f, 0.0f);

    [Space]
    public int maxAmmoClip = 15;
    public int maxAmmoHeld = 150;
    public int currentAmmoClip = 15;
    public int currentAmmoHeld = 150;

    bool reloading { get { return reloadTime > 0; } }

    [Header("Health & Interactions")]
    public float maxHealth;
    public float health;
    [Space]
    public float maxStamina = 100.0f;
    public float stamina = 100.0f;
    public float staminaRegen = 20.0f;
    public float staminaRegenCooldown = 2.0f;
    public float sprintStaminaCost = 5.0f;
    float staminaRegenTimer;
    [Space]
    public float interactionRange = 5.0f;
    Interactable interactingWith;
    float interactionTimer;
    public int score;

    Vector3 spawnPos;
    Quaternion spawnRot;

    #region Unity
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        health = maxHealth;
        stamina = maxStamina;

        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    void Update()
    {
        if (GameManager.instance.gameStopped) return;

        DoLook();
        DoMove();
        DoShoot();
        CheckInteractions();
        RegenStamina();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Indoors") {
            indoorsTriggersEntered++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Indoors") {
            indoorsTriggersEntered--;
        }
    }

    public void Restart()
    {
        health = maxHealth;
        stamina = maxStamina;
        currentAmmoClip = maxAmmoClip;
        currentAmmoHeld = maxAmmoHeld;

        characterController.enabled = false;
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        characterController.enabled = true;
    }
    #endregion

    #region Movement
    void DoLook()
    {
        //cam.transform.position = Vector3.SmoothDamp(cam.transform.position, transform.position, ref cameraVelocity, cameraSmoothTime);
        angleLook.x += Input.GetAxis("Mouse X") * lookSpeed.x;
        angleLook.y += -Input.GetAxis("Mouse Y") * lookSpeed.y;

        if (angleLook.y > lookLimitVertical.y) {
            angleLook.y = lookLimitVertical.y;
        } else if (angleLook.y < lookLimitVertical.x) {
            angleLook.y = lookLimitVertical.x;
        }

        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.up, angleLook.x, Space.World);

        cam.transform.localRotation = Quaternion.identity;
        cam.transform.Rotate(cam.transform.right, angleLook.y, Space.World);
        //cam.transform.Rotate(Vector3.up, angleLook.x, Space.World);
    }

    void DoMove()
    {
        grounded = false;
        Collider[] nearGroundColliders = Physics.OverlapSphere(transform.position - transform.up * (characterController.height / 2 + groundedCheckBuffer), characterController.radius);
        for (int i = 0; i < nearGroundColliders.Length; i++) {
            if (nearGroundColliders[i].gameObject.layer == LayerMask.NameToLayer("Gun")) continue;
            if (nearGroundColliders[i] != characterController && !nearGroundColliders[i].isTrigger) {
                grounded = true;
            }
        }

        if (stamina < sprintStaminaCost * Time.deltaTime || aimDownSights) {
            sprinting = false;
        } else {
            sprinting = Input.GetButton("Sprint");
        }

        Vector3 moveInput = new Vector3();
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.z = Input.GetAxis("Vertical");
        moveInput = moveInput.normalized;
        if (sprinting) {
            moveInput *= sprintSpeed;
            UseStamina(sprintStaminaCost * Time.deltaTime);
            anim.SetBool("isSprinting", true);
        } else {
            moveInput *= moveSpeed;
            anim.SetBool("isSprinting", false);
        }

        moveInput = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * moveInput;

        if (grounded) {
            if (velocity.y < 0) velocity.y = 0;
            if (Input.GetButtonDown("Jump")) {
                velocity.y = Mathf.Sqrt(2 * -gravity * jumpHeight);
            }
        } else {
            velocity.y += gravity * Time.deltaTime;
        }

        moveInput += velocity;

        characterController.Move(moveInput * Time.deltaTime);
    }
    #endregion

    #region Shooting
    void DoShoot()
    {
        if (Input.GetButton("Fire2")) {
            if (!aimDownSights) {
                aimDownSights = true;
                anim.SetBool("aimDownSights", true);
            }
        } else {
            if (aimDownSights) {
                aimDownSights = false;
                anim.SetBool("aimDownSights", false);
            }
        }

        if (shootTimer <= 0) {
            if (fullAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1")) {
                if (currentAmmoClip > 0 && !reloading) {
                    Vector3 target;
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit)) {
                        target = hit.point;
                        if (Vector3.Dot(target - bulletSpawn.position, bulletSpawn.forward) < 0) {
                            target = bulletSpawn.position + bulletSpawn.forward;
                        }
                    } else {
                        target = bulletSpawn.position + bulletSpawn.forward;
                    }

                    //Quaternion rot = cam.transform.rotation;
                    Quaternion rot = Quaternion.LookRotation(target - bulletSpawn.position, Vector3.up);

                    for (int i = 0; i < bulletCount; i++) {
                        Quaternion spreadAngle;
                        if (aimDownSights) {
                            spreadAngle = rot * Quaternion.Euler(Random.Range(-adsBulletSpread.x, adsBulletSpread.x), Random.Range(-adsBulletSpread.y, adsBulletSpread.y), 0);
                        } else {
                            spreadAngle = rot * Quaternion.Euler(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), 0);
                        }
                        Bullet b = Instantiate(bulletPrefab, bulletSpawn.position, spreadAngle);
                        b.speed = bulletSpeed;
                        b.damage = bulletDamage;
                    }
                    anim.SetTrigger("shooting");
                    currentAmmoClip--;

                    shootTimer = shootCooldown;
                } else {
                    if (currentAmmoHeld > 0) {
                        StartCoroutine(Reload(reloadDuration));
                    }
                }
            }
        } else {
            shootTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Reload")) {
            if (currentAmmoHeld > 0 && currentAmmoClip < maxAmmoClip) {
                StartCoroutine(Reload(reloadDuration));
            }
        }
    }

    public void RestoreAmmo(int amount)
    {
        if (amount <= 0) return;

        currentAmmoHeld += amount;
        if (currentAmmoHeld > maxAmmoClip - currentAmmoClip + maxAmmoHeld) {
            currentAmmoHeld = maxAmmoClip - currentAmmoClip + maxAmmoHeld;
        }
    }

    IEnumerator Reload(float duration)
    {
        if (reloadTime > 0) yield break;
        anim.SetTrigger("Reload");
        reloadTime = duration;
        while ((reloadTime -= Time.deltaTime) > 0) {
            yield return new WaitForEndOfFrame();
        }


        int ammoRestored = maxAmmoClip - currentAmmoClip;
        int possibleRestore = currentAmmoHeld - ammoRestored;
        if (possibleRestore < 0) ammoRestored = currentAmmoHeld;

        currentAmmoClip = currentAmmoClip + ammoRestored;
        currentAmmoHeld -= ammoRestored;

        yield break;
    }
    #endregion

    #region Health
    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        health -= damage;
        if (health <= 0) {
            GameManager.instance.GameOver();
        }
    }

    public void HealDamage(float amount)
    {
        if (amount <= 0) return;

        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }
    }

    public void UseStamina(float amount)
    {
        stamina -= amount;
        staminaRegenTimer = staminaRegenCooldown;
    }

    void RegenStamina() 
    {
        staminaRegenTimer -= Time.deltaTime;
        if (staminaRegenTimer <= 0) {
            stamina += staminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }
    }
    #endregion

    #region Interactions
    void CheckInteractions()
    {
        if (interactingWith != null) {
            if (Input.GetButton("Interact")) {
                if (interactionTimer > 0) {
                    interactionTimer -= Time.deltaTime;
                } else {
                    interactingWith.Interact(this);
                    if (interactingWith.CanInteract(this)) {
                        interactionTimer = interactingWith.timeToInteract;
                    } else {
                        interactingWith = null;
                    }
                }
            } else {
                interactingWith = null;
            }
        } else {
            // layermask 6 is the "Interactable" layer, layermask 7 is the "Boards" layer
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionRange, 1 << 6 | 1 << 7, QueryTriggerInteraction.Collide)) {
                if (Input.GetButtonDown("Interact")) {
                    interactingWith = hit.transform.GetComponent<Interactable>();
                    if (interactingWith != null && interactingWith.CanInteract(this)) {
                        interactionTimer = interactingWith.timeToInteract;
                    } else {
                        interactingWith = null;
                    }
                }
            }
        }
    }
    #endregion
}
