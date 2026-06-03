using UnityEngine;

public class MocopiLook : MonoBehaviour
{
    [Header("プレイヤー")]
    public Transform playerBody;

    [Header("MOCOPI WRIST")]
    public Transform rightWrist;
    public Transform leftWrist;

    [Header("感度")]
    public float horizontalSensitivity = 1f;
    public float verticalSensitivity = 1f;

    [Header("スムージング")]
    public float rotationSmoothSpeed = 15f;

    [Header("上下制限")]
    public float minVerticalAngle = -70f;
    public float maxVerticalAngle = 70f;

    [Header("リコイル")]
    public float recoilRecoverySpeed = 8f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float recoilOffset = 0f;

    private bool initialized = false;
    private Vector3 baseForward;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (playerBody != null)
        {
            yRotation = playerBody.eulerAngles.y;
        }
    }

    void Update()
    {
        if (rightWrist == null || leftWrist == null || playerBody == null)
        {
            return;
        }

        Vector3 gunDirection = leftWrist.position - rightWrist.position;

        if (gunDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        gunDirection.Normalize();

        if (!initialized)
        {
            baseForward = gunDirection;
            initialized = true;
        }

        Quaternion offsetRotation = Quaternion.FromToRotation(baseForward, gunDirection);
        Vector3 offsetEuler = offsetRotation.eulerAngles;

        float targetYaw = NormalizeAngle(offsetEuler.y) * horizontalSensitivity;
        float targetPitch = -NormalizeAngle(offsetEuler.x) * verticalSensitivity;

        yRotation = Mathf.LerpAngle(
            yRotation,
            targetYaw,
            Time.deltaTime * rotationSmoothSpeed
        );

        xRotation = Mathf.LerpAngle(
            xRotation,
            targetPitch,
            Time.deltaTime * rotationSmoothSpeed
        );

        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        float finalPitch = xRotation - recoilOffset;
        finalPitch = Mathf.Clamp(finalPitch, -90f, 90f);

        transform.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
        playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        recoilOffset = Mathf.Lerp(
            recoilOffset,
            0f,
            Time.deltaTime * recoilRecoverySpeed
        );
    }

    public void AddRecoil(float amount)
    {
        recoilOffset += amount;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    public void ResetBaseDirection()
    {
        if (rightWrist == null || leftWrist == null)
        {
            return;
        }

        Vector3 gunDirection = leftWrist.position - rightWrist.position;

        if (gunDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        baseForward = gunDirection.normalized;
        initialized = true;

        xRotation = 0f;
        yRotation = 0f;
        recoilOffset = 0f;
    }
}