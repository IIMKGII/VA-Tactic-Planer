using System;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable] public class DraggablesForSave
    {
        public int index;
        public bool isActive;
        public float positionX, positionZ, positionY;
        public float rotationX, rotationY, rotationZ, rotationW;
        public float sizeDeltaX, sizeDeltaY;
        public float colorR, colorG, colorB, colorA;
        public String parent, objectName;
        public int imageIndex;

        public DraggablesForSave(int index, bool isActive, Vector3 position, Quaternion rotation, Vector2 sizeDelta, Color color, String parent,
            int imageIndex, String objectName)
        {
            this.index = index;
            this.isActive = isActive;
            this.positionX = position.x;
            this.positionY = position.y;
            this.positionZ = position.z;
            this.rotationX = rotation.x;
            this.rotationY = rotation.y;
            this.rotationZ = rotation.z;
            this.rotationW = rotation.w;
            this.sizeDeltaX = sizeDelta.x;
            this.sizeDeltaY = sizeDelta.y;
            this.colorR = color.r;
            this.colorG = color.g;
            this.colorB = color.b;
            this.colorA = color.a;
            this.parent = parent;
            this.imageIndex = imageIndex;
            this.objectName = objectName;
        }

        public String getParent()
        {
            return this.parent;
        }
        
    }
}