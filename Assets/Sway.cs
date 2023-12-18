using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{

    [Header("Position")]
    public float amount;
    public float maxAmount;
    public float smoothAmount;

    [Header("Rotation")]
    public float rotationAmount = 4;
    public float maxRotationAmount = 5;
    public float smoothRotation = 12;

    [Space]
    public bool rotationX = true;
    public bool rotationY = true;
    public bool rotationZ = true;

    //Internal Privates
    private Vector3 initialPosition;
    private Quaternion initialRotation;


    private float InputX;
    private float InputY;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        CalculateSway();

        MoveSway();
        TiltSway();
    }

    private void CalculateSway()
    {
        InputX = -Input.GetAxis("Mouse X") * amount;
        InputY = -Input.GetAxis("Mouse Y") * amount;
    }


    private void MoveSway()
    {
        float movex = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
        float movey = Mathf.Clamp(InputY * amount, -maxAmount, maxAmount);

        Vector3 finalPosition = new Vector3(movex, movey, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }

    private void TiltSway()
    {
        float tiltY = Mathf.Clamp(InputX * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = Mathf.Clamp(InputY * rotationAmount, -maxRotationAmount, maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(rotationX ? -tiltX : 0f, rotationY ? tiltY : 0f, rotationZ ? tiltY : 0));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, Time.deltaTime * smoothRotation);
    }
}
