using System.Threading.Tasks;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Doinject
{
    internal static class MethodInfoExtension
    {
        public static bool IsTask(this System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(Task);
        }

        public static bool IsValueTask(this System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(ValueTask);
        }

#if USE_UNITASK
        public static bool IsUniTask(this System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(UniTask);
        }
#endif
    }
}