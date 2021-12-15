//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               BarChartBar.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              A script controlling a single bar in a bar chart
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A script controlling a single bar in a bar chart
/// </summary>
public class BarChartBar : MonoBehaviour
{
    #region Private Variables
    [SerializeField] 
    [Tooltip("The game object instantiated for the text to be displayed in")]
    private GameObject textPrefab;

    [SerializeField]
    [Tooltip("The value that the bar chart represents")]
    private int countedValue;
    [SerializeField]
    [Tooltip("The number of time the value the bar is counting has been counted")]
    private int count;

    [SerializeField]
    [Tooltip("The text the bar used to display its value")]
    private TextMeshPro textMesh;

    [SerializeField]
    [Tooltip("The transform the bar it self ")]
    private Transform bar;
    #endregion

    #region Public Properties
    /// <summary>
    /// The game object instantiated for the text to be displayed in
    /// </summary>
    public GameObject TextPrefab
    {
        get
        {
            return textPrefab;
        }
        set
        {
            textPrefab = value;
        }
    }

    /// <summary>
    /// The value that the bar chart represents
    /// </summary>
    public int CountedValue
    {
        get
        {
            return countedValue;
        }
        set
        {
            countedValue = value;
        }
    }
    /// <summary>
    /// The number of time the value the bar is counting has been counted
    /// </summary>
    public int Count
    {
        get
        {
            return count;
        }
        set
        {
            count = value;
        }
    }

    /// <summary>
    /// The text the bar used to display its value
    /// </summary>
    public TextMeshPro TextMesh
    {
        get
        {
            return textMesh;
        }
        set
        {
            textMesh = value;
        }
    }
    /// <summary>
    /// The transform the bar it self 
    /// </summary>
    public Transform Bar
    {
        get
        {
            return bar;
        }
        set
        {
            bar = value;
        }
    }
    #endregion

    #region Unity Methods
    void Update()
    {
        transform.localScale = new Vector3(1f,0f,1f) + Vector3.up * Count * 1f;
        TextMesh.transform.position = transform.position + Vector3.Distance(transform.position, Bar.position) * Vector3.up * 2f + Vector3.up;
        TextMesh.text = CountedValue.ToString() + ": " + Count;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates the game object with the text mesh and stores it text mesh component
    /// </summary>
    public void CreateTextMesh()
    {
        TextMesh = GameObject.Instantiate(TextPrefab).GetComponent<TextMeshPro>();
    }
    #endregion
}
