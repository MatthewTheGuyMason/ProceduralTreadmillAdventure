using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BarChartBar : MonoBehaviour
{
    public TextMeshPro textMesh;

    public Transform bar;

    public int countedValue;

    public int count;

    public GameObject textPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(1f,0f,1f) + Vector3.up * count * 1f;
        textMesh.transform.position = transform.position + Vector3.Distance(transform.position, bar.position) * Vector3.up * 2f + Vector3.up;
        textMesh.text = countedValue.ToString() + ": " + count;
    }

    public void CreateTextMesh()
    {
        textMesh = GameObject.Instantiate(textPrefab).GetComponent<TextMeshPro>();
    }
}
