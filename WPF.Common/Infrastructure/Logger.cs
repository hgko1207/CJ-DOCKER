using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WPF.Common.Infrastructure
{
    public class Logger : ILogger, IDisposable
    {
        private readonly TextWriter writer;
        private readonly FileStream fileStream;
        private readonly string savePath;

        public Logger()
        {
            savePath = GenerateLoggingPath();
            fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
            WriteBasicInfo();
        }

        private static string GenerateLoggingPath()
        {
            string directory = @"C:\AIXAC_RX\log\";
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch { }

            return Path.Combine(directory + $"AIXAC_RX.{DateTime.Now:yyyyMMdd}.log");
        }

        private void WriteBasicInfo()
        {
            writer.WriteLine($"{SystemInfo.Caption}: [{SystemInfo.Version}] {SystemInfo.OSArchitecture}");
        }

        public void Log(string message, IDictionary<string, string> properties)
        {
            string messageToLog = string.Format(CultureInfo.InvariantCulture, "[{0:u}] {1}.", DateTime.Now, message);
            writer.WriteLine(messageToLog);
        }

        public void Report(Exception ex, IDictionary<string, string> properties)
        {
            string messageToLog = string.Format(CultureInfo.InvariantCulture, "[{0:u}] {1}.", DateTime.Now, ex.Message);
            writer.WriteLine(messageToLog);
        }

        public void TrackEvent(string name, IDictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                writer?.Dispose();
                fileStream?.Dispose();
            }
        }
    }
}
