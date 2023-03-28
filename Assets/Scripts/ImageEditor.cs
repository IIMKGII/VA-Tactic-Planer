using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;


public class ImageEditor : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float panSpeed = 1f;

    public Camera cam;
    public Canvas mainCanvas;
    public GameObject[] hideScreenshotObjects;
    
    private Vector3 canvasPosition;
    private Vector3 lastMousePosition;

    public float canvasScale;
    
    public Image imageComponent;


    private Texture2D texture;
    private Color[] colors;
    private Vector2 previousPosition;
    private bool isDrawing;
    private int thickness;

    private int counter;
    private bool doScreenshot = false;



    void Start()
    {

        canvasPosition = transform.localPosition;
        canvasScale = transform.localScale.x;


        texture = new Texture2D((int)imageComponent.rectTransform.rect.width,
            (int)imageComponent.rectTransform.rect.height);
        colors = texture.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
            colors[i].a = 0;
        }

        texture.SetPixels(colors);
        texture.Apply();
        imageComponent.sprite =
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            saveScreenshot();
        }

        if (Input.GetMouseButtonDown(0) && FindObjectOfType<Controller>().getDrawingEnabled())
        {
            isDrawing = true;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageComponent.rectTransform, Input.mousePosition,
                null, out localPoint);
            previousPosition = localPoint;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageComponent.rectTransform, Input.mousePosition,
                null, out localPoint);
            int x1 = Mathf.RoundToInt(previousPosition.x + imageComponent.rectTransform.rect.width / 2);
            int y1 = Mathf.RoundToInt(previousPosition.y + imageComponent.rectTransform.rect.height / 2);
            int x2 = Mathf.RoundToInt(localPoint.x + imageComponent.rectTransform.rect.width / 2);
            int y2 = Mathf.RoundToInt(localPoint.y + imageComponent.rectTransform.rect.height / 2);

            Color currentColor = FindObjectOfType<Controller>().currentColor;

            if (currentColor.a == 0)
            {
                DrawLine(x1, y1, x2, y2, currentColor, 20);
            }
            else
            {
                DrawLine(x1, y1, x2, y2, currentColor, thickness);
            }
            previousPosition = localPoint;
        }



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

    public void ClearDrawing()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    public void setThickness(int thickness)
    {
        if (thickness == 0)
        {
            thickness = 1;
        }
        this.thickness = thickness;
    }

    public int getThickness()
    {
        return thickness;
    }

    void DrawLine(int x1, int y1, int x2, int y2, Color color, int size)
    {

        x1 = x1 - size / 2;
        y1 = y1 - size / 2;
        x2 = x2 - size / 2;
        y2 = y2 - size / 2;
        
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int pixelIndex = (y1 + i) * texture.width + (x1 + j);
                    colors[pixelIndex] = color;
                }
            }
        
            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
    }

    public void saveScreenshot()
    {
        foreach (var hideObject in hideScreenshotObjects)
        {
            hideObject.SetActive(false);
        }

        gameObject.GetComponent<RectTransform>().position = new Vector3(1372, 720, 0);
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.worldCamera = cam;
            
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);

        File.WriteAllBytes("Screenshot.png", Bytes);
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        counter = 0;
            
        foreach (var hideObject in hideScreenshotObjects)
        {
            hideObject.SetActive(true);
        }
    }
}
