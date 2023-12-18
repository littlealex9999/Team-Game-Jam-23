using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Camera & Movement")]
    public GameObject centralObj;
    //public bool inverseCamera;
    public float cameraSmoothTime = 0.5f;

    public float moveSpeed = 5.0f;
    public float sprintSpeed = 10.0f;
    public Vector2 lookSpeed = new Vector2(5.0f, 5.0f);
    public Vector2 lookLimitVertical = new Vector2(-90.0f, 90.0f);

    private Vector3 cameraVelocity;
    private Vector2 angleLook;
    public bool sprinting { get; private set; }

    [Header("Shooting")]
    public Bullet bulletPrefab;
    public Transform bulletSpawn;
    public float shootCooldown;
    float shootTimer;
    public float reloadDuration = 4.0f;
    float reloadTime;

    [Space]
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        DoLook();
        DoMove();
        DoShoot();
    }

    void DoLook()
    {
        centralObj.transform.position = Vector3.SmoothDamp(centralObj.transform.position, transform.position, ref cameraVelocity, cameraSmoothTime);
        angleLook.x += Input.GetAxis("Mouse X") * lookSpeed.x;
        angleLook.y += -Input.GetAxis("Mouse Y") * lookSpeed.y;

        if (angleLook.y > lookLimitVertical.y) {
            angleLook.y = lookLimitVertical.y;
        } else if (angleLook.y < lookLimitVertical.x) {
            angleLook.y = lookLimitVertical.x;
        }

        centralObj.transform.rotation = Quaternion.identity;
        centralObj.transform.Rotate(centralObj.transform.right, angleLook.y, Space.World);
        centralObj.transform.Rotate(Vector3.up, angleLook.x, Space.World);
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

        //if (inverseCamera) moveInput = -moveInput;

        transform.position += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * moveInput * Time.deltaTime;
    }

    #region Shooting
    void DoShoot()
    {
        if (shootTimer <= 0) {
            if (Input.GetButton("Fire1")) {
                if (currentAmmoClip > 0 && !reloading) {
                    //Quaternion rot = transform.rotation;
                    //if (inverseCamera) rot = Quaternion.Euler(-rot.eulerAngles);

                    for (int i = 0; i < bulletCount; i++) {
                        Quaternion spreadAngle = transform.rotation * Quaternion.Euler(Random.Range(-bulletSpread.x, bulletSpread.x), Random.Range(-bulletSpread.y, bulletSpread.y), 0);
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

    }
    #endregion
}
