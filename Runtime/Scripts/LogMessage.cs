using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ConsoleLog
{
    public readonly struct LogMessage
    {
        private const string k_CoroutineRegex = @"(\w+).<(\w+)>\w+.MoveNext";
        private const string k_MethodRegex = @"(\w+):(\w+)";

        private static readonly Regex s_CoroutineRegex = new Regex(k_CoroutineRegex, RegexOptions.Compiled);
        private static readonly Regex s_MethodRegex = new Regex(k_MethodRegex, RegexOptions.Compiled);

        private static readonly float s_TicksPerMillisecond = 1000.0f / Stopwatch.Frequency;

        public readonly string message;
        public readonly string stacktrace;
        public readonly LogType type;
        public readonly float milliseconds;
        public readonly ulong frame;

        public LogMessage(string msg, string st, LogType lt, ulong f, long ticks)
        {
            message = msg;
            stacktrace = st;
            type = lt;
            frame = f;
            milliseconds = ticks * s_TicksPerMillisecond;
        }

        internal static (string className, string methodName) GetClassAndMethodName(string st)
        {
            if (string.IsNullOrEmpty(st))
            {
                return (null, null);
            }

            // coroutine method name is hidden by this: .<\w+>\w+:MoveNext
            // try matching the coroutine regex to stacktrace
            if (s_CoroutineRegex.IsMatch(st))
            {
                var coroutineMatch = s_CoroutineRegex.Match(st);

                var cn = coroutineMatch.Groups[1].Value;
                var mn = coroutineMatch.Groups[2].Value;

                return (cn, mn);
            }

            var methodMatch = s_MethodRegex.Match(st);
            while (methodMatch.Success)
            {
                var cn = methodMatch.Groups[1].Value;
                var mn = methodMatch.Groups[2].Value;

                if (((cn.Equals("Logger") || cn.Equals("Debug")) && mn.StartsWith("Log"))
                    || (cn.Equals("MonoBehaviour") && mn.Equals("print")))
                {
                    methodMatch = methodMatch.NextMatch();
                    continue;
                }

                return (cn, mn);
            }

            return (null, null);
        }
    }
}
