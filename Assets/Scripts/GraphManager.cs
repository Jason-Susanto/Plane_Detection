using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation.VisualScripting;

public class GraphManager : Assets.Scripts.Core.Singleton<GraphManager>
{
    private List<Vector3> anchors;

    [SerializeField]
    public RectTransform graphParent;

    [SerializeField]
    public GameObject pointPrefab; // A UI Image Prefab

    private void Start()
    {
        var list = CloudAnchorPersistenceManager.Instance.LoadAllAnchors().anchors;
        graphParent.gameObject.SetActive(false);

        if (list.Count == 0) return;

        foreach (var data in list)
        {
            anchors.Add(data.position);
        } 
        DrawGraph();
    }

    private void DrawGraph()
    {
        // Step 1 & 2: Find min/max and normalize positions
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (Vector3 anchor in anchors)
        {
            minX = Mathf.Min(minX, anchor.x);
            maxX = Mathf.Max(maxX, anchor.x);
            minZ = Mathf.Min(minZ, anchor.z);
            maxZ = Mathf.Max(maxZ, anchor.z);
        }

        // Adjust for non-zero origin
        float rangeX = maxX - minX;
        float rangeZ = maxZ - minZ;

        // Handle cases where all anchors are on the same line
        if (rangeX == 0) rangeX = 1;
        if (rangeZ == 0) rangeZ = 1;

        RectTransform graphRect = graphParent.GetComponent<RectTransform>();
        float graphWidth = graphRect.rect.width;
        float graphHeight = graphRect.rect.height;

        // Step 3: Map normalized positions to UI and instantiate points
        foreach (Vector3 anchor in anchors)
        {
            float normalizedX = (anchor.x - minX) / rangeX;
            float normalizedZ = (anchor.z - minZ) / rangeZ;

            float uiX = normalizedX * graphWidth - (graphWidth / 2f);
            float uiY = normalizedZ * graphHeight - (graphHeight / 2f);

            GameObject point = Instantiate(pointPrefab, graphParent);
            point.GetComponent<RectTransform>().anchoredPosition = new Vector2(uiX, uiY);
        }
    }
}