using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    public Button tileButtonPrefab;

    public VerticalLayoutGroup layoutGroupForPlacement;

    public List<Tile> placeablePrefabs;

    public TilePlacer tilePlacer;

    private void Start()
    {
        for (int i = 0; i < placeablePrefabs.Count; ++i)
        {
            Button newButton = GameObject.Instantiate(tileButtonPrefab.gameObject, layoutGroupForPlacement.transform).GetComponent<Button>();
            ColorBlock colorBlock = ColorBlock.defaultColorBlock;
            colorBlock.normalColor = placeablePrefabs[i].GetComponent<Renderer>().sharedMaterial.color;
            tileButtonPrefab.GetComponent<Button>().colors = colorBlock;

            newButton.gameObject.name = placeablePrefabs[i].name + " Button";
            newButton.GetComponent<TileSelectionButton>().Initalise(placeablePrefabs[i], tilePlacer);
        }
    }
}
