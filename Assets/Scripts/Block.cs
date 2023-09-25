using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool empty = true;
    public Tile tile = null;
    public Vector3 pos => transform.position;

    public void Fill(Tile newTile) {
        tile = newTile;
        empty = false;
        tile.block = this;
    }

    public void Empty() {
        tile.block = null;
        tile = null;
        empty = true;
    }
}
