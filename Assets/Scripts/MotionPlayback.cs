using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Mapping parameters for a single servo: relationship between Arduino angles and simulation angles.
/// Use with ArduinoSimAngleMapping.ArduinoToSim and SimToArduino.
/// </summary>
[Serializable]
public struct ArduinoSimAngleMapping
{
    /// <summary>Simulation angle (degrees) that corresponds to Arduino 0°.</summary>
    public float zeroOffsetDegrees;

    /// <summary>+1 if increasing Arduino angle increases simulation angle; -1 if it decreases it.</summary>
    public int direction;

    /// <summary>Optional Arduino lower limit (degrees). Use float.NaN for no limit.</summary>
    public float arduinoMinDegrees;

    /// <summary>Optional Arduino upper limit (degrees). Use float.NaN for no limit.</summary>
    public float arduinoMaxDegrees;

    /// <summary>Convert Arduino angle to simulation angle.</summary>
    public static float ArduinoToSim(ArduinoSimAngleMapping m, float arduinoAngleDegrees)
    {
        return m.zeroOffsetDegrees + m.direction * arduinoAngleDegrees;
    }

    /// <summary>Convert simulation angle to Arduino angle. Clamps to arduinoMinDegrees/arduinoMaxDegrees when set (non-NaN).</summary>
    public static float SimToArduino(ArduinoSimAngleMapping m, float simAngleDegrees)
    {
        float arduino = m.direction * (simAngleDegrees - m.zeroOffsetDegrees);
        if (!float.IsNaN(m.arduinoMinDegrees))
            arduino = Mathf.Max(arduino, m.arduinoMinDegrees);
        if (!float.IsNaN(m.arduinoMaxDegrees))
            arduino = Mathf.Min(arduino, m.arduinoMaxDegrees);
        return arduino;
    }
}

/// <summary>
/// Plays back a motion from a CSV file by setting target angles (in degrees) on PID controllers
/// and waiting the specified time between keyframes. Expects header: leftFemur, rightFemur, leftFoot, rightFoot, leftKnee, rightKnee, wait
/// with angles in degrees and wait in milliseconds.
/// </summary>
public class MotionPlayback : MonoBehaviour
{
    [Header("CSV")]
    [Tooltip("Filename under StreamingAssets (e.g. Motion.csv).")]
    public string csvFilename = "Motion.csv";

    [Header("PID controllers (order: leftFemur, rightFemur, leftFoot, rightFoot, leftKnee, rightKnee)")]
    public JointPidController[] pidControllers = new JointPidController[6];

    [Header("Playback")]
    [Tooltip("Start playback automatically when the scene runs.")]
    public bool playOnStart = false;
    [Tooltip("Use realtime for wait so timing is independent of Time.timeScale.")]
    public bool useRealtimeWait = true;

    const string WaitColumnName = "wait";
    static readonly string[] ExpectedJointNames = { "leftFemur", "rightFemur", "leftFoot", "rightFoot", "leftKnee", "rightKnee" };

    List<float[]> _keyframes;
    int[] _columnIndices;
    bool _playing;

    void Start()
    {
        if (playOnStart)
            StartPlayback();
    }

    /// <summary>
    /// Load CSV from StreamingAssets and start playback from the first keyframe.
    /// </summary>
    public void StartPlayback()
    {
        if (pidControllers == null || pidControllers.Length != 6)
        {
            Debug.LogError("MotionPlayback: Assign exactly 6 JointPidController references (leftFemur, rightFemur, leftFoot, rightFoot, leftKnee, rightKnee).", this);
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, csvFilename);
        if (!File.Exists(path))
        {
            Debug.LogError($"MotionPlayback: CSV not found: {path}", this);
            return;
        }

        string raw;
        try
        {
            raw = File.ReadAllText(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MotionPlayback: Failed to read CSV: {e.Message}", this);
            return;
        }

        if (!ParseCsv(raw, out _keyframes, out _columnIndices))
        {
            Debug.LogError("MotionPlayback: CSV parse failed. Expect header: leftFemur, rightFemur, leftFoot, rightFoot, leftKnee, rightKnee, wait", this);
            return;
        }

        if (_keyframes.Count == 0)
        {
            Debug.LogWarning("MotionPlayback: No keyframe rows in CSV.", this);
            return;
        }

        _playing = true;
        StartCoroutine(PlaybackCoroutine());
    }

    public void StopPlayback()
    {
        _playing = false;
    }

    IEnumerator PlaybackCoroutine()
    {
        for (int i = 0; i < _keyframes.Count && _playing; i++)
        {
            float[] row = _keyframes[i];
            for (int j = 0; j < 6 && j < pidControllers.Length; j++)
            {
                if (pidControllers[j] != null && _columnIndices[j] >= 0 && _columnIndices[j] < row.Length)
                    pidControllers[j].SetTargetFromDegrees(row[_columnIndices[j]]);
            }

            int waitMs = 0;
            if (_columnIndices[6] >= 0 && _columnIndices[6] < row.Length)
                waitMs = Mathf.RoundToInt(row[_columnIndices[6]]);

            if (waitMs > 0)
            {
                if (useRealtimeWait)
                    yield return new WaitForSecondsRealtime(waitMs * 0.001f);
                else
                    yield return new WaitForSeconds(waitMs * 0.001f);
            }
        }

        _playing = false;
    }

    bool ParseCsv(string raw, out List<float[]> keyframes, out int[] columnIndices)
    {
        keyframes = new List<float[]>();
        columnIndices = new int[7]; // 6 joints + wait
        for (int i = 0; i < columnIndices.Length; i++)
            columnIndices[i] = -1;

        string[] lines = raw.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return true; // header only => no keyframes

        string[] header = SplitCsvLine(lines[0]);
        for (int c = 0; c < header.Length; c++)
        {
            string name = header[c].Trim();
            for (int j = 0; j < ExpectedJointNames.Length; j++)
            {
                if (string.Equals(name, ExpectedJointNames[j], System.StringComparison.OrdinalIgnoreCase))
                {
                    columnIndices[j] = c;
                    break;
                }
            }
            if (string.Equals(name, WaitColumnName, System.StringComparison.OrdinalIgnoreCase))
                columnIndices[6] = c;
        }

        for (int j = 0; j < 7; j++)
        {
            if (columnIndices[j] < 0 && (j < 6 || j == 6))
            {
                if (j < 6)
                    Debug.LogWarning($"MotionPlayback: Header missing column '{ExpectedJointNames[j]}'.", this);
                else
                    Debug.LogWarning("MotionPlayback: Header missing column 'wait'.", this);
            }
        }

        for (int l = 1; l < lines.Length; l++)
        {
            string[] parts = SplitCsvLine(lines[l]);
            if (parts.Length == 0) continue;

            float[] values = new float[parts.Length];
            bool valid = true;
            for (int c = 0; c < parts.Length; c++)
            {
                if (!float.TryParse(parts[c].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out values[c]))
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
                keyframes.Add(values);
        }

        return true;
    }

    static string[] SplitCsvLine(string line)
    {
        var list = new List<string>();
        int start = 0;
        while (start < line.Length)
        {
            int end = line.IndexOf(',', start);
            if (end < 0) end = line.Length;
            list.Add(line.Substring(start, end - start));
            start = end + 1;
        }
        return list.ToArray();
    }
}
