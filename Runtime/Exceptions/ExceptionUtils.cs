using System;

namespace Doinject
{
    public static class ExceptionUtils
    {
        private const string MsgColor = "#FFC107";
        public static string ToExceptionMessage(this string message, Exception inner = null)
        {
            return $"<color={MsgColor}>{message}</color>\n{LeafMessage(inner)}\n\n";
        }

        private static string InnerMessage(Exception inner)
        {
            return inner is null
                ? string.Empty
                : $"> {ToExceptionMessage(inner.Message, inner.InnerException)}";
        }

        private static string LeafMessage(Exception exception)
        {
            return exception is null
                ? string.Empty
                : exception.InnerException is null
                    ? $"{exception}"
                    : LeafMessage(exception.InnerException);
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