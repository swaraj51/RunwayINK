using UnityEngine;

// Task 2.1: Drawing Engine
public interface IDrawingEngine 
{
    void ProcessInput(Vector3 position, Quaternion rotation, float pressure, bool isDrawingNow);
    void SetColor(Color color);
    void ClearAllStrokes();
}


// Task 2.2: Mannequin System
public interface IMannequinSystem 
{
    void LoadMannequin(bool isMale);
    void SetScale(float scalePercentage);
    Transform GetActiveMannequinTransform();
}

public class MannequinSystem : MonoBehaviour, IMannequinSystem 
{
    public void LoadMannequin(bool isMale) { }
    public void SetScale(float scalePercentage) { }
    public Transform GetActiveMannequinTransform() => transform;
}

// Task 2.3: Animation Controller
public interface IAnimationController 
{
    void PlayIdle();
    void PlayRunwayWalk();
    void SetSpeed(float speedMultiplier);
}

public class AnimationController : MonoBehaviour, IAnimationController 
{
    public void PlayIdle() { }
    public void PlayRunwayWalk() { }
    public void SetSpeed(float speedMultiplier) { }
}

// Task 2.4: Material System
public interface IMaterialSystem 
{
    void SetFabric(string fabricType);
    void SetSkinTone(int toneIndex);
}

public class MaterialSystem : MonoBehaviour, IMaterialSystem 
{
    public void SetFabric(string fabricType) { }
    public void SetSkinTone(int toneIndex) { }
}

// Task 2.5: Project Manager (Save/Load)
public interface IProjectManager 
{
    void SaveProject();
    void LoadProject(string projectId);
}

public class ProjectManager : MonoBehaviour, IProjectManager 
{
    public void SaveProject() { }
    public void LoadProject(string projectId) { }
}

// Task 2.6: Camera System
public interface ICameraSystem 
{
    void SetPresetAngle(int angleIndex);
    void EnableRunwayFollow(Transform target);
}

public class CameraSystem : MonoBehaviour, ICameraSystem 
{
    public void SetPresetAngle(int angleIndex) { }
    public void EnableRunwayFollow(Transform target) { }
}
public interface IStylusProvider 
{
    Vector3 TipPosition { get; }
    Quaternion TipRotation { get; }
    float Pressure { get; }
    bool IsDrawing { get; }
    bool IsEraser { get; }
}