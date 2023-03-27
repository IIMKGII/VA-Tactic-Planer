using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public TMP_Dropdown imageDropdown;
    public Image mainCanvasImage;
    
    private GameObject currentDraggableImage;

    public Sprite[] imageSprites;
    
    public GameObject[] draggableImages;

    private bool isSelected, smallDrag, deleteImg;

    private int currentIndex;
    
    private float timer;

    void Start()
    {
        // Populate the dropdown options
        imageDropdown.ClearOptions();
        foreach (Sprite sprite in imageSprites)
        {
            imageDropdown.options.Add(new TMP_Dropdown.OptionData(sprite.name));
        }
        imageDropdown.value = 0;
        imageDropdown.RefreshShownValue();
    }

    public void OnDropdownValueChanged(int value)
    {
        // Set the sprite on the main canvas image component
        mainCanvasImage.sprite = imageSprites[value];
        mainCanvasImage.preserveAspect = true;
    }


    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(currentDraggableImage);
        }


        if (Input.GetMouseButtonUp(0))
        {
            timer = 0f;
            smallDrag = false;
        }
        
        if (smallDrag)
        {
            currentDraggableImage.transform.position = Input.mousePosition;
        }
        
        
        if (isSelected && Input.GetMouseButtonDown(0))
        {
            currentDraggableImage = Instantiate(draggableImages[currentIndex], Input.mousePosition, Quaternion.identity);
            currentDraggableImage.transform.SetParent(FindObjectOfType<ImageEditor>().transform);
            Destroy(currentDraggableImage.GetComponent<Button>());
            currentDraggableImage.tag = "button";
            float scale = FindObjectOfType<ImageEditor>().canvasScale;
            currentDraggableImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100 * scale, 100 * scale);
        }

        bool foundSth = false;
        
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
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

        isSelected = !foundSth;
    }

    public void currentSelected(int i)
    {
        deleteImg = false;
        currentIndex = i;
        isSelected = true; 
        Destroy(currentDraggableImage);
    }

    public void exportPDF()
    {
        isSelected = false;
    }
    
    
    
}
