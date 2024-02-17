# Introduction

## What is Doinject

![Logo.svg](Logo.svg)

Doinject is an asynchronous DI (Dependency Injection) framework for Unity.

The concept is based on an asynchronous DI container.
Unfortunately, we do not support versions of Unity prior to 2022.

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/license-MIT-blue)

## Concept

### Asynchronous DI Container

The framework supports the creation and release of asynchronous instances.
This allows you to create instances through the Addressables Asset Systems.
Also, if you create your own custom factory, you can delegate the creation of time-consuming instances to the DI container.

### Context Space that Does Not Contradict Unity's Lifecycle

It is designed to define a context space in a way that does not contradict Unity's lifecycle.
When you close a scene, it closes the context associated with the scene, and the instances created in that context space disappear.
If you Destroy a game object with a context, it will also close the context.

The context space is automatically configured by the framework, and if multiple contexts are loaded, a parent-child relationship is created.

### Collaboration with Addressable Asset System

It can handle instances of the Addressable Asset System and automate the release of load handles.
Managing resources with Addressables often requires the creation of a custom resource management system or careful implementation.
However, with Doinject, it automatically handles the loading and releasing of Addressables.

### Simple Coding

You can achieve replacements for the factory pattern, (context-closed) singleton pattern, and service locator pattern with simple coding.
Also, by creating custom factories and custom resolvers, you can handle more complex instance creation scenarios.