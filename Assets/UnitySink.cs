// LICENSE
// GNetworking, SimpleUnityClient and GameServer are property of Gordon Alexander MacPherson
// No warranty is provided with this code, and no liability shall be granted under any circumstances.
// All rights reserved GORDONITE LTD 2018 (c) Gordon Alexander MacPherson.

using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using UnityEngine;
namespace Core
{
    public class UnitySink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public UnitySink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }


        /// <summary>
        /// todo: make this properly throw still so stack trace can be investigated in editors
        /// </summary>
        /// <param name="logEvent"></param>
        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage(_formatProvider);
            switch(logEvent.Level)
            {
                case LogEventLevel.Debug:
                    Debug.Log("<color=blue>[Debug]</color> "+message);
                    break;
                case LogEventLevel.Error:
                    Debug.LogError("<color=red>[Error]</color> "+message);
                    break;
                case LogEventLevel.Fatal:
                    Debug.LogError("<b><color=red>[Fatal]</color></b> " + message);
                    break;
                case LogEventLevel.Information:
                    Debug.Log("<i><color=darkblue>[Info]</color></i> " + message);
                    break;
                case LogEventLevel.Verbose:
                    Debug.Log("<color=green>[Verbose]</color> " + message);
                    break;
                case LogEventLevel.Warning:
                    Debug.LogWarning("<color=yellow>[Warning]</color> " + message);
                    break;
            }
           //Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);
        }
    }

    public static class UnitySinkExtension
    {
        public static LoggerConfiguration UnitySink(
                    this LoggerSinkConfiguration loggerConfiguration,
                    IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new UnitySink(formatProvider));
        }
    }
}