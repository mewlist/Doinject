# Automatically Load Dependent Parent Contexts

If a scene context depends on another parent context space, you need to open the scene that serves as the parent context and then execute the editor.

However, closing the scene you're supposed to be working on and opening another scene every time you need to verify operation can be very inefficient and stressful.

Here, we will explain how to automatically load the dependent parent context without closing the scene you are editing.

## ParentSceneContextRequirement

To automatically load the dependent parent context scene, place the ```ParentSceneContextRequirement``` component anywhere in the current scene.

![ParentContextSceneRequirement.png](ParentContextSceneRequirement.png)

In the inspector, specifying the dependent parent context scene will automatically load the parent context every time you play this scene.

Also, if the parent scene has additional dependent scenes, you can load them in a chain by setting up the parent scene in the same way.