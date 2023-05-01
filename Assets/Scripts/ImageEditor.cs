using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TMPro;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;


public class ImageEditor : MonoBehaviour
{
    public float zoomSpeed = 1f;

    public RectTransform panel;
    
    public Camera cam;
    public Canvas mainCanvas;
    public GameObject[] hideScreenshotObjects;
    
    private Vector3 canvasPosition;
    private Vector3 lastMousePosition;

    public float canvasScale;
    
    public Image imageComponent;

    private Vector3 positionBuffer;
    private float scaleBuffer;
    
    public Texture2D texture;
    private Color[] colors;
    private Vector2 previousPosition;
    private bool isDrawing;
    private int thickness;

    private int counter;

    private TMP_InputField inputField;
    
    public void resetScale()
    {
        positionBuffer = canvasPosition;
        scaleBuffer = canvasScale;
        
        canvasPosition = Vector3.zero;
        canvasScale = 1f;
    }
    
    void Awake()
    {
        canvasPosition = transform.localPosition;
        canvasScale = transform.localScale.x;
        
        setAllPixelsTransparent();
    }

    private void Start()
    {
        inputField = FindObjectOfType<TMP_InputField>();

        setTextFieldPosition();
    }

    public void clearAll()
    {
        Controller controller = FindObjectOfType<Controller>();
        setAllPixelsTransparent();
        Draggable[] draggables = controller.getDraggables();

        foreach (var draggable in draggables)
        {
            if (draggable != null)
            {
                draggables[draggable.getIndex()] = null;
                Destroy(draggable.gameObject);
            }
        }
        controller.resetDraggableIndex();
    }
    
    public void setTextFieldPosition()
    {
        float scaler = calculateScaler();
        
        float calculateWIdth = gameObject.GetComponent<Image>().sprite.texture.width / scaler;
        
        inputField.transform.localPosition = new Vector3(calculateWIdth/2f + inputField.GetComponent<RectTransform>().rect.width/2f , 0, 0);
    }

    private float calculateScaler()
    {
        float scaler = gameObject.GetComponent<Image>().sprite.texture.height /
                       gameObject.GetComponent<RectTransform>().rect.height;
        return scaler;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            saveScreenshot(false, true);
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            saveScreenshot(true, false);
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            saveScreenshot(false, false);
        }

        draw();
        if(Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height)
        {
            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            canvasScale += scroll * zoomSpeed;
            canvasScale = Mathf.Clamp(canvasScale, 1f, 10f);
        }
        
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

    private void draw()
    {
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
            //check if mouse is over gameobject tagged UIElement
            PointerEventData pointerData = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
            List<RaycastResult> results2 = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results2);
            foreach (RaycastResult result in results2)
            {
            
                if (result.gameObject.CompareTag("UIElement"))
                {
                    return;
                }
            }
            
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
            // Define the center and radius of the circle
            int centerX = x1+size/2;
            int centerY = y1+size/2;
            int radius = size;

            // Iterate over a rectangular area that encloses the circle
            for (int y = centerY - radius; y <= centerY + radius; y++) {
                for (int x = centerX - radius; x <= centerX + radius; x++) {
                    // Calculate the distance from the center of the circle
                    int dx2 = x - centerX;
                    int dy2 = y - centerY;
                    float distance = (float) Math.Sqrt(dx2*dx2 + dy2*dy2);

                    // Check if the pixel falls inside the circle
                    if (distance <= radius) {
                        // Calculate the index of the pixel in the colors array
                        int pixelIndex = y * texture.width + x;
                        colors[pixelIndex] = color;
                    }
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

    public void saveScreenshot(bool showText, bool zoom)
    {
        foreach (var hideObject in hideScreenshotObjects)
        {
            hideObject.SetActive(false);
        }

        Vector3 bufferPos;
        
        if (!showText)
        {
            
            inputField.gameObject.SetActive(false);

            if (zoom)
            {
                float panelWidth = (Screen.height/1080f) * 510;
                Debug.Log(panelWidth);
                cam.targetTexture = new RenderTexture(Screen.width - (int) panelWidth, Screen.height, 24);
            }
            else
            {
                cam.targetTexture = new RenderTexture(transform.GetComponent<Image>().sprite.texture.width,
                    transform.GetComponent<Image>().sprite.texture.height, 24);
            }
            
            bufferPos = gameObject.GetComponent<RectTransform>().position;
            gameObject.GetComponent<RectTransform>().position = mainCanvas.gameObject.transform.position;
        }
        else
        {
            inputField = FindObjectOfType<TMP_InputField>();

            float scaler = calculateScaler();

            float sth = 16f / 9;
            float sth2 = (float )Screen.width / Screen.height;

            sth /= sth2;
            
            float xOffset = Screen.width / 2560f;
            xOffset *= sth;
            
            float calculateWidth = gameObject.GetComponent<Image>().sprite.texture.width / scaler;
            float calculateHeight = gameObject.GetComponent<Image>().sprite.texture.height / scaler;

            cam.targetTexture = new RenderTexture( (int)calculateWidth  + (int) inputField.GetComponent<RectTransform>().rect.width, (int) calculateHeight, 24);
            
            bufferPos = gameObject.GetComponent<RectTransform>().position;
            gameObject.GetComponent<RectTransform>().position 
                = new Vector3( mainCanvas.gameObject.transform.localPosition.x 
                             -400*xOffset, mainCanvas.gameObject.transform.position.y, 0);
            
        }

        if (!zoom)
        {
            gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
        else
        {
            gameObject.GetComponent<RectTransform>().position = new Vector3(bufferPos.x - panel.sizeDelta.x/2f, bufferPos.y, bufferPos.z);
        }
        
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.worldCamera = cam;
            
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        int height = cam.targetTexture.height;
        int width = cam.targetTexture.width;

        Texture2D Image = new Texture2D(width, height);
        Image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);
        
        
        String fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        File.WriteAllBytes(Application.dataPath + "/../Screenshots/Screenshot"+ fileName +".png", Bytes);

        iTextSharp.text.Rectangle pageSize = null;
 
        using (var srcImage = new Bitmap(Application.dataPath + "/../Screenshots/Screenshot"+ fileName + ".png"))
        {
            pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
        }

        using (var ms = new MemoryStream())
        {
            var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
            document.Open();
            var image = iTextSharp.text.Image.GetInstance(Application.dataPath + "/../Screenshots/Screenshot"+ fileName + ".png");
            document.Add(image);
            document.Close();

            File.WriteAllBytes(Application.dataPath + "/../Screenshots/Screenshot"+ fileName + ".pdf", ms.ToArray());
        }


        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        counter = 0;
            
        foreach (var hideObject in hideScreenshotObjects)
        {
            hideObject.SetActive(true);
        }
        inputField.gameObject.SetActive(true);
    }

    private void setAllPixelsTransparent()
    {
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
}
