# RunwayINK 👗✨

**RunwayINK** is an innovative VR/3D fashion design application built in Unity. It allows designers to intuitively sketch, drape, and create 3D clothing directly onto a digital mannequin in real-time. Forget complex 3D modeling software—if you can sketch it, you can wear it!
APK: https://drive.google.com/file/d/1Xbt4DaFfdf5vbCMnrU-FsCNU7a3wv6xG/view?usp=sharing
---

## 🌟 Key Features

*   **Spatial Drawing Engine:** Draw directly onto the mannequin in 3D space. Your strokes automatically snap to the contours of the body using our smart bone-scanning algorithm.
*   **Sketch Pencil & Outlines:** Use the Sketch Pencil to draw crisp boundary lines and define the silhouette of your garments.
*   **Fabric Marker (Drape Mode):** Switch to Drape Mode to draw physical 3D fabrics directly onto the body.
*   **Smart Paint Bucket:** Automatically fill your sketched outlines with physical 3D fabric meshes.
*   **Spline/Node Editor:** Refine your designs by grabbing and moving individual points of your drawn strokes for perfect tailoring.
*   **Dynamic Fabric System:** Choose between different physical fabric materials (Silk, Denim, Leather, Cotton) that react beautifully to lighting.
*   **Color Dipping System:** Seamlessly change the color of your fabrics and pencils via an intuitive UI palette.
*   **Smart Undo System:** Safely undo your last pencil stroke or fabric patch without losing your entire design.

## 🛠️ Technology Stack

*   **Engine:** Unity 3D
*   **Language:** C#
*   **Rendering:** Universal Render Pipeline (URP)

## 🎮 How It Works

1.  **Select Your Tool:** Use the `DockManager` UI to choose between the **Sketch Pencil** (for outlines) or **Fabric Marker** (for draping).
2.  **Pick a Color & Fabric:** Dip your stylus into a color and select your preferred fabric material (Silk, Denim, etc.).
3.  **Draw on the Mannequin:** Aim your stylus at the mannequin. The smart laser will turn cyan when it detects the canvas (skin). Pull the trigger and start drawing!
4.  **Edit & Refine:** Hold the edit button to grab any point on your drawn lines and adjust its position in 3D space.
5.  **Fill with Fabric:** Use the Paint Bucket to instantly turn your closed outlines into double-sided fabric patches.

## 📁 Project Structure

*   **`DrawingEngine.cs`:** The core spatial drawing logic, raycasting, mesh generation, and stroke management.
*   **`DockManager.cs`:** Handles the VR UI panels, tool switching (Pencil vs. Marker), and fabric material selection.
*   **`ColorDipButton.cs`:** Manages the color palette and applies the chosen color to the active tool.
*   **`PaletteManager.cs` & `RunwayController.cs`:** Handles additional UI and scene interactions.

## 🚀 Getting Started

1.  Clone this repository.
2.  Open the project in **Unity**.
3.  Ensure your project is set to use the **Universal Render Pipeline (URP)**.
4.  Open the main scene.
5.  Assign your VR/Input controller to the `DrawingEngine` script on the stylus.
6.  Press **Play** and start designing!

---

*“Bridging the gap between a sketchpad and the runway.”*
