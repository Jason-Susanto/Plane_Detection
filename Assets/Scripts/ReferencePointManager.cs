using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(CloudAnchorManager))]
public class ReferencePointManager : Assets.Scripts.Core.Singleton<ReferencePointManager>
{
    [SerializeField]
    private TextMeshProUGUI debugLog;

    [SerializeField]
    private TextMeshProUGUI AnchorPointCount;

    [SerializeField]
    private GameObject anchorPrefab;

    private ARRaycastManager arRaycastManager;

    private ARAnchorManager arAnchorManager;

    private ARPlaneManager arPlaneManager;

    private List<ARAnchor> anchorPoints = new List<ARAnchor>();

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool canPlaceAnchor = true;

    private List<GameObject> anchorPrefabInstances = new List<GameObject>();

    [SerializeField]
    private GameObject placedPrefab;

    private Dictionary<ARAnchor, GameObject> anchorToPrefabMap = new Dictionary<ARAnchor, GameObject>();


    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        debugLog.text = "debug info";
        AnchorPointCount.text = "point count: 0";
    }

    // Update is called once per frame
    void Update()
    {
        //Check if a Touchscreen device is even available
        if (Touchscreen.current == null)
        {
            // This log might spam, consider removing or making it more specific
            // debugLog.text = "No touchscreen detected. Make sure Unity Remote or touch simulation is active.";
            return; // Exit Update if no touchscreen input source is available
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Touch hit UI, don't place reference point
            return;
        }

        var primaryTouch = Touchscreen.current.primaryTouch;
        var currentTouchPhase = primaryTouch.phase.ReadValue();

        // Check if the primary touch has just begun (the "Began" phase)
        if (currentTouchPhase !=UnityEngine.InputSystem.TouchPhase.Began && 
            currentTouchPhase != UnityEngine.InputSystem.TouchPhase.Ended)
        {
            Debug.Log("you must either be Began or Ended");
            Debug.Log($"1 {currentTouchPhase} {canPlaceAnchor}");
            return;
        }

        if(currentTouchPhase == UnityEngine.InputSystem.TouchPhase.Began && canPlaceAnchor == false)
        {
            Debug.Log("you must be able to place anchor");
            return;
        }

        if (currentTouchPhase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            canPlaceAnchor = true;
            Debug.Log($"2 {currentTouchPhase} {canPlaceAnchor}");
            return;
        }

        canPlaceAnchor = false;


        Debug.Log($"4 {currentTouchPhase} {canPlaceAnchor}");
        if(arRaycastManager.Raycast(primaryTouch.position.ReadValue(),hits,TrackableType.PlaneWithinPolygon))
        {
            Debug.Log($"5 {currentTouchPhase} {canPlaceAnchor}");
            Pose hitPose = hits[0].pose;
            ARAnchor anchorPoint = arAnchorManager.AddAnchor(hitPose);

            if(anchorPoint == null)
            {
                string errorEntry = "There was an error creating an anchor point\n";
                Debug.Log(errorEntry);
                debugLog.text += errorEntry;
            }
            else
            {
                GameObject prefabInstance = Instantiate(anchorPrefab, anchorPoint.transform);
                anchorToPrefabMap[anchorPoint] = prefabInstance;

                anchorPoints.Add(anchorPoint);
                Debug.Log($"6 {currentTouchPhase} {canPlaceAnchor}");

                CloudAnchorManager.Instance.QueueAnchor(anchorPoint);
                CloudAnchorManager.Instance.HostAnchor();

                AnchorPointCount.text = $"Reference Point Count: {anchorPoints.Count}";
            }
        }
    }

    //this technically no need; function after can do this function's job as well
    public void RecreateAnchor(Transform transform)
    {
        GameObject placedGameObject = Instantiate(placedPrefab, transform.position, transform.rotation);
        placedGameObject.transform.parent = transform;

        anchorPrefabInstances.Add(placedGameObject);

    }

    public void LoadAndDisplayAllAnchors()
    {

        if (anchorPrefabInstances.Count > 1)
        {
            foreach (var anchorObject in anchorPrefabInstances)
            {
                if (anchorObject != null)
                {
                    Destroy(anchorObject);
                }
            }
            anchorPrefabInstances.Clear();
        }


        // Get the list of all saved anchors from local storage.
        LocalCloudAnchorList allAnchors = CloudAnchorPersistenceManager.Instance.LoadAllAnchors();

        // Instantiate a prefab for each anchor at its saved position.
        foreach (var anchorData in allAnchors.anchors)
        {
            if (anchorPrefabInstances[0].transform.position == anchorData.position && anchorPrefabInstances[0].transform.rotation == anchorData.rotation)
            {
                continue;
            }
            // Instantiate the prefab at the stored position and rotation.
            GameObject anchorObject = Instantiate(anchorPrefab, anchorData.position, anchorData.rotation);
            anchorPrefabInstances.Add(anchorObject);

            Debug.Log($"Instantiated anchor: {anchorData.cloudAnchorId} at position: {anchorObject.transform.position}");
        }
    }

    public GameObject GetPrefabAssociation(ARAnchor anchor)
    {
        if (anchorToPrefabMap.ContainsKey(anchor))
        {
            return anchorToPrefabMap[anchor];
        }
        return null;
    }
}
