//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               RandomBarChart.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              Script for controlling a bar graph for showing the distribution of random numbers
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for controlling a bar graph for showing the distribution of random numbers
/// </summary>
public class RandomBarChart : MonoBehaviour
{
    #region Public Variables
    [SerializeField] 
    [Tooltip("The main camera used in the scene")]
    private Camera sceneCamera;

    [SerializeField]
    [Tooltip("The range of number of display on the bar chart ")]
    private int numberRange;

    [SerializeField]
    [Tooltip("The prefab used for the bars as they are shown in unity")]
    private GameObject barsPrefab;
    #endregion

    #region Public Properties
    /// <summary>
    /// The main camera used in the scene
    /// </summary>
    public Camera SceneCamera
    {
        get
        {
            return sceneCamera;
        }
        set
        {
            sceneCamera = value;
        }
    }

    /// <summary>
    /// The range of number of display on the bar chart 
    /// </summary>
    public int NumberRange
    {
        get
        {
            return numberRange;
        }
        set
        {
            numberRange = value;
        }
    }

    /// <summary>
    /// The prefab used for the bars as they are shown in unity
    /// </summary>
    public GameObject BarsPrefab
    {
        get
        {
            return barsPrefab;
        }
        set
        {
            barsPrefab = value;
        }
    }

    #endregion

    #region Private Variables
    /// <summary>
    /// The bars used in the bar chart
    /// </summary>
    private List<BarChartBar> barChartBars;
    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        barChartBars = new List<BarChartBar>();
        XorPseudoRandomNumberGenerator.seed = 78914072347890;
    }
    // Update is called once per frame
    private void Update()
    {
        // Get the random value
        int value = System.Convert.ToInt32( XorPseudoRandomNumberGenerator.XorShiftStarInt() % NumberRange);

        // Find the value and add to the bar, adding a new bar if none was found
        int index = -1;
        for (int i = 0; i < barChartBars.Count; ++i)
        {
            if (value == barChartBars[i].CountedValue)
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
            ++barChartBars[index].Count;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Creates a bar and add it to the bar chart
    /// </summary>
    /// <param name="countedValue">The value to set the bar at in the bar chart</param>
    private void CreateBar(int countedValue)
    {
        // Create the new bar
        barChartBars.Add(GameObject.Instantiate(BarsPrefab).GetComponent<BarChartBar>());
        barChartBars[barChartBars.Count - 1].CountedValue = countedValue;
        ++barChartBars[barChartBars.Count - 1].Count; 
        barChartBars[barChartBars.Count - 1].CreateTextMesh();
        barChartBars[barChartBars.Count - 1].TextMesh.text = countedValue.ToString();

        // Sort the bar to ascending numeric value
        List<BarChartBar> sortedBarCharts = new List<BarChartBar>(barChartBars.Count);
        while (barChartBars.Count > 0)
        {
            int lowestValue = int.MaxValue;
            int lowestValueIndex = -1;
            for (int i = 0; i < barChartBars.Count; ++i)
            {
                if (lowestValue > barChartBars[i].CountedValue)
                {
                    lowestValue = barChartBars[i].CountedValue;
                    lowestValueIndex = i;
                }
            }
            sortedBarCharts.Add(barChartBars[lowestValueIndex]);
            barChartBars.RemoveAt(lowestValueIndex);
        }
        barChartBars = sortedBarCharts;

        // Move them all into position
        for (int i = 0; i < barChartBars.Count; ++i)
        {
            barChartBars[i].transform.position = Vector3.right * i * 4f;
        }
    }
    #endregion
}
