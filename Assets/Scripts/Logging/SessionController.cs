// ============================================================
//  SessionController.cs
//  Simple keyboard-only controller for testing on desktop.
//  Uses the new Unity Input System package.
//
//  HOW TO USE:
//    1. Create an empty GameObject, name it "SessionController"
//    2. Attach this script to it
//    3. During play:
//         - Press A to EXPORT data
//         - Press X to start a NEW session
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;

public class SessionController : MonoBehaviour
{
    [Header("Keyboard Controls")]
    public bool enableKeyboardExport = true;
    public bool enableKeyboardNewSession = true;

    [Header("Auto Export")]
    [Tooltip("Also export when this script is destroyed (scene change / app quit)")]
    public bool exportOnDestroy = true;

    private void Start()
    {
        Debug.Log("[SessionController] Started. Press A to export, X for new session.");

        if (VRLogger.Instance == null)
            Debug.LogWarning("[SessionController] VRLogger.Instance is null at Start. Be sure a VRLogger exists in the scene.");

        if (Keyboard.current == null)
            Debug.LogError("[SessionController] No keyboard detected by the new Input System!");
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (enableKeyboardExport && Keyboard.current.aKey.wasPressedThisFrame)
        {
            Debug.Log("[SessionController] Keyboard A pressed. Exporting data.");
            ExportAndEnd();
        }

        if (enableKeyboardNewSession && Keyboard.current.xKey.wasPressedThisFrame)
        {
            Debug.Log("[SessionController] Keyboard X pressed. Starting new session.");
            StartNewSession();
        }
    }

    private void OnDestroy()
    {
        if (exportOnDestroy && VRLogger.Instance != null && VRLogger.Instance.EventCount > 0)
            VRLogger.Instance.ExportData();
    }

    public void ExportAndEnd()
    {
        if (VRLogger.Instance == null)
        {
            Debug.LogWarning("[SessionController] VRLogger not found in scene!");
            return;
        }
        VRLogger.Instance.ExportData();
    }

    public void StartNewSession()
    {
        VRLogger.Instance?.StartSession();
    }
}