# Unity3D Mask Drawer
This component allows to draw RGB mask directly inside scene view
## Preview
![image](/ReadmeImages/Preview.gif "Usage of component inside editor")
## Abilities
- Draw RGB mask in editor.
- Draw in play mode (require to have "MainCamera" tag on scene camera).
- Don't care about stitches. Brush projected directly on mesh. <details>
    <summary>Proof</summary>
  
    ![image](/ReadmeImages/Image0.jpg "Plane used in preview scene")
</details>

- Support skinned mesh renderer.
- "Read/Write enabled" in import settings doesn't required
- Custom brush masks.
- Orthographic and perspective projection types.
- Easy export result as .PNG.
- Up to 4 images for draw (for black, red, green, blue; alpha channel didn't use)
## How to use
This component works as usual MonoBehaviour script.
1. Assign [Drawer.cs](/Assets/Scripts/Drawer.cs) to any object on scene.
2. Assign shaders to component. [This](/Assets/Shaders/UVRenderer.shader) to "UV Projection Shader", [this](/Assets/Shaders/RenderTexture%20Shader.shader) to "RenderTexture Shader"
3. Assign target mesh from hierarchy.
4. Create a mask. In project folder create **Custom Render Texture**. 
- Configure size as you wish (power-of-two size of side is preferred).
- Initialization Source => Texture and Color; Color => Set black color.
- Be sure to check **Double buffered** switch turned on.
5. Assign mask into Drawer component on scene.
6. Create material. You need to create a shader that support masks. I prepared [this one](/Assets/Shaders/WorldCoordSimple.shader). It's simple world-coord shader. You can choose this from path "Drawable/World Coord Simple".
7. Assign created material on target mesh.
8. Assign draw mask in property field. You can use [prepared](/Assets/Masks/Circle.jpg) image. If you want to create you own, don't forget to set **Wrap Mode** to **Clamp** in import settings of your asset.
9. Profit! For start draw just turn on toggle "Draw in edit mode"
## Requirements
- CustomRenderTexture feature is released in 2017.1. [Check](https://unity.com/releases/editor/whats-new/2017.1.0-0). You need younger version.
- Image Conversion package (in Build-in packages) to export mask as .PNG file.
## Known issues
- Doesn't work with (new) Input System.
- Catch left mouse button press and deactivate selection frame works weird. When you draw, don't use combination of Alt+LMB to orbit. Alternatively use RMB to orbit, MMB to move editor camera.