using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doinject.Tests
{
    public class ConstructorInjectionTestClass
    {
        public InjectedObject InjectedObject { get; set; }

        public ConstructorInjectionTestClass(InjectedObject injectedObject)
        {
            InjectedObject = injectedObject;
        }
    }

    public class MethodInjectionTestClass
    {
        public InjectedObject InjectedObjectSync { get; set; }
        public InjectedObject InjectedObjectASync { get; set; }
        public InjectedObject InjectedObjectValueTask { get; set; }

        [Inject]
        public void Inject(InjectedObject injectedObject)
        {
            InjectedObjectSync = injectedObject;
        }

        [Inject]
        public async Task InjectAsync(InjectedObject injectedObject)
        {
            await Task.Delay(100);
            InjectedObjectASync = injectedObject;
        }

        [Inject]
        public async ValueTask InjectValueTask(InjectedObject injectedObject)
        {
            await Task.Delay(100);
            InjectedObjectValueTask = injectedObject;
        }
    }

    public class InjectedObject : IAsyncDisposable
    {
        public bool OnInjectedCalled { get; private set; }
        public bool OnPostInjectedCalled { get; private set; }

        public InjectedObject()
        { }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public void OnInjected()
        {
            OnInjectedCalled = true;
        }

        public void OnPostInjected()
        {
            OnPostInjectedCalled = true;
        }
    }

    public class WithArgsObject : IAsyncDisposable
    {
        public InjectedObject InjectedObject { get; }
        public int Arg1 { get; }
        public string Arg2 { get; }
        public List<int> Arg3 { get; }

        public WithArgsObject(int arg1, InjectedObject injectedObject, string arg2, List<int> arg3)
        {
            InjectedObject = injectedObject;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }
    }

    public class OptionalInjectionTestClass
    {
        public OptionalInjectionTestClass([Optional]InjectedObject injectedObject)
        {
        }
    }

    public class InjectableObject : IDisposable
    {
        public InjectedObject InjectedObject { get; set; }

        public InjectableObject(InjectedObject injectedObject)
        {
            InjectedObject = injectedObject;
        }

        public void Dispose()
        { }
    }

    public interface ITestInterface
    {
    }

    public interface INoImplementInterface
    {
    }

    public class TestBaseClassObject
    {
    }

    public class TestNotOriginBaseClassObject
    {
    }

    public class TestSubClassObject : TestBaseClassObject
    {
    }

    public class TestImplementClassObject : ITestInterface
    {
    }
}