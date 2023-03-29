using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using DefaultNamespace;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public TMP_Dropdown imageDropdown;
    public Image mainCanvasImage;
    public Image mainCalloutImage;

    private Draggable currentDraggableImage;
    public Draggable[] draggables = new Draggable[200];
    private int draggableIndex = 0;

    [FormerlySerializedAs("imageSprites")] public Sprite[] normalMaps;
    public Sprite[] calloutMaps;

    private int playerIndex;
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
        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
            for(int i = 0; i < 5; i++)
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves" + "/Save" + i);
            }
        }
        
        
        
        
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
            // currentDraggableImage.transform.position = Input.mousePosition;
            currentDraggableImage.setPosition(Input.mousePosition);
            if (currentDraggableImage.name.Contains("Player(Clone)"))
            {
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setPosition(Input.mousePosition);
            }
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
                            currentDraggableImage = result.gameObject.GetComponent<Draggable>();
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
            GameObject temp = Instantiate(draggableImages[currentIndex], Input.mousePosition, Quaternion.identity);
            temp.AddComponent<Draggable>();
            
            currentDraggableImage = temp.GetComponent<Draggable>();
            currentDraggableImage.setPosition(Input.mousePosition);
            currentDraggableImage.setImageIndex(currentIndex);
            currentDraggableImage.gameObject.name += draggableIndex;
            currentDraggableImage.setObjectName(currentDraggableImage.gameObject.name);

            // currentDraggableImage.transform.SetParent(FindObjectOfType<ImageEditor>().transform);
            currentDraggableImage.setParent(FindObjectOfType<ImageEditor>().transform);
            currentDraggableImage.setIndex(draggableIndex);
            draggables[draggableIndex] = currentDraggableImage;
            
            Destroy(currentDraggableImage.gameObject.GetComponent<Button>());
            
            currentDraggableImage.tag = "button";
            
            float scale = FindObjectOfType<ImageEditor>().canvasScale;
            
            
            if (!currentDraggableImage.name.Contains("Player(Clone)"))
            {
                // currentDraggableImage.GetComponent<Image>().color = currentColor;
                currentDraggableImage.setColor(currentColor);
                
                // currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
                currentDraggableImage.setSizeDelta(new Vector2(50 * scale, 50 * scale));
            }
            else
            {
                draggableIndex++;
                // currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
                currentDraggableImage.setSizeDelta(new Vector2(50 * scale, 50 * scale));

                // currentDraggableImage.gameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(100 * scale, 100 * scale);
                // currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setParent(currentDraggableImage.getParent());
                temp.transform.GetChild(0).GetComponent<Draggable>().setParent(currentDraggableImage.transform);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setSizeDelta(new Vector2(100 * scale, 100 * scale));
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setIndex(draggableIndex);
                draggables[draggableIndex] = currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>();
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setActive(false);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setPosition(currentDraggableImage.getPosition());
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setColor(Color.white);
                currentDraggableImage.setColor(Color.white);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setActive(false);

                int saveIndex = playerIndex + 5;
                currentDraggableImage.setImageIndex(saveIndex);
                saveIndex = 11 + playerIndex;
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setImageIndex(saveIndex);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setObjectName(currentDraggableImage.transform.GetChild(0).gameObject.name);
            }
            draggableIndex++;
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
            draggables[currentDraggableImage.getIndex()] = null;
            Destroy(currentDraggableImage.gameObject);
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
        if (!currentDraggableImage.name.Contains("Player(Clone)"))
        {
            // currentDraggableImage.transform.rotation = Quaternion.Euler(0, 0, number*360);
            currentDraggableImage.setRotation(Quaternion.Euler(0, 0, number*360));
        }
        else
        {
            // currentDraggableImage.transform.GetChild(0).gameObject.SetActive(number != 0);
            currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setActive(number != 0);
            // currentDraggableImage.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, number*360);
            currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setRotation(Quaternion.Euler(0, 0, number*360));
            
            currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setLocalPosition(Vector3.zero);
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
            playerIndex = i;
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

    public void saveDraggableItemsToFile(int index)
    {
        int counter = 0;
        for (int i = 0; i < draggables.Length; i++)
        {
            if (draggables[i] != null)
            {
                counter++;
            }
        }
        
        DraggablesForSave[] draggablesForSaves = new DraggablesForSave[counter];

        counter = 0;
        for (int i = 0; i < draggables.Length; i++)
        {
            if(draggables[i] != null)
            {

                draggablesForSaves[counter] = new DraggablesForSave(draggables[i].getIndex(),
                    draggables[i].getActive(), draggables[i].getPosition(), draggables[i].getRotation(),
                    draggables[i].getSizeDelta(), draggables[i].getColor(), draggables[i].getParent().name, draggables[i].getImageIndex(), draggables[i].getObjectName());
                counter++;
            }
            
        }
        
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/Saves/Save" + index+ "/savedDraggableItems.dat", FileMode.Create);

        Texture2D Sprite = FindObjectOfType<ImageEditor>().imageComponent.sprite.texture;
        
        byte[] bytes = Sprite.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/Saves/Save" + index+ "/savedImage.png", bytes);
        
        

        bf.Serialize(stream, draggablesForSaves);
        stream.Close();
    }
    
    public void loadDraggableItemsFromFile(int index)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/Saves/Save" + index+ "/savedDraggableItems.dat", FileMode.Open);

        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/Saves/Save" + index + "/savedImage.png");

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        FindObjectOfType<ImageEditor>().newDrawing(texture);
        


        DraggablesForSave[] draggablesCopy = (DraggablesForSave[]) bf.Deserialize(stream);
        stream.Close();

        for(int i = 0; i < draggablesCopy.Length; i++)
        {
            if (draggablesCopy[i] != null)
            {
                GameObject temp = null;
                if (draggablesCopy[i].imageIndex <= 4)
                { 
                    temp = Instantiate(draggableImages[draggablesCopy[i].imageIndex], new Vector3(draggablesCopy[i].positionX, draggablesCopy[i].positionY, draggablesCopy[i].positionZ), new Quaternion(draggablesCopy[i].rotationX, draggablesCopy[i].rotationY, draggablesCopy[i].rotationZ, draggablesCopy[i].rotationW));
                    temp.AddComponent<Draggable>();
                    Destroy(temp.transform.GetComponent<Button>());
                    draggables[i] = temp.GetComponent<Draggable>();
                    draggables[i].gameObject.tag = "button";
                }else if (draggablesCopy[i].imageIndex <= 10)
                {
                    temp = Instantiate(draggableImages[5], new Vector3(draggablesCopy[i].positionX, draggablesCopy[i].positionY, draggablesCopy[i].positionZ), new Quaternion(draggablesCopy[i].rotationX, draggablesCopy[i].rotationY, draggablesCopy[i].rotationZ, draggablesCopy[i].rotationW));
                    Destroy(temp.transform.GetChild(0).gameObject);
                    temp.AddComponent<Draggable>();
                    draggables[i] = temp.GetComponent<Draggable>();
                    draggables[i].transform.GetComponent<Image>().sprite = players[draggablesCopy[i].imageIndex - 5];
                    draggables[i].gameObject.tag = "button";
                }
                else
                {
                    temp = Instantiate(draggableImages[5].transform.GetChild(0).gameObject);
                    draggables[i] = temp.GetComponent<Draggable>();
                    draggables[i].transform.GetComponent<Image>().sprite = arrows[draggablesCopy[i].imageIndex - 11];
                    draggables[i].gameObject.tag = "Untagged";
                }
               
                draggables[i].setObjectName(draggablesCopy[i].objectName);
                draggables[i].setIndex(draggablesCopy[i].index);
                draggables[i].setImageIndex(draggablesCopy[i].imageIndex);
                draggables[i].setActive(draggablesCopy[i].isActive);
                draggables[i].setPosition(new Vector3(draggablesCopy[i].positionX, draggablesCopy[i].positionY, draggablesCopy[i].positionZ));
                draggables[i].setRotation(new Quaternion(draggablesCopy[i].rotationX, draggablesCopy[i].rotationY, draggablesCopy[i].rotationZ, draggablesCopy[i].rotationW));
                draggables[i].setSizeDelta(new Vector2(draggablesCopy[i].sizeDeltaX, draggablesCopy[i].sizeDeltaY));
                draggables[i].setColor(new Color(draggablesCopy[i].colorR, draggablesCopy[i].colorG, draggablesCopy[i].colorB, draggablesCopy[i].colorA));
                draggables[i].setParent(GameObject.Find(draggablesCopy[i].parent).transform);
            }
        }
    }
}
