using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TileSelectionButton : MonoBehaviour
{
    public TilePlacer tilePlacer;

    public Tile tilePrefabContained;

    public Button buttonAttachedTo;

    public void Initalise(Tile tilePrefabContained, TilePlacer tilePlacer)
    {
        this.tilePrefabContained = tilePrefabContained;
        this.tilePlacer = tilePlacer;
        buttonAttachedTo.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        tilePlacer.SwitchTilePrefab(tilePrefabContained);
    }
}
