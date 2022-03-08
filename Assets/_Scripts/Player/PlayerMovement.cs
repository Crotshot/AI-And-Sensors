using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float lookSpeedX, lookSpeedY, moveSpeed, cameraLimitUp, cameraLimitDown, sidewaysMoveSpeedModifier, sprintSpeedModifier, walkingNoise, runningNoise, idleNoise;
    Transform playerCam;
    Inputs inputs;
    NoiseMaker nM;

    private void Awake() {
        inputs = FindObjectOfType<Inputs>();
        playerCam = transform.GetChild(0);
        nM = GetComponentInChildren<NoiseMaker>();
    }

    Vector2 mouseInput;
    float cameraAngle;
    void LateUpdate()
    {
        mouseInput = inputs.GetMouseMovement() * Time.deltaTime * 111f;
        transform.RotateAround(transform.position, transform.up, mouseInput.x * lookSpeedX);// * Time.deltaTime);

        cameraAngle = Mathf.Clamp(cameraAngle + mouseInput.y * -lookSpeedY, cameraLimitUp, cameraLimitDown);
        playerCam.localEulerAngles = new Vector3(cameraAngle,0,0);
    }

    Vector3 movementInput;
    float sprintSpeed;
    private void FixedUpdate() {
        if (Time.timeScale < 0)
            return;
        movementInput = inputs.GetMovementInput();
        sprintSpeed = inputs.GetShiftAlt() * sprintSpeedModifier;

        if (sprintSpeed <= 0) {
            sprintSpeed = 1;
        }

        if (movementInput.x != 0)
            transform.position += transform.forward * movementInput.z * moveSpeed * Time.deltaTime * sidewaysMoveSpeedModifier * sprintSpeed;
        else
            transform.position += transform.forward * movementInput.z * moveSpeed * Time.deltaTime * sprintSpeed;

        transform.position += transform.right * movementInput.x * moveSpeed * Time.deltaTime * sidewaysMoveSpeedModifier;

        if (movementInput.x != 0 || movementInput.z != 0) {
            if (sprintSpeed == 1) {
                nM.MakeNoise(walkingNoise);
            }
            else {
                nM.MakeNoise(runningNoise);
            }
        }
        else {
            nM.MakeNoise(idleNoise);
        }
    }
}
