using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    public Camera cameraInUse;

    public Tile prefabInUse;

    public TileExampleGrid tileGrid;

    public GameObject cursorObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Ray cast to the grid squares
        if (Physics.Raycast(cameraInUse.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit, 100f))
        {
            if (raycastHit.collider.TryGetComponent<Tile>(out Tile tile))
            {
                cursorObject.transform.position = tile.transform.position + raycastHit.normal;

                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 localNormal = tile.transform.InverseTransformDirection(raycastHit.normal);
                    Vector3Int placementGridCoordinates = tile.gridCoordinates + new Vector3Int(Mathf.RoundToInt(localNormal.x), Mathf.RoundToInt(localNormal.y), Mathf.RoundToInt(localNormal.z));
                    if (tileGrid.CheckIfGridCoordinatesValid(placementGridCoordinates))
                    {
                        Tile newTile = GameObject.Instantiate(prefabInUse.gameObject, cursorObject.transform.position, cursorObject.transform.rotation).GetComponent<Tile>();
                        tileGrid.TryAddTile(newTile, placementGridCoordinates);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            cameraInUse.transform.RotateAround(Vector3.zero, Vector3.up, 90f);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            cameraInUse.transform.RotateAround(Vector3.zero, Vector3.up, -90f);
        }
    }

    public void SwitchTilePrefab(Tile tileToUse)
    {
        prefabInUse = tileToUse;

        // Set up cursor
        if (cursorObject != null)
        {
            GameObject newCursor = GameObject.Instantiate(prefabInUse.gameObject, cursorObject.transform.position, cursorObject.transform.rotation);
            Destroy(cursorObject);
            if (newCursor.TryGetComponent<Collider>(out Collider collider))
            {
                Destroy(collider);
            }
            cursorObject = newCursor;
        }
    }
}
