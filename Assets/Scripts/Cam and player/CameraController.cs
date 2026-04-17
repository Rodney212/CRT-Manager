using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    [Header("Movement")]
    public float panSpeed = 20f;

    [Header("Rotation")]
    public float rotateSpeed = 80f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = 10f;
    public float maxZoom = 80f;

    void Update()
    {
        HandlePan();
        HandleRotation();
        HandleZoom();
//        Debug.Log(Mouse.current.scroll.ReadValue().y);
    }

    void HandlePan()
    {
        Vector2 input = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
        );

        Vector3 move = (transform.right * input.x + transform.forward * input.y) * panSpeed * Time.deltaTime;
        move.y = 0;
        transform.position += move;
    }

    void HandleRotation()
    {
        if (Keyboard.current.qKey.isPressed)
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);
        if (Keyboard.current.eKey.isPressed)
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    //dodgy, fix in the future
void HandleZoom()
{
    float scroll = Mouse.current.scroll.ReadValue().y;
    
    if (Mathf.Abs(scroll) > 0.1f)
    {
        // 1. Calculate the new local Z
        // Usually, + scroll moves the camera "forward" (towards 0)
        float newLocalZ = cam.transform.localPosition.z + (scroll * zoomSpeed * Time.deltaTime);

        // 2. Clamp using the negative values
        // Remember: minZoom (closer, e.g., -10) must be the SECOND parameter
        // and maxZoom (further, e.g., -80) must be the FIRST parameter in the Clamp
        // because -80 is smaller than -10.
        newLocalZ = Mathf.Clamp(newLocalZ, -maxZoom, -minZoom);

        // 3. Apply
        cam.transform.localPosition = new Vector3(0, 0, newLocalZ);
    }
}
}