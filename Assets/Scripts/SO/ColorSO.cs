using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Color")]
public class ColorSO : ScriptableObject{

    public List<BlockType> blocks = new();

}

[System.Serializable]
public struct BlockType {
    public int value;
    public Color color;
}