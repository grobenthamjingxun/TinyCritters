using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableItem : MonoBehaviour
{
    private Camera cam;
    private bool dragging = false;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        if (!Mouse.current.leftButton.isPressed) return;

        dragging = true;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pangolin"))
        {
            PangolinManager pm = FindFirstObjectByType<PangolinManager>();
            pm.Feed("banana");
            

            Debug.Log("Fed animal using drag & drop");

            Destroy(gameObject);
        }
    }
}