using System.Collections;
using System.Collections.Generic;
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
    Vector3 cameraVelocity;
    Vector2 angleLook;
    Vector3 velocity;

    public bool sprinting { get; private set; }
    public bool grounded { get { return Physics.Raycast(transform.position, -Vector3.up, characterController.height / 2 + groundedCheckBuffer); } }


    [Header("Shooting")]
    public Bullet bulletPrefab;
    public Transform bulletSpawn;
    public float shootCooldown;
    float shootTimer;
    public float reloadDuration = 4.0f;
    float reloadTime;

    [Space]
    public bool fullAuto = true;
    public int bulletCount = 1;
    public float bulletSpeed = 15.0f;
    public float bulletDamage = 10.0f;
    public Vector2 bulletSpread = new Vector2(1.0f, 1.0f);

    [Space]
    public int maxAmmoClip = 15;
    public int maxAmmoHeld = 150;
    public int currentAmmoClip = 15;
    public int currentAmmoHeld = 150;

    bool reloading { get { return reloadTime > 0; } }

    [Header("Health & Interactions")]
    public float maxHealth;
    public float health;
    public float interactionRange = 5.0f;
    Interactable interactingWith;
    float interactionTimer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        characterController = GetComponent<CharacterController>();

        health = maxHealth;
    }

    void Update()
    {
        DoLook();
        DoMove();
        DoShoot();
    }

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
        sprinting = Input.GetButton("Sprint");

        Vector3 moveInput = new Vector3();
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.z = Input.GetAxis("Vertical");
        if (sprinting) {
            moveInput *= sprintSpeed;
        } else {
            moveInput *= moveSpeed;
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

    #region Shooting
    void DoShoot()
    {
        if (shootTimer <= 0) {
            if (fullAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1")) {
                if (currentAmmoClip > 0 && !reloading) {
                    Quaternion rot = cam.transform.rotation;
                    //if (inverseCamera) rot = Quaternion.Euler(-rot.eulerAngles);

                    for (int i = 0; i < bulletCount; i++) {
                        Quaternion spreadAngle = rot * Quaternion.Euler(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), 0);
                        Bullet b = Instantiate(bulletPrefab, bulletSpawn.position, spreadAngle);
                        b.speed = bulletSpeed;
                        b.damage = bulletDamage;
                    }

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

        reloadTime = duration;
        while ((reloadTime -= Time.deltaTime) > 0) {
            yield return new WaitForEndOfFrame();
        }

        int ammoRestored = maxAmmoClip - currentAmmoClip;
        int possibleRestore = currentAmmoHeld - ammoRestored;
        if (possibleRestore < 0) ammoRestored = currentAmmoHeld;

        currentAmmoClip = ammoRestored;
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
    #endregion

    #region Interactions
    void CheckInteractions()
    {
        if (interactingWith != null) {

        } else {
            // layermask 6 is the "Interactable" layer
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionRange, 6)) {

            }
        }
    }
    #endregion
}
