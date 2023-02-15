using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class FurnitureData : ScriptableObject
{
    public string furnitureName;
    public Texture2D furnitureIcon;
    public GameObject furniturePrefab;
}
