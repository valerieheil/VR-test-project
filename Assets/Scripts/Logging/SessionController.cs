// ============================================================
//  SessionController.cs
//  A simple controller that lets you start and stop/export
//  a session by pressing a VR controller button.
//
//  HOW TO USE:
//    1. Create an empty GameObject, name it "SessionController"
//    2. Attach this script to it
//    3. During play:
//         - Press RIGHT controller Primary Button (A) to EXPORT data
//         - Press LEFT controller Primary Button (X) to start a NEW session
//    4. You can also call ExportAndEnd() from a UI button
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SessionController : MonoBehaviour
{
    [Header("Controller Buttons")]
    [Tooltip("Which button exports/ends the session (default: Right A button)")]
    public OVRInput.Button exportButton = OVRInput.Button.One;          // A button

    [Tooltip("Which button starts a new session (default: Left X button)")]
    public OVRInput.Button newSessionButton = OVRInput.Button.Three;    // X button

    [Header("Auto Export")]
    [Tooltip("Also export when this script is destroyed (scene change / app quit)")]
    public bool exportOnDestroy = true;

    [Header("In-Headset Notifications")]
    [Tooltip("Show toast notifications in the headset")]
    public bool showNotifications = true;

    [Tooltip("Duration the notification stays visible (seconds)")]
    public float notificationDuration = 2f;

    private TextMeshProUGUI _notificationText;
    private CanvasGroup _notificationCanvasGroup;

    private void Awake()
    {
        // Auto-create notification UI if it doesn't exist
        if (showNotifications)
            CreateNotificationUI();
    }

    private void Update()
    {
        // ── Export current session ───────────────────────────
        // Player presses A (right hand) to save the data
        if (OVRInput.GetDown(exportButton))
        {
            Debug.Log("[SessionController] Export button pressed.");
            ShowNotification("📤 Exporting data...");
            ExportAndEnd();
        }

        // ── Start new session ────────────────────────────────
        // Player presses X (left hand) to clear and restart
        if (OVRInput.GetDown(newSessionButton))
        {
            Debug.Log("[SessionController] New session button pressed.");
            ShowNotification("✨ New session started!");
            VRLogger.Instance.StartSession();
        }
    }

    private void OnDestroy()
    {
        if (exportOnDestroy && VRLogger.Instance != null && VRLogger.Instance.EventCount > 0)
            VRLogger.Instance.ExportData();
    }

    // ── Public methods (can be called from UI buttons too) ───

    /// <summary>Export data and stop the current session.</summary>
    public void ExportAndEnd()
    {
        if (VRLogger.Instance == null)
        {
            Debug.LogWarning("[SessionController] VRLogger not found in scene!");
            return;
        }
        VRLogger.Instance.ExportData();
    }

    /// <summary>Start a fresh new session (clears old data).</summary>
    public void StartNewSession()
    {
        VRLogger.Instance?.StartSession();
    }

    // ── Notification System ──────────────────────────────────

    private void CreateNotificationUI()
    {
        // Create Canvas as WorldSpace (für VR!)
        GameObject canvasGO = new GameObject("NotificationCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Get Main Camera and position canvas in front of it
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 camPos = mainCam.transform.position;
            Vector3 camForward = mainCam.transform.forward;
            canvasGO.transform.position = camPos + camForward * 0.5f + Vector3.down * 0.3f;
            canvasGO.transform.rotation = mainCam.transform.rotation;
        }

        // Setup WorldSpace Canvas
        RectTransform canvasRect = canvasGO.AddComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 150);
        
        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        // Create Panel for notification
        GameObject panelGO = new GameObject("NotificationPanel");
        panelGO.transform.SetParent(canvasGO.transform);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);

        panelGO.AddComponent<CanvasGroup>();
        _notificationCanvasGroup = panelGO.GetComponent<CanvasGroup>();
        _notificationCanvasGroup.alpha = 0;

        // Create Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panelGO.transform);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _notificationText = textGO.AddComponent<TextMeshProUGUI>();
        _notificationText.text = "";
        _notificationText.alignment = TextAlignmentOptions.Center;
        _notificationText.fontSize = 40;
        _notificationText.color = Color.white;
    }

    private void ShowNotification(string message)
    {
        if (!showNotifications || _notificationText == null)
            return;

        _notificationText.text = message;
        StopAllCoroutines();
        StartCoroutine(NotificationRoutine());
    }

    private IEnumerator NotificationRoutine()
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            _notificationCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.2f);
            yield return null;
        }
        _notificationCanvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(notificationDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            _notificationCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / 0.3f);
            yield return null;
        }
        _notificationCanvasGroup.alpha = 0f;
    }
}
