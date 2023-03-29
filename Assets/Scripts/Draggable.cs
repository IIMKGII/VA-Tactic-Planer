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

    public void onValueChanged()
    {
        transform.rotation = rotation;
        gameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        transform.position = position;
        gameObject.GetComponent<Image>().color = color;
        gameObject.SetActive(isActive);
        transform.SetParent(parent);
    }
    
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
    
    public void setParent(Transform parent)
    {
        this.parent = parent;
        onValueChanged();
    }
    
    public Transform getParent()
    {
        return parent;
    }
    
    public Vector2 getSizeDelta()
    {
        return sizeDelta;
    }
    
    public void setSizeDelta(Vector2 sizeDelta)
    {
        this.sizeDelta = sizeDelta;
        onValueChanged();
    }
    
    public Quaternion getRotation()
    {
        return rotation;
    }
    
    public void setRotation(Quaternion rotation)
    {
        this.rotation = rotation;
        onValueChanged();
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
        onValueChanged();
    }

    public void setColor(Color color)
    {
        this.color = color;
        onValueChanged();
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
        onValueChanged();
    }
    
    public void setIndex(int index)
    {
        this.index = index;
    }

}
