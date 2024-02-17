# Project Context Space

This is the context that encompasses the entire project. If a project context is set for the project, it becomes effective immediately after the application starts. You can register the necessary bindings for the entire project using the *Scriptable Object Installer*.

## Creating a Project Context

To enable the project context, select the 
<ui-path>Tools > Doinject > Create Project Context</ui-path>
menu.

![CreateProjectContext.png](CreateProjectContext.png)

The project context is created as a ```ScriptableObject``` in the <path>Assets/Resources</path> path.

![ProjectContextInspector.png](ProjectContextInspector.png)

From the inspector, by registering the *ScriptableObject Installer*, you can install the necessary bindings throughout the entire project.