using System.Collections.Generic;
using UnityEngine;

// Task 5.1: Stroke Data Structure
public struct StrokePoint
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Pressure; // 0.0 to 1.0 (from controller trigger)
    public Color PointColor;

    public StrokePoint(Vector3 pos, Quaternion rot, float pressure, Color color)
    {
        Position = pos;
        Rotation = rot;
        Pressure = pressure;
        PointColor = color;
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