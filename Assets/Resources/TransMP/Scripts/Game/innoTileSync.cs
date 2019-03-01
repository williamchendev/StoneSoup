using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Sync", menuName = "TileSync")]
public class innoTileSync : ScriptableObject
{
    [Header("Tile Data")]
    public string tile_name;
    public Sprite[] sprites;

    [Header("Network Animation Settings")]
    public bool update_sprite;
    public bool update_flip;

    [Header("Network Transform Settings")]
    public bool update_transform;
    public bool position;
    public bool rotation;

    [Header("Sync Settings")]
    public bool player;
    public float sync_rate = 10;
    
}
