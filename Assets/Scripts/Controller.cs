using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public TMP_Dropdown imageDropdown;
    public Image mainCanvasImage;
    public Image mainCalloutImage;

    private GameObject currentDraggableImage;

    [FormerlySerializedAs("imageSprites")] public Sprite[] normalMaps;
    public Sprite[] calloutMaps;

    public Sprite[] players, arrows;
    
    public Image playerImage, arrowImage;
    
    public GameObject[] draggableImages;

    private bool isSelected, smallDrag, deleteImg, drawingEnabled;

    private int currentIndex;
    
    private float timer;

    public Button colorButton;

    public Color currentColor;

    void Start()
    {
        changeThickness(0.2167675f);
        // Populate the dropdown options
        imageDropdown.ClearOptions();
        foreach (Sprite sprite in normalMaps)
        {
            imageDropdown.options.Add(new TMP_Dropdown.OptionData(sprite.name));
        }
        imageDropdown.value = 0;
        imageDropdown.RefreshShownValue();
        
        currentColor = new Color(0.00392156862745098f, 0.42745098039215684f, 0.5019607843137255f);
        setupColors();
    }

    public void OnDropdownValueChanged(int value)
    {
        // Set the sprite on the main canvas image component
        mainCanvasImage.sprite = normalMaps[value];
        mainCanvasImage.preserveAspect = true;
        mainCalloutImage.sprite = calloutMaps[value];
        mainCalloutImage.preserveAspect = true;
    }
    

    private void Update()
    {
        checkNumbers();
        
        if (smallDrag)
        {
            currentDraggableImage.transform.position = Input.mousePosition;
        }
        
        

        bool foundSth = false;
        bool spawn = true;
        
        if (EventSystem.current.IsPointerOverGameObject() && !drawingEnabled)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("UIElement"))
                {
                    spawn = false;
                    break;
                }
                if (result.gameObject.CompareTag("button") && !smallDrag)
                {
                    foundSth = true;
                    if (Input.GetMouseButton(0))
                    {
                        timer += Time.deltaTime;
                        if(timer > 0.05f)
                        {
                            currentDraggableImage = result.gameObject;
                            smallDrag = true;
                        }

                        if (deleteImg)
                        {
                            Destroy(result.gameObject);
                        }
                    }
                }
            }
        }
        
        if (isSelected && Input.GetMouseButtonDown(0) && !drawingEnabled && spawn)
        {
            currentDraggableImage = Instantiate(draggableImages[currentIndex], Input.mousePosition, Quaternion.identity);
            currentDraggableImage.transform.SetParent(FindObjectOfType<ImageEditor>().transform);
            Destroy(currentDraggableImage.GetComponent<Button>());
            currentDraggableImage.tag = "button";
            
            float scale = FindObjectOfType<ImageEditor>().canvasScale;
            
            
            if (currentDraggableImage.name != "Player(Clone)")
            {
                currentDraggableImage.GetComponent<Image>().color = currentColor;
                currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
            }
            else
            {
                currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
                currentDraggableImage.gameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(100 * scale, 100 * scale);
                currentDraggableImage.transform.GetChild(0).gameObject.SetActive(false);
            }
            
        }

        isSelected = !foundSth;
    }

    private void checkNumbers()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            colorSelector(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            colorSelector(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            colorSelector(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            colorSelector(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            colorSelector(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            colorSelector(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            colorSelector(6);
        }
        setupColors();

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Scrollbar scrollbar = FindObjectOfType<Scrollbar>();
            if (scrollbar.value > 0)
            {
                changeThickness(FindObjectOfType<Scrollbar>().value - (1f / 11f));
                FindObjectOfType<Scrollbar>().value -= (1f / 11f);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Scrollbar scrollbar = FindObjectOfType<Scrollbar>();
            if (scrollbar.value < 1)
            {
                changeThickness(FindObjectOfType<Scrollbar>().value + (1f / 11f));
                FindObjectOfType<Scrollbar>().value += (1f / 11f);
            }
            
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(currentDraggableImage);
        }
        if (Input.GetMouseButtonUp(0))
        {
            timer = 0f;
            smallDrag = false;
        }
    }

    public void currentSelected(int i)
    {
        deleteImg = false;
        drawingEnabled = false;
        currentIndex = i;
        isSelected = true;
    }

    public void rotateObject(float number)
    {
        if (currentDraggableImage.name != "Player(Clone)")
        {
            currentDraggableImage.transform.rotation = Quaternion.Euler(0, 0, number*360);
        }
        else
        {
            currentDraggableImage.transform.GetChild(0).gameObject.SetActive(number != 0);
            currentDraggableImage.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, number*360);
        }
    }


    public void toggleMaps(bool isNormal)
    {
        mainCalloutImage.gameObject.SetActive(!isNormal);
    }
    
    public void exportPDF()
    {
        FindObjectOfType<ImageEditor>().saveScreenshot();
        isSelected = false;
    }

    public void drawMode()
    {
        isSelected = false;
        drawingEnabled = true;
    }
    
    public bool getDrawingEnabled()
    {
        return drawingEnabled;
    }

    public void changeThickness(float thickness)
    {
        FindObjectOfType<ImageEditor>().setThickness((int) (thickness*20));
    }

    public void colorSelector(int i)
    {
        
        if (i < 6)
        {
            arrowImage.sprite = arrows[i];
            playerImage.sprite = players[i];
        }
        
        switch(i)
        {
            
            
            case 0:
                currentColor = new Color(0.00392156862745098f, 0.42745098039215684f, 0.5019607843137255f);

                break;
            case 1:
                currentColor = new Color(0.5137254902f, 0.0745098039f, 0.6196078431f);

                break;
            case 2:
                currentColor = new Color(0.0823529412f, 0.8078431373f, 0.8901960784f);
                break;
            case 3:
                currentColor = new Color(0.6549019608f, 0.0470588235f, 0.2352941176f);

                break;
            case 4:
                currentColor = new Color(0.0392156863f, 0.7019607843f, 0.3490196078f);
                break;
            case 5: 
                currentColor = new Color(0.9333333333f, 0.9294117647f, 0.2078431373f);
                break;
            case 6: 
                currentColor = Color.black;
                currentColor.a = 0;
                break;
                
            
        }

        setupColors();
    }


    private void setupColors()
    {
        ColorBlock colorBlock = new ColorBlock
        {
            normalColor = currentColor,
            highlightedColor = currentColor,
            pressedColor = currentColor,
            disabledColor = currentColor,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };

        colorButton.colors = colorBlock;

        foreach (var image in draggableImages)
        {
            if (image.name != "Player")
            {
                image.GetComponent<Button>().colors = colorBlock;
            }
        }
    }
}
