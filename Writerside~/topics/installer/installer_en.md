# Installer

To specify the types to be resolved in the DI container and how to resolve them, we use an installer.

The installer can be placed as a component in a context scene or a game object context, allowing you to register the bindings described there in the DI container.

## Direct Binding of Components Placed in the Scene

Without writing an installer script, you can also directly bind components placed in the scene from the inspector.

## Prefab Installer

These can be bundled into a Prefab and instantiated and installed at runtime. 
For example, when performing binding of functions that require game objects, such as UI components, they can be bundled into prefabs by function.

In addition, things that do not depend on instances in scenes or prefabs can also be structured as installers in the form of ScriptableObjects.