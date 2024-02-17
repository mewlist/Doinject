# What to Read When You're Stuck

## When You Don't Understand the Error Message

```
FailedToResolveParameterException: Failed to resolve parameter [SomeObject] for [SomeObject]
```

This error indicates that the argument `SomeObject` at the injection point of `SomeComponent` is not registered in the DI container. Check if the binding of `SomeObject` is coded in the installer. Depending on the situation, you may need to define the argument with Args().

```
Exception: Already has binding for type [SomeObject]
```

This error means that the type `SomeObject` is being bound twice. Review the coding in the installer and check if the same type is not bound twice.

> Doinject can only code a single binding for each type in a DI container. 
> If multiple instances are needed, consider creating a factory to generate them, or prepare a new type by inheriting or placing an adapter. 
> By creating a type, it will be easier to receive IDE support and your code will have a clearer meaning.
{style="note"}