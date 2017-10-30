namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    public static class LogMessageUtility
    {
        private const string LogFileRelativePath = @"Log\LogMessages.json";


        private static readonly Lazy<IReadOnlyDictionary<string, LogMessage>> LazyLogMessageDictionary = new Lazy<IReadOnlyDictionary<string, LogMessage>>(
            () =>
            {
                var path = Path.Combine(Utility.GetCurrentAssemblyFolder(), LogFileRelativePath);
                var logMessages = JsonUtility.FromJsonString<List<LogMessage>>(File.ReadAllText(path));
                return logMessages.ToDictionary(lm => lm.Code, StringComparer.OrdinalIgnoreCase);
            });

        public static IReadOnlyDictionary<string, LogMessage> LogMessageDictionary => LazyLogMessageDictionary.Value;

        public static string FormatMessage(LogCode logCode, params object[] args)
        {
            LogMessage logMessage;
            if (!LogMessageDictionary.TryGetValue(logCode.ToString(), out logMessage))
            {
                throw new ApplicationException($"Log message is not defined for log code: {logCode}.");
            }

            try
            {
                string message = string.Format(logMessage.Message, args);
                return string.IsNullOrEmpty(message) ? logCode.ToString() : message;
            }
            catch (FormatException fe)
            {
                var argsString = args == null ? "<null>" : string.Join(", ", args);
                throw new ApplicationException($"Log message {logMessage} for log code {logCode} has less paramters than provided {argsString}", fe);
            }
        }
    }

    public class LogMessage
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
