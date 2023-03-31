using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class Controller : MonoBehaviour
{
    public TMP_Dropdown imageDropdown;
    public Image mainCanvasImage;
    public Image mainCalloutImage;

    private Draggable currentDraggableImage;
    public Draggable[] draggables = new Draggable[200];
    private int draggableIndex = 0;

    public Sprite[] normalMaps = new Sprite[50];
    public Sprite[] calloutMaps = new Sprite[50];

    private int playerIndex;
    public Sprite[] players, arrows;
    
    public Image playerImage, arrowImage;
    
    public GameObject[] draggableImages;

    private bool isSelected, smallDrag, deleteImg, drawingEnabled;

    private int currentIndex;
    
    private float timer;

    private float imageScale = 0.6f;
    
    public Button colorButton;

    public Color currentColor;

    private bool reloadFirstTimeLoad;
    
    int saveCounter = 0;
    
    private int reloadIndex;
    private GameObject[] markForDestroy;
    
    
    void Awake()
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
        
        
        if (!Directory.Exists(Application.persistentDataPath + "/Maps"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Maps");
        }
        
        if (!Directory.Exists(Application.dataPath + "/../Screenshots"))
        {
            Directory.CreateDirectory(Application.dataPath + "/../Screenshots");
        }

        if (!Directory.Exists(Application.persistentDataPath + "/CalloutMaps"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/CalloutMaps");
        }
        
        String[] mapLocations = Directory.GetFiles (Application.persistentDataPath + "/Maps");
        String[] calloutMapLocations = Directory.GetFiles (Application.persistentDataPath + "/CalloutMaps");
        Texture2D[] mapTexture = new Texture2D[mapLocations.Length];
        Texture2D[] calloutMapTexture = new Texture2D[calloutMapLocations.Length];
        

        for (int i = 0; i < mapLocations.Length; i++)
        {
            byte[] bytes = File.ReadAllBytes(mapLocations[i]);
            byte[] bytes2 = File.ReadAllBytes(calloutMapLocations[i]);

            mapTexture[i] = new Texture2D(2, 2);
            mapTexture[i].LoadImage(bytes);
            calloutMapTexture[i] = new Texture2D(2, 2);
            calloutMapTexture[i].LoadImage(bytes2);
            
            normalMaps[i] = Sprite.Create(mapTexture[i], new Rect(0, 0, mapTexture[i].width, mapTexture[i].height), Vector2.one * 0.5f);
            
            normalMaps[i].name = mapLocations[i].Substring(mapLocations[i].LastIndexOf("/") + 6).Remove(mapLocations[i].Substring(mapLocations[i].LastIndexOf("/") + 6).Length -4);
            
            calloutMaps[i] = Sprite.Create(calloutMapTexture[i], new Rect(0, 0, calloutMapTexture[i].width, calloutMapTexture[i].height), Vector2.one * 0.5f);
        }

        if (normalMaps.Length > 1)
        {
            mainCanvasImage.sprite = normalMaps[0];
            mainCanvasImage.preserveAspect = true;
            mainCalloutImage.sprite = calloutMaps[0];
            mainCalloutImage.preserveAspect = true;
        }

        imageDropdown.ClearOptions();
        foreach (Sprite sprite in normalMaps)
        {
            if (sprite != null)
            {
                imageDropdown.options.Add(new TMP_Dropdown.OptionData(sprite.name));
            }
        }
        
        imageDropdown.value = 0;
        imageDropdown.RefreshShownValue();
        
        currentColor= new Color(0.0000000000f, 0.5647058824f, 0.6627450980f);
        setupColors();

        FindObjectOfType<Toggle>().isOn = false;
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

        if (reloadFirstTimeLoad)
        {
            loadDraggableItemsFromFile(reloadIndex);
            reloadFirstTimeLoad = false;
            return;
        }

        if (saveCounter > 0)
        {
            saveDraggableItemsToFile(reloadIndex);
            return;
        }
        
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
        
        if (isSelected && Input.GetMouseButtonDown(0) && !drawingEnabled && spawn && currentColor.a != 0)
        {
            GameObject temp = Instantiate(draggableImages[currentIndex], Input.mousePosition, Quaternion.identity);
            temp.AddComponent<Draggable>();
            
            currentDraggableImage = temp.GetComponent<Draggable>();
            currentDraggableImage.setPosition(Input.mousePosition);
            currentDraggableImage.setImageIndex(currentIndex);
            currentDraggableImage.gameObject.name += draggableIndex;
            currentDraggableImage.setObjectName(currentDraggableImage.gameObject.name);

            // currentDraggableImage.transform.SetParent(FindObjectOfType<ImageEditor>().transform);
            currentDraggableImage.setMyParent(FindObjectOfType<ImageEditor>().transform);
            currentDraggableImage.setIndex(draggableIndex);
            draggables[draggableIndex] = currentDraggableImage;
            
            Destroy(currentDraggableImage.gameObject.GetComponent<Button>());
            
            currentDraggableImage.tag = "button";

            if (!currentDraggableImage.name.Contains("Player(Clone)"))
            {
                // currentDraggableImage.GetComponent<Image>().color = currentColor;
                currentDraggableImage.setColor(currentColor);
                
                // currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
                currentDraggableImage.setSizeDelta(new Vector2(1*imageScale , 1*imageScale ));
            }
            else
            {
                draggableIndex++;
                // currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50 * scale, 50 * scale);
                currentDraggableImage.setSizeDelta(new Vector2(0.6f*imageScale , 0.6f*imageScale ));
                
                
                temp.transform.GetChild(0).GetComponent<Draggable>().setMyParent(currentDraggableImage.transform);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setIndex(draggableIndex);
                draggables[draggableIndex] = currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>();
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setActive(false);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setPosition(currentDraggableImage.getPosition());
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setColor(Color.white);
                currentDraggableImage.transform.GetChild(0).GetComponent<Draggable>().setSizeDelta(new Vector2(1f, 1f));
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

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            drawMode();
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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentSelected(0);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentSelected(1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentSelected(2);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentSelected(3);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentSelected(4);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            currentSelected(5);
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

    public void imageScaler(float scaler)
    {
        imageScale = scaler;

        foreach (var draggable in draggables)
        {
            if (draggable != null)
            {
                if (!draggable.name.Contains("Arrow"))
                {
                    if (draggable.name.Contains("Player(Clone)"))
                    {
                        draggable.setSizeDelta(new Vector2(0.6f*imageScale , 0.6f*imageScale ));
                    }
                    else
                    {
                        draggable.setSizeDelta(new Vector2(1f*imageScale , 1f*imageScale ));
                    }
                }
            }
        }
    }


    public void toggleMaps(bool isNormal)
    {
        mainCalloutImage.gameObject.SetActive(isNormal);
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
                currentColor = new Color(0.0000000000f, 0.5647058824f, 0.6627450980f);

                break;
            case 1:
                currentColor = new Color(0.0000000000f, 1.0000000000f, 0.3372549020f); 

                break;
            case 2:
                currentColor = new Color(0.9764705882f, 0.9803921569f, 0.0078431373f); 
                break;
            case 3:
                currentColor = new Color(1.0000000000f, 0.0000000000f, 0.3176470588f); 

                break;
            case 4:
                currentColor = new Color(0.0901960784f, 0.0000000000f, 1.0000000000f); 
                break;
            case 5: 
                currentColor = new Color(0.9254901961f, 0.0000000000f, 1.0000000000f); 
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

        if (currentColor.a != 0)
        {
            colorButton.transform.GetChild(0).GetComponent<Image>().color = currentColor;
        }

        // foreach (var image in draggableImages)
        // {
        //     if (image.name != "Player" || currentColor.a != 0)
        //     {
        //         image.GetComponent<Button>().colors = colorBlock;
        //     }
        // }

        for (int i = 0; i < draggableImages.Length-1; i++)
        {
            if (currentColor.a != 0)
            {
                draggableImages[i].GetComponent<Button>().colors = colorBlock;
            }
        }
    }

    public void newInstance()
    {
        Process p = new Process();
        p.StartInfo.FileName = Application.dataPath + "/../PavlovStrategyPlaner.exe";
        p.Start();
    }
    
    public void saveDraggableItemsToFile(int index)
    {
        reloadIndex = index;
        if (saveCounter == 0)
        {
            FindObjectOfType<ImageEditor>().resetScale();
            saveCounter++;
            return;
        }

        if (saveCounter == 1)
        {
            for (int i = 0; i < draggables.Length; i++)
            {
                if (draggables[i] != null)
                {
                    draggables[i].setPosition(draggables[i].transform.position);
                }
            }

            saveCounter++;
            FindObjectOfType<ImageEditor>().setToBuffer();
            return;
        }

        if (saveCounter == 2)
        {
            saveCounter = 0;
        }
        
        
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

        String textString = FindObjectOfType<TMP_InputField>().text;
        File.WriteAllText(Application.persistentDataPath + "/Saves/Save" + index+ "/savedText.txt", textString);
        
        

        bf.Serialize(stream, draggablesForSaves);
        stream.Close();
    }
    
    public void loadDraggableItemsFromFile(int index)
    {
        if (!reloadFirstTimeLoad)
        {
            reloadFirstTimeLoad = true;
            reloadIndex = index;
        }

        FindObjectOfType<ImageEditor>().resetScale();
        
        GameObject[] tempObjects = new GameObject[200];
        int tempCounter = 0;

        for (int i = 0; i < draggables.Length; i++)
        {
            if (draggables[i] != null)
            {
                Destroy(draggables[i].gameObject);
                draggables[i] = null;
            }
        }
        
        int counter = 0;
        
        if(!File.Exists(Application.persistentDataPath + "/Saves/Save" + index+ "/savedDraggableItems.dat") || !File.Exists(Application.persistentDataPath + "/Saves/Save" + index + "/savedImage.png") || !File.Exists(Application.persistentDataPath + "/Saves/Save" + index + "/savedText.txt"))
        {
            return;
        }
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/Saves/Save" + index+ "/savedDraggableItems.dat", FileMode.Open);

        byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + "/Saves/Save" + index + "/savedImage.png");

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        FindObjectOfType<ImageEditor>().newDrawing(texture);

        FindObjectOfType<TMP_InputField>().text = File.ReadAllText(Application.persistentDataPath + "/Saves/Save" + index + "/savedText.txt");
        
        DraggablesForSave[] draggablesCopy = (DraggablesForSave[]) bf.Deserialize(stream);
        stream.Close();
        
        
        for(int i = 0; i < draggablesCopy.Length; i++)
        {
            if (draggablesCopy[i] != null)
            {
                counter++;
                GameObject temp = null;
                if (draggablesCopy[i].imageIndex <= 4)
                {
                    
                    GameObject myGameObject = new GameObject();
                    myGameObject.AddComponent<Draggable>();
                    myGameObject.AddComponent<Image>();
                    draggables[i] = myGameObject.GetComponent<Draggable>();
                    draggables[i].gameObject.tag = "button";
                    draggables[i].transform.GetComponent<Image>().sprite = draggableImages[draggablesCopy[i].imageIndex].GetComponent<Image>().sprite;
                    draggables[i].setSizeDelta(new Vector2(imageScale,  imageScale));
                    draggables[i].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
                }else if (draggablesCopy[i].imageIndex <= 10)
                {
                    GameObject myGameObject = new GameObject();
                    myGameObject.AddComponent<Draggable>();
                    myGameObject.AddComponent<Image>();
                    draggables[i] = myGameObject.GetComponent<Draggable>();
                    draggables[i].transform.GetComponent<Image>().sprite = players[draggablesCopy[i].imageIndex - 5];
                    draggables[i].gameObject.tag = "button";
                    draggables[i].setSizeDelta(new Vector2(0.6f*imageScale , 0.6f*imageScale));
                    draggables[i].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
                }
                else
                {
                    GameObject myGameObject = new GameObject();
                    myGameObject.AddComponent<Draggable>();
                    myGameObject.AddComponent<Image>();
                    draggables[i] = myGameObject.GetComponent<Draggable>();
                    draggables[i].transform.GetComponent<Image>().sprite = arrows[draggablesCopy[i].imageIndex - 11];
                    draggables[i].gameObject.tag = "Untagged";
                    draggables[i].setSizeDelta(Vector3.one);
                    tempObjects[i] = myGameObject;
                    continue;
                }
                
                draggables[i].setObjectName(draggablesCopy[i].objectName);
                draggables[i].setIndex(draggablesCopy[i].index);
                draggables[i].setImageIndex(draggablesCopy[i].imageIndex);
                draggables[i].setActive(draggablesCopy[i].isActive);
                draggables[i].setPosition(new Vector3(draggablesCopy[i].positionX, draggablesCopy[i].positionY, draggablesCopy[i].positionZ));
                draggables[i].setRotation(new Quaternion(draggablesCopy[i].rotationX, draggablesCopy[i].rotationY, draggablesCopy[i].rotationZ, draggablesCopy[i].rotationW));
                draggables[i].setColor(new Color(draggablesCopy[i].colorR, draggablesCopy[i].colorG, draggablesCopy[i].colorB, draggablesCopy[i].colorA));
                draggables[i].setMyParent(GameObject.Find(draggablesCopy[i].parent).transform);
            }
        }

        for (int i = 0; i < tempObjects.Length; i++)
        {
            if (tempObjects[i] != null)
            {
                draggables[i].setMyParent(draggables[i-1].transform);
                draggables[i].setObjectName(draggablesCopy[i].objectName);
                draggables[i].setIndex(draggablesCopy[i].index);
                draggables[i].setImageIndex(draggablesCopy[i].imageIndex);
                draggables[i].setActive(draggablesCopy[i].isActive);
                draggables[i].setPosition(new Vector3(draggablesCopy[i].positionX, draggablesCopy[i].positionY, draggablesCopy[i].positionZ));
                draggables[i].setRotation(new Quaternion(draggablesCopy[i].rotationX, draggablesCopy[i].rotationY, draggablesCopy[i].rotationZ, draggablesCopy[i].rotationW));
                draggables[i].setSizeDelta(new Vector2(1f, 1f));
                draggables[i].setColor(new Color(draggablesCopy[i].colorR, draggablesCopy[i].colorG, draggablesCopy[i].colorB, draggablesCopy[i].colorA));
                draggables[i].transform.localScale = Vector3.one;
                
                
            }
        }
        imageScaler(imageScale);
    }
}
