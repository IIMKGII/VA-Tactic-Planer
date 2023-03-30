using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Draggable : MonoBehaviour
{
    public Vector3 position;
    public Color color;
    public bool isActive = true;
    public int index;
    public Vector2 sizeDelta;
    public Quaternion rotation;
    public Transform parent;
    public int imageIndex;
    public String objectName;

    
    
    public void setLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }

    public void setObjectName(String objectName)
    {
        this.objectName = objectName;
        gameObject.name = objectName;
    }
    
    public String getObjectName()
    {
        return objectName;
    }
    
    public void setImageIndex(int imageIndex)
    {
        this.imageIndex = imageIndex;
    }
    
    public int getImageIndex()
    {
        return imageIndex;
    }
    
    public void setMyParent(Transform parent)
    {
        this.parent = parent;
        transform.SetParent(parent);
    }
    
    public Transform getParent()
    {
        return parent;
    }
    
    public Vector2 getSizeDelta()
    {
        return sizeDelta;
    }
    
    public void setSizeDelta(Vector3 sizeDelta)
    {
        this.sizeDelta = sizeDelta;
        gameObject.GetComponent<RectTransform>().localScale = sizeDelta;


    }
    
    public Quaternion getRotation()
    {
        return rotation;
    }
    
    public void setRotation(Quaternion rotation)
    {
        this.rotation = rotation;
        transform.rotation = rotation;
    }
    
    
    public Vector3 getPosition()
    {
        return position;
    }

    public Color getColor()
    {
        return color;
    }
    
    public void setPosition(Vector3 position)
    {
        this.position = position;
        
        transform.position = position;
    }

    public void setColor(Color color)
    {
        this.color = color;
        gameObject.GetComponent<Image>().color = color;
    }
    
    public int getIndex()
    {
        return index;
    }
    
    public bool getActive()
    {
        return isActive;
    }
    
    public void setActive(bool isActive)
    {
        this.isActive = isActive;
        gameObject.SetActive(isActive);
    }
    
    public void setIndex(int index)
    {
        this.index = index;
    }

}
