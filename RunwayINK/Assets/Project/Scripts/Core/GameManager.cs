using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
   [Header("Subsystems")]
    [SerializeField] private DrawingEngine drawingEngine;
    //[SerializeField] private MannequinSystem mannequinSystem;
    //[SerializeField] private AnimationController animationController;
    //[SerializeField] private MaterialSystem materialSystem;
    //[SerializeField] private ProjectManager projectManager;
    //[SerializeField] private CameraSystem cameraSystem;

    private void Awake()
    {
        // Register all core systems on boot
        XRServiceLocator.Register<IDrawingEngine>(drawingEngine);
        //XRServiceLocator.Register<IMannequinSystem>(mannequinSystem);
        //XRServiceLocator.Register<IAnimationController>(animationController);
        //XRServiceLocator.Register<IMaterialSystem>(materialSystem);
        //XRServiceLocator.Register<IProjectManager>(projectManager);
        //XRServiceLocator.Register<ICameraSystem>(cameraSystem);
        
        Debug.Log("[GameManager] Core architecture bootstrapped successfully.");
    }

    private void OnDestroy()
    {
        // Clean up when scene changes
        XRServiceLocator.Clear();
    }
    
}
