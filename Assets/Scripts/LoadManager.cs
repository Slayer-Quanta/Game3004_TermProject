using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject progressTextObject;
    [SerializeField] private GameObject loadingMessageObject;

    [Header("Messages")]
    [SerializeField]
    private string[] loadingMessages = new string[]
    {
        "Generating terrain...",
        "Placing trees...",
        "Creating chunks...",
        "Building world..."
    };

    private Component progressTextComponent;
    private Component loadingMessageComponent;
    private Coroutine messageCoroutine;

    private static LoadingScreen _instance;
    public static LoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoadingScreen>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (progressTextObject != null)
        {
            progressTextComponent = progressTextObject.GetComponent("TMPro.TMP_Text") ??
                                   progressTextObject.GetComponent<Text>();
        }

        if (loadingMessageObject != null)
        {
            loadingMessageComponent = loadingMessageObject.GetComponent("TMPro.TMP_Text") ??
                                     loadingMessageObject.GetComponent<Text>();
        }

        // Start with loading screen hidden
        HideLoadingScreen();
    }
    public void ShowLoadingScreen()
    {
        if (loadingCanvas == null) return;

        loadingCanvas.gameObject.SetActive(true);
        UpdateProgress(0f);

        // Stop any existing coroutine before starting a new one
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(CycleLoadingMessages());
    }

    public void HideLoadingScreen()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(false);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressTextComponent != null)
            SetTextValue(progressTextComponent, $"{Mathf.Round(progress * 100)}%");
    }

    private IEnumerator CycleLoadingMessages()
    {
        int index = 0;

        while (loadingCanvas != null && loadingCanvas.gameObject.activeSelf)
        {
            if (loadingMessageComponent != null && loadingMessages.Length > 0)
            {
                SetTextValue(loadingMessageComponent, loadingMessages[index]);
                index = (index + 1) % loadingMessages.Length;
            }

            yield return new WaitForSeconds(2f);
        }

        messageCoroutine = null;
    }

    // Helper method to set text on either TMP_Text or Text component
    private void SetTextValue(Component textComponent, string value)
    {
        if (textComponent == null) return;

        System.Type type = textComponent.GetType();
        type.GetProperty("text")?.SetValue(textComponent, value);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}