using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Check if the 'left button' was pressed this frame
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"You clicked on: {hit.collider.name}");
                
                // Example: Trigger a method on the object
                if (hit.collider.TryGetComponent<BoatClickable>(out var clickable))
                {
                    clickable.OnClick();
                }
            }
        }
    }
}