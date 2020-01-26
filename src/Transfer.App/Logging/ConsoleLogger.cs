using System;

namespace Transfer.App.Logging
{
    internal class ConsoleLogger : ILog
    {
        public void Info(string message) => LogMessage(message, MessageType.INFO);

        public void Warn(string message) => LogMessage(message, MessageType.WARNING);

        public void Error(string message) => LogMessage(message, MessageType.ERROR);

        private void LogMessage(string message, MessageType messageType)
        {
            SetConsoleColor(messageType);
            Console.WriteLine($"{DateTime.Now} - {messageType}: {message + Environment.NewLine}");
        }

        private static void SetConsoleColor(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.WARNING:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case MessageType.ERROR:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }
        }

        private enum MessageType
        {
            INFO,
            WARNING,
            ERROR
        }
    }
}