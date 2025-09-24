//basis from https://github.com/dilmerv/ARCloudAnchors/blob/master/Assets/Scripts/Managers/ARCloudAnchorManager.cs
using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using Assets.Scripts.Core;

public class AnchorCreatedEvent : UnityEvent<Transform> {}
public class CloudAnchorManager : Assets.Scripts.Core.Singleton<CloudAnchorManager>
{

    [SerializeField]

    private Camera arCamera = null;

    [SerializeField]
    private float resolveAnchorPassedTimeout = 0.5f;

    private ARAnchorManager arAnchorManager = null;

    private ARAnchor pendingHostAnchor = null;

    private ARCloudAnchor cloudAnchor = null;

    private List<string> cloudAnchorList = new List<string>();

    private string anchorIdToResolve;

    private bool anchorHostInProgress = false;

    private bool anchorResolveInProgress = false;

    private float safeToResolvePassed = 0;

    private bool checkingForMapQuality = false;

    private float lastQualityCheck = 0f;
    private float qualityCheckInterval = 2.0f;

    private AnchorCreatedEvent cloudAnchorCreatedEvent = null; 

    private void Awake()
    {
        cloudAnchorCreatedEvent = new AnchorCreatedEvent();
        cloudAnchorCreatedEvent.AddListener((t) => ReferencePointManager.Instance.RecreateAnchor(t));
        arAnchorManager = GetComponent<ARAnchorManager>();
    }

    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position, arCamera.transform.rotation);
    }

    #region Anchor Cycle

    public List<string> GetCloudAnchorList()
    {
        return cloudAnchorList;
    }


    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor = arAnchor;
    }

    public void HostAnchor()
    {
        ARDebugManager.Instance.LogInfo("HostAnchor call in progress");
        ARDebugManager.Instance.LogInfo("Checking map quality");

        checkingForMapQuality = true;
    }

    private void CheckMapQuality()
    {
        ARDebugManager.Instance.LogInfo("HostAnchor call in progress");

        //reccomendded up to 30 seconds of scanning before calling host anchor
        FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        ARDebugManager.Instance.LogInfo($"Feature Map Quality is:{quality}");

        if (quality == FeatureMapQuality.Good)
        {
            checkingForMapQuality = false;
            ActualHosting();
        }
    }

    private void ActualHosting()
    {
        cloudAnchor = arAnchorManager.HostCloudAnchor(pendingHostAnchor,1);

        if(cloudAnchor == null)
        {
            ARDebugManager.Instance.LogError("Unable to host cloud anchor");
        }
        else
        {
            anchorHostInProgress = true;
        }
    }
        

    public void Resolve(string id)
    {
        ARDebugManager.Instance.LogInfo("Resolve call in progress");
        
        if(id != null) { anchorIdToResolve = id; }

        cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorIdToResolve);

        if (cloudAnchor == null)
        {
            ARDebugManager.Instance.LogError($"Unable to host cloud anchor {anchorIdToResolve}");

        }
        else
        {
            anchorResolveInProgress = true;
        }
    }

    private void CheckHostingProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;

        if(cloudAnchorState == CloudAnchorState.Success)
        {
            anchorHostInProgress = false;
            ARDebugManager.Instance.LogInfo($"cloud anchor hosting successful");

            pendingHostAnchor.gameObject.SetActive(false);
            GameObject prefab = ReferencePointManager.Instance.GetPrefabAssociation(pendingHostAnchor);
            if (prefab != null)  // Add this null check
            {
                prefab.SetActive(false);
            }
            prefab.SetActive(false);

            cloudAnchorList.Add(cloudAnchor.cloudAnchorId);
            CloudAnchorPersistenceManager.Instance.SaveAnchorData(cloudAnchor);


        }
        else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
           ARDebugManager.Instance.LogError($"cloud anchor hosting in progress");
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            ARDebugManager.Instance.LogError($"Error while hosting cloud anchor: {cloudAnchorState}");
            anchorHostInProgress = false;
        }

    }

    private void CheckResolveProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;

        if (cloudAnchorState == CloudAnchorState.Success)
        {
            anchorResolveInProgress = false;
            ARDebugManager.Instance.LogInfo("Resolving Successful");
            cloudAnchorCreatedEvent?.Invoke(cloudAnchor.transform);
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            ARDebugManager.Instance.LogError($"Error while resolving cloud anchor: {cloudAnchorState}");
            anchorResolveInProgress = false;
        }
    }

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

#endregion

    // Update is called once per frame
    void Update()
    {
        if (checkingForMapQuality)
        {
            if (Time.time - lastQualityCheck >= qualityCheckInterval)
            {
                lastQualityCheck = Time.time;
                CheckMapQuality();
            }
            return;
        }

        //check for host result
        if(anchorHostInProgress)
        {
            CheckHostingProgress();
            return;
        }

        //checked for resolve result
        if(anchorResolveInProgress && safeToResolvePassed <= 0)
        {
            safeToResolvePassed = resolveAnchorPassedTimeout;
            if(!string.IsNullOrEmpty(anchorIdToResolve))
            {
                ARDebugManager.Instance.LogInfo($"Resolving Anchor Id: {anchorIdToResolve}");
                CheckResolveProgress();

            }

        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }


    }
}
