using System.Collections.Generic;
using UnityEngine;

// Task 5.1: Stroke Data Structure
public struct StrokePoint
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Pressure;
    public Color PointColor;
    
    // NEW SPRINT 3 VARIABLE
    public Vector3 Normal; 

    public StrokePoint(Vector3 pos, Quaternion rot, float pressure, Color color, Vector3 normal)
    {
        Position = pos;
        Rotation = rot;
        Pressure = pressure;
        PointColor = color;
        Normal = normal; // Save the skin angle!
    }
}

public class Stroke
{
    public List<StrokePoint> Points { get; private set; } = new List<StrokePoint>();
    public float StartTime { get; private set; }

    public Stroke()
    {
        StartTime = Time.time;
    }

    public void AddPoint(StrokePoint point)
    {
        Points.Add(point);
    }
}