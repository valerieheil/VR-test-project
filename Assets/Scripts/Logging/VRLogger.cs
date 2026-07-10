// ============================================================
//  VRLogger.cs
//  The central logging manager (Singleton).
//
//  HOW TO USE:
//    1. Create an empty GameObject in your scene, name it "VRLogger"
//    2. Drag this script onto it
//    3. From any tracker script, call:
//         VRLogger.Instance.Log(new LogEvent(...))
//    4. At the end of a session call:
//         VRLogger.Instance.ExportData()
//       OR enable "autoExportOnQuit" in the Inspector
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class VRLogger : MonoBehaviour
{
   // ── Singleton setup ──────────────────────────────────────
   // Only one VRLogger can exist. Any script can reach it via
   // VRLogger.Instance without needing a reference.
   public static VRLogger Instance { get; private set; }

   // ── Inspector settings ───────────────────────────────────
   [Header("Session Settings")]
   [Tooltip("Unique ID for this session. Auto-generated if left empty.")]
   public string sessionID = "";

   [Header("Export Settings")]
   [Tooltip("Folder where JSON and CSV files will be saved.")]
   public string exportFolder = "VRLogs";          // Relative to Application.persistentDataPath

   [Tooltip("Export JSON file at end of session")]
   public bool exportJSON = true;

   [Tooltip("Export CSV file at end of session")]
   public bool exportCSV  = true;

   [Tooltip("Also export separate pre-filtered CSVs (Gaze.csv, Movement.csv, " +
            "Interaction.csv) alongside the full combined CSV, so you don't " +
            "have to filter in Excel if you just want one type of data.")]
   public bool exportFilteredByCategory = true;

   [Tooltip("Automatically export when the app quits or scene ends")]
   public bool autoExportOnQuit = true;

   // ── Internal state ───────────────────────────────────────
   private List<LogEvent> _eventBuffer = new List<LogEvent>(); // All events in memory
   private float          _sessionStart;                        // Time.time when session began
   private bool           _isRunning = false;

   // CSV column headers — must match LogEvent.ToCSVRow() order
   // v2: grabCount / isTrigger / speed / totalDistance are now real
   // columns (blank when not applicable) instead of being packed
   // into "extra". Use semicolons so Excel in German locales parses
   // the file into separate columns by default.
   private const char CSV_SEPARATOR = ';';
   private const string CSV_HEADER =
       //"eventType;objectID;timestamp;posX;posY;posZ;duration;grabCount;isTrigger;speed;totalDistance;extra";
        "eventType;objectID;timestamp;posX;posY;posZ;duration;extra";
   private const string CSV_BOM = "\uFEFF";

   // ── Filter categories ─────────────────────────────────────
   // Maps a friendly file name to the eventType values it includes.
   // Edit these lists if you rename an eventType or add a new tracker.
   private static readonly Dictionary<string, string[]> FilterCategories =
       new Dictionary<string, string[]>
   {
       { "Gaze",        new[] { "GazeEnter", "GazeExit" } },
       { "Movement",    new[] { "HeadMove", "LeftHandPos", "RightHandPos" } },
       { "Interaction", new[] { "Grab", "Release" } },
   };

   // ── Unity lifecycle ──────────────────────────────────────

   private void Awake()
   {
       // Singleton pattern: destroy any duplicate
       if (Instance != null && Instance != this)
       {
           Destroy(gameObject);
           return;
       }
       Instance = this;
       DontDestroyOnLoad(gameObject); // Survives scene changes

       StartSession();
   }

   private void OnApplicationQuit()
   {
       if (autoExportOnQuit)
           ExportData();
   }

   // ── Public API ───────────────────────────────────────────

   /// <summary>
   /// Call this to log any event. Thread-safe for the main thread.
   /// Example: VRLogger.Instance.Log(new LogEvent("Grab", "Bottle", ...))
   /// </summary>
   public void Log(LogEvent evt)
   {
       if (!_isRunning) return;
       _eventBuffer.Add(evt);

       // Optional: print to Unity console while testing
       Debug.Log($"[VRLogger] {evt.timestamp:F2}s | {evt.eventType} | {evt.objectID}");
   }

   /// <summary>
   /// Starts (or restarts) a new session and clears the buffer.
   /// </summary>
   public void StartSession()
   {
       _eventBuffer.Clear();
       _sessionStart = Time.time;
       _isRunning    = true;

       // Always generate a new session ID for each new session
       sessionID = "Session_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

       Debug.Log($"[VRLogger] Session started: {sessionID}");
        // Helpful debug output showing where files will be written on device
        Debug.Log($"[VRLogger] persistentDataPath: {Application.persistentDataPath}");
        Debug.Log($"[VRLogger] Export folder path: {Path.Combine(Application.persistentDataPath, exportFolder, sessionID)}");
   }

   /// <summary>
   /// Stops logging and exports all collected data to disk.
   /// </summary>
   public void ExportData()
   {
       _isRunning = false;
       Debug.Log($"[VRLogger] Exporting {_eventBuffer.Count} events for session {sessionID}");

       // Build the export folder path
       string folder = Path.Combine(Application.persistentDataPath, exportFolder, sessionID);
       Directory.CreateDirectory(folder); // Creates all missing folders

       if (exportJSON) WriteJSON(folder);
       if (exportCSV)  WriteCSV(folder);
       if (exportCSV && exportFilteredByCategory) WriteFilteredCSVs(folder);

       Debug.Log($"[VRLogger] Data saved to: {folder}");
   }

   /// <summary>
   /// Returns elapsed session time in seconds (use this for timestamps).
   /// </summary>
   public float SessionTime => Time.time - _sessionStart;

   /// <summary>
   /// How many events are currently in the buffer.
   /// </summary>
   public int EventCount => _eventBuffer.Count;

   // ── Private helpers ──────────────────────────────────────

   private void WriteJSON(string folder)
   {
       // Build a wrapper object so the JSON has session metadata at the top
       var export = new SessionExport
       {
           sessionID    = this.sessionID,
           exportedAt   = DateTime.Now.ToString("o"),   // ISO 8601
           totalEvents  = _eventBuffer.Count,
           durationSec  = SessionTime,
           events       = _eventBuffer
       };

       string json = JsonUtility.ToJson(export, prettyPrint: true);
       string path = Path.Combine(folder, $"{sessionID}.json");
       File.WriteAllText(path, json, Encoding.UTF8);
       Debug.Log($"[VRLogger] JSON written: {path}");
   }

   private void WriteCSV(string folder)
   {
       string path = Path.Combine(folder, $"{sessionID}.csv");
       File.WriteAllText(path, CSV_BOM + BuildCSVContent(_eventBuffer), Encoding.UTF8);
       Debug.Log($"[VRLogger] CSV written: {path}");
   }

   /// <summary>
   /// Writes one CSV per category defined in FilterCategories
   /// (currently: Gaze.csv, Movement.csv, Interaction.csv), each
   /// containing only the rows whose eventType matches that category.
   /// Runs automatically after ExportData() if exportFilteredByCategory
   /// is enabled. Categories with zero matching events are skipped.
   /// </summary>
   private void WriteFilteredCSVs(string folder)
   {
       foreach (var category in FilterCategories)
       {
           string categoryName   = category.Key;
           string[] eventTypes   = category.Value;

           var matching = _eventBuffer.FindAll(evt => Array.IndexOf(eventTypes, evt.eventType) >= 0);
           if (matching.Count == 0) continue; // nothing to write for this category

           string path = Path.Combine(folder, $"{sessionID}_{categoryName}.csv");
           File.WriteAllText(path, CSV_BOM + BuildCSVContent(matching), Encoding.UTF8);
           Debug.Log($"[VRLogger] Filtered CSV written: {path} ({matching.Count} rows)");
       }
   }

   /// <summary>
   /// Call this yourself (e.g. from a UI button or SessionController) to
   /// export an ad-hoc filtered CSV for any set of eventTypes, beyond the
   /// fixed categories above. Example:
   ///   VRLogger.Instance.ExportFiltered(new[] { "GazeExit" }, "GazeOnly");
   /// Writes into the same session folder as the main export.
   /// </summary>
   public void ExportFiltered(string[] eventTypes, string label)
   {
       var matching = _eventBuffer.FindAll(evt => Array.IndexOf(eventTypes, evt.eventType) >= 0);

       string folder = Path.Combine(Application.persistentDataPath, exportFolder, sessionID);
       Directory.CreateDirectory(folder);

       string path = Path.Combine(folder, $"{sessionID}_{label}.csv");
       File.WriteAllText(path, BuildCSVContent(matching), Encoding.UTF8);
       Debug.Log($"[VRLogger] Ad-hoc filtered CSV written: {path} ({matching.Count} rows)");
   }

   /// <summary>
   /// Builds the full CSV text (metadata header + column header + rows)
   /// for a given list of events. Shared by WriteCSV, WriteFilteredCSVs,
   /// and ExportFiltered so all three always stay in the same format.
   /// </summary>
   private string BuildCSVContent(List<LogEvent> events)
   {
       var sb = new StringBuilder();

       // Session metadata rows at the top (easy to see in Excel)
       sb.AppendLine($"# SessionID{CSV_SEPARATOR}{sessionID}");
       sb.AppendLine($"# ExportedAt{CSV_SEPARATOR}{DateTime.Now:o}");
       sb.AppendLine($"# TotalEvents{CSV_SEPARATOR}{events.Count}");
       sb.AppendLine($"# DurationSec{CSV_SEPARATOR}{SessionTime:F2}");
       sb.AppendLine(); // blank line before data

       // Column headers
       sb.AppendLine(CSV_HEADER);

       // One row per event
       foreach (var evt in events)
           sb.AppendLine(evt.ToCSVRow());

       return sb.ToString();
   }

   // ── JSON wrapper class ───────────────────────────────────
   [System.Serializable]
   private class SessionExport
   {
       public string          sessionID;
       public string          exportedAt;
       public int             totalEvents;
       public float           durationSec;
       public List<LogEvent>  events;
   }
}