using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Vertical rotation (clamped to avoid flipping)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
