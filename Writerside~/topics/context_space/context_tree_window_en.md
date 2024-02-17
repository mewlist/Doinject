# Checking the Structure of the Context Space (DI Context Tree Window)

You can check the structure of the context space at runtime, as well as the status of the DI containers belonging to each context.

From the Unity menu, select ```Window > Doinject > DI Context Tree```.
A window like the one below will be displayed, and you can check the types of contexts and their parent-child relationships.

![DI Context Tree](DIContextTree_Bindings.png)

When you select each context, you can check the status of the DI container belonging to that context.

| Column        | Description         |
|---------------|---------------------|
| Type          | Bound type |
| Resolver      | Type of instance  |
| Strategy      | Method of instance creation |
| InstanceCount | Number of instances   |

Instances created with AsTransient or Factory are not displayed here because they are not managed by the DI container.