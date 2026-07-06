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

    private void Update()
    {
        // ── Export current session ───────────────────────────
        // Player presses A (right hand) to save the data
        if (OVRInput.GetDown(exportButton))
        {
            Debug.Log("[SessionController] Export button pressed.");
            ExportAndEnd();
        }

        // ── Start new session ────────────────────────────────
        // Player presses X (left hand) to clear and restart
        if (OVRInput.GetDown(newSessionButton))
        {
            Debug.Log("[SessionController] New session button pressed.");
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
}
