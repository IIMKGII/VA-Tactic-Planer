using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    Vector3 position;
    float size;
    Color color;
    
    public Draggable(Vector3 position, float size, Color color)
    {
        this.position = position;
        this.size = size;
        this.color = color;
    }
    
    public Vector3 getPosition()
    {
        return position;
    }
    
    public float getSize()
    {
        return size;
    }
    
    public Color getColor()
    {
        return color;
    }
    
    public void setPosition(Vector3 position)
    {
        this.position = position;
    }
    
    public void setSize(float size)
    {
        this.size = size;
    }
    
    public void setColor(Color color)
    {
        this.color = color;
    }
}
