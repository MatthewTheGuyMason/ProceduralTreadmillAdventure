using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBarChart : MonoBehaviour
{
    private List<BarChartBar> barChartBars;

    public GameObject barsPrefab;

    public int numberRange;

    public Camera sceneCamera;

    // Start is called before the first frame update
    void Start()
    {
        barChartBars = new List<BarChartBar>();
        PseudoRandomNumberGenerator.seed = 78914072347890;
    }

    // Update is called once per frame
    void Update()
    {
        int value = System.Convert.ToInt32( PseudoRandomNumberGenerator.XorShiftStarInt() % numberRange);

        int index = -1;
        for (int i = 0; i < barChartBars.Count; ++i)
        {
            if (value == barChartBars[i].countedValue)
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            CreateBar(value);
        }
        else
        {
            ++barChartBars[index].count;
        }
    }

    private void CreateBar(int countedValue)
    {
        barChartBars.Add(GameObject.Instantiate(barsPrefab).GetComponent<BarChartBar>());
        barChartBars[barChartBars.Count - 1].countedValue = countedValue;
        ++barChartBars[barChartBars.Count - 1].count; 
        barChartBars[barChartBars.Count - 1].CreateTextMesh();
        barChartBars[barChartBars.Count - 1].textMesh.text = countedValue.ToString();

        List<BarChartBar> sortedBarCharts = new List<BarChartBar>(barChartBars.Count);

        while (barChartBars.Count > 0)
        {
            int lowestValue = int.MaxValue;
            int lowestValueIndex = -1;
            for (int i = 0; i < barChartBars.Count; ++i)
            {
                if (lowestValue > barChartBars[i].countedValue)
                {
                    lowestValue = barChartBars[i].countedValue;
                    lowestValueIndex = i;
                }
            }
            sortedBarCharts.Add(barChartBars[lowestValueIndex]);
            barChartBars.RemoveAt(lowestValueIndex);
        }
        barChartBars = sortedBarCharts;

        for (int i = 0; i < barChartBars.Count; ++i)
        {
            barChartBars[i].transform.position = Vector3.right * i * 4f;
        }
    }
}
