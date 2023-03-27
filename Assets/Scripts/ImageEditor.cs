using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEditor : MonoBehaviour
{
    public float zoomSpeed =1f;
    public float panSpeed = 1f;

    private Vector3 canvasPosition;
    private Vector3 lastMousePosition;
    public float canvasScale;

    void Start()
    {
        canvasPosition = transform.localPosition;
        canvasScale = transform.localScale.x;
    }

    void Update()
    {
        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        canvasScale += scroll * zoomSpeed;
        canvasScale = Mathf.Clamp(canvasScale, 1f, 10f);

        // Pan with middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            delta /= canvasScale; // adjust for canvas scale
            canvasPosition += delta;
            lastMousePosition = Input.mousePosition;
        }

        // Update canvas transform
        transform.localScale = Vector3.one * canvasScale;
        transform.localPosition = canvasPosition;
    }
}
