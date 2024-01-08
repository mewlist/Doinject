using System;

namespace Doinject
{
    public static class ExceptionUtils
    {
        private const string MsgColor = "#FFC107";
        public static string ToExceptionMessage(this string message, Exception inner = null)
        {
            return $"<color={MsgColor}>{message}{InnerMessage(inner)}</color>";
        }

        private static string InnerMessage(Exception inner)
        {
            return inner is null
                ? string.Empty
                : $"\n > {inner.Message}";
        }


        public static Exception FindLeafException(this Exception exception)
        {
            var node = exception;
            while (node.InnerException != null)
                node = node.InnerException;
            return node;
        }

        public static T FindInnerException<T>(this Exception exception)
            where T : Exception
        {
            var inner = exception.InnerException;
            while (inner != null)
            {
                if (inner is T t) return t;
                inner = inner.InnerException;
            }

            return null;
        }
    }
}