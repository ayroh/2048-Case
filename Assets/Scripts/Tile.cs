using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{

    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private ColorSO colorSO;

    public Block block = null;
    public Tile mergingTile = null;
    public bool merging = false;

    public Vector3 pos => transform.position;

    private int _value;
    public int value { 
        set { _value = value;
            valueText.text = _value.ToString();
            renderer.color = colorSO.blocks.Find(block => block.value == _value).color; 
        }
        get { return _value; }
    }


    public void ResetTile() {
        merging = false;
        mergingTile = null;
        block = null;
    }
}
