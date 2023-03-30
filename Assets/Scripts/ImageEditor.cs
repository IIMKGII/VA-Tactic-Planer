using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;


public class ImageEditor : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float panSpeed = 1f;

    public Camera cam;
    public Canvas mainCanvas;
    public GameObject[] hideScreenshotObjects;
    
    private Vector3 canvasPosition;
    private Vector3 lastMousePosition;
    
    private Vector3 defaultPosition;
    private float defaultScale;

    public float canvasScale;
    
    public Image imageComponent;


    public Texture2D texture;
    private Color[] colors;
    private Vector2 previousPosition;
    private bool isDrawing;
    private int thickness;

    private int counter;
    private bool doScreenshot = false;
    private int screenShotIndex;



    public void resetScale()
    {
        canvasPosition = Vector3.zero;
        canvasScale = 0f;
    }
    
    void Start()
    {
        canvasPosition = transform.localPosition;
        canvasScale = transform.localScale.x;

        defaultPosition = transform.localPosition;
        defaultScale = transform.localScale.x;

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
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerEventData, results);
                
                foreach (RaycastResult result in results)
                {
                    if (result.gameObject.CompareTag("button"))
                    {
                        FindObjectOfType<Controller>().draggables[result.gameObject.GetComponent<Draggable>().getIndex()] = null;
                        Destroy(result.gameObject);
                    }
                }
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
            canvasPosition += delta;
            lastMousePosition = Input.mousePosition;
        }

        // Update canvas transform
        transform.localScale = Vector3.one * canvasScale;
        transform.localPosition = canvasPosition;
    }

    public void newDrawing(Texture2D texture)
    {
        colors = texture.GetPixels();
        
        this.texture.SetPixels(colors);
        this.texture.Apply();
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

        cam.targetTexture = new RenderTexture(transform.GetComponent<Image>().sprite.texture.width,
            transform.GetComponent<Image>().sprite.texture.height, 24);
        
        // gameObject.GetComponent<RectTransform>().position = new Vector3(1372, 720, 0);
        gameObject.GetComponent<RectTransform>().position = mainCanvas.gameObject.transform.position;
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

        File.WriteAllBytes("Screenshot"+ screenShotIndex +".png", Bytes);

        iTextSharp.text.Rectangle pageSize = null;
 
        using (var srcImage = new Bitmap("Screenshot"+ screenShotIndex + ".png"))
        {
            pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
        }

        using (var ms = new MemoryStream())
        {
            var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
            document.Open();
            var image = iTextSharp.text.Image.GetInstance("Screenshot"+ screenShotIndex + ".png");
            document.Add(image);
            document.Close();

            File.WriteAllBytes("Screenshot"+ screenShotIndex + ".pdf", ms.ToArray());
        }



        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        counter = 0;
            
        foreach (var hideObject in hideScreenshotObjects)
        {
            hideObject.SetActive(true);
        }
        screenShotIndex++;
    }
}
