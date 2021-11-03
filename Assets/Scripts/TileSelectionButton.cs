using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TileSelectionButton : MonoBehaviour
{
    public TilePlacer tilePlacer;

    public TileComponent tilePrefabContained;

    public Button buttonAttachedTo;

    public void Initalise(TileComponent tilePrefabContained, TilePlacer tilePlacer)
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
