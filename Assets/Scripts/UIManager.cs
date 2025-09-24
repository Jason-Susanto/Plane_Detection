

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager: MonoBehaviour
{
    [SerializeField]
    private Button resolveButton;

    private TextMeshProUGUI buttonText;

    [SerializeField]
    private Button showGraphButton;

    private TextMeshProUGUI showGraphButtonText;

    [SerializeField]
    private GameObject toggleOption;

    [SerializeField]
    private GameObject toggleGroup;

    private List<Toggle> toggleOptions = new List<Toggle>();

    [SerializeField]
    private Canvas ui;

    private string idToResolve;

    private GameObject instantiatedToggleGroupContainer;

    private ToggleGroup instantiatedToggleGroupComponent;

    private bool toggleGroupOn = false;

    private void Awake()
    {
        instantiatedToggleGroupContainer = Instantiate(toggleGroup, ui.transform);

        instantiatedToggleGroupComponent = instantiatedToggleGroupContainer.GetComponent<ToggleGroup>();
        instantiatedToggleGroupComponent.allowSwitchOff = true;

        // Initially hide the instantiated toggle group container
        instantiatedToggleGroupContainer.SetActive(false);

        resolveButton.onClick.RemoveAllListeners();
        buttonText = resolveButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "Show Anchors";

        resolveButton.interactable = true;
        resolveButton.onClick.AddListener(OnResolveButtonClick);


        showGraphButton.onClick.RemoveAllListeners();
        showGraphButtonText = showGraphButton.GetComponentInChildren<TextMeshProUGUI>();
        showGraphButtonText.text = "Show Graph";

        showGraphButton.interactable = true;
        showGraphButton.onClick.AddListener(OnShowGraphButtonClick);



    }

    void OnResolveButtonClick()
    {
        ARDebugManager.Instance.LogInfo("Buton Clicked, " + buttonText.text);

        if (buttonText.text == "Show Anchors" && toggleGroupOn == false)
        {
            if (toggleOptions.Count != 0)
            {
                foreach (Toggle toggle in toggleOptions)
                {
                    Destroy(toggle.gameObject);
                }
                toggleOptions.Clear();
            }

            ARDebugManager.Instance.LogInfo("getting anchirs");

            List<string> cloudAnchors = CloudAnchorManager.Instance.GetCloudAnchorList();
            //float startY = (cloudAnchors.Count - 1) * 0.5f * 100f;

            ARDebugManager.Instance.LogInfo("getting getting id's");
            ARDebugManager.Instance.LogInfo($"{cloudAnchors}");

            foreach (string id in cloudAnchors)
            {
                GameObject toggleInstance = Instantiate(toggleOption, instantiatedToggleGroupContainer.transform);
                Toggle toggle = toggleInstance.GetComponent<Toggle>();

                TextMeshProUGUI toggleText = toggleInstance.GetComponentInChildren<TextMeshProUGUI>();
                toggleText.text = id;

                RectTransform toggleRect = toggleInstance.GetComponent<RectTransform>();

                //// The sizeDelta, anchor, pivot should already be set correctly on the prefab itself
                //toggleRect.anchoredPosition = new Vector2(0, startY);
                //toggleRect.sizeDelta = new Vector2(500f, 50f);


                ARDebugManager.Instance.LogInfo($"{toggle.isOn}");
                toggle.isOn = false;
                ARDebugManager.Instance.LogInfo($"{toggle.isOn}");


                toggle.group = instantiatedToggleGroupComponent;

                toggle.onValueChanged.AddListener((isOn) => OptionSelected(toggle));


                toggleOptions.Add(toggle);

                //startY -= 100f;

            }
            ARDebugManager.Instance.LogInfo("show toggles");

            instantiatedToggleGroupContainer.SetActive(true);
            toggleGroupOn = true;
        }
        else if (buttonText.text == "Resolve Anchor" && toggleGroupOn == true)
        {
            ARDebugManager.Instance.LogInfo("reslve anchor");
            if(idToResolve == null)
            {
                ARDebugManager.Instance.LogInfo($"id is null");
                return;
            }
            CloudAnchorManager.Instance.Resolve(idToResolve);
            instantiatedToggleGroupContainer.SetActive(false);
            buttonText.text = "Show Anchors";
            toggleGroupOn = true;
        }
        else if(toggleGroupOn == true)
        {
            buttonText.text = "Show Anchors";
            instantiatedToggleGroupContainer.SetActive(false);
            toggleGroupOn = false;
        }



    }

    void OptionSelected(Toggle toggle)
    {
        if (toggle.isOn)
        {
            ARDebugManager.Instance.LogInfo("option selected");

            buttonText.text = "Resolve Anchor";

            TextMeshProUGUI text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            idToResolve = text.text;
        }
        else
        {
            buttonText.text = "Show Anchor";
            idToResolve = null;
        }
    }

    void OnShowGraphButtonClick()
    {
        ARDebugManager.Instance.LogInfo("Buton Clicked, " + showGraphButtonText.text);

        RectTransform panel = GraphManager.Instance.graphParent.GetComponent<RectTransform>();

        panel.gameObject.SetActive(!panel.gameObject.activeSelf);

    }


}