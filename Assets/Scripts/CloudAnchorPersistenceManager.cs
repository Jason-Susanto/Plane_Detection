using Google.XR.ARCoreExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation; // Make sure this is imported for ARCloudAnchor

// A serializable class to store the necessary anchor data.
// It holds the cloud anchor ID, a user-friendly name, and its transform.
[Serializable]
public class LocalCloudAnchor
{
    public string cloudAnchorId;
    public Vector3 position;
    public Quaternion rotation;
}


public class LocalCloudAnchorList
{
    public List<LocalCloudAnchor> anchors;

    public LocalCloudAnchorList()
    {
        anchors = new List<LocalCloudAnchor>();
       
    }

}
public class CloudAnchorPersistenceManager : Assets.Scripts.Core.Singleton<CloudAnchorPersistenceManager>
{

    private const string KEY = "map";

    public void SaveAnchorData(ARCloudAnchor arCloudAnchor)
    {
        if (arCloudAnchor == null)
        {
            ARDebugManager.Instance.LogError("ARCloudAnchor is Null");
            return;
        }

        // Create a new data object for the current anchor.
        LocalCloudAnchor newAnchorData = new LocalCloudAnchor
        {
            cloudAnchorId = arCloudAnchor.cloudAnchorId,
            position = arCloudAnchor.transform.position,
            rotation = arCloudAnchor.transform.rotation
        };


        // Load any existing anchor data.
        LocalCloudAnchorList allAnchors = LoadAllAnchors();

        // Add the new anchor's data to the list.
        allAnchors.anchors.Add(newAnchorData);

        // Serialize the entire list to a JSON string.
        string json = JsonUtility.ToJson(allAnchors);

        // Save the JSON string to PlayerPrefs which is like a local persistent storage.
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save(); // Ensure data is written to disk immediately

        ARDebugManager.Instance.LogInfo($"Successfully saved anchor data with ID: {newAnchorData.cloudAnchorId}");
    }

    public LocalCloudAnchorList LoadAllAnchors()
    {
        if (PlayerPrefs.HasKey(KEY))
        {
            // Retrieve the JSON string from PlayerPrefs.
            string json = PlayerPrefs.GetString(KEY);

            // Deserialize the JSON string back into the C# object.
            try
            {
                LocalCloudAnchorList data = JsonUtility.FromJson<LocalCloudAnchorList>(json);
                ARDebugManager.Instance.LogInfo($"Loaded {data.anchors.Count} cloud anchors from local storage.");
                return data;
            }
            catch (Exception e)
            {
                ARDebugManager.Instance.LogError($"Failed to deserialize anchor data: {e.Message}");
                // Return a new, empty list on failure to avoid null reference exceptions.
                return new LocalCloudAnchorList();
            }
        }
        else
        {
            ARDebugManager.Instance.LogError("No cloud anchor data found in local storage.");
            return new LocalCloudAnchorList(); // Return a new, empty list.
        }
    }

}
