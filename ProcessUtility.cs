namespace Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ProcessExecutionInfo
    {
        /// <summary>
        /// Gets or sets the file path of the exe to execute
        /// </summary>
        public string ExeFilePath { get; set; }

        /// <summary>
        /// Gets or sets the working directory, null to use system default
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the arguments to call the exe
        /// </summary>
        public string Arguments { get; set; }
    }

    public static class ProcessUtility
    {
        /// <summary>
        /// Execute exe
        /// </summary>
        /// <param name="executionInfo">execution info</param>
        /// <param name="stdoutWriter">stdout stream writer, null to ignore stdout</param>
        /// <param name="stderrWriter">stderr stream writer, null to ignore stderr</param>
        /// <param name="timeoutInMilliseconds">timeout in milliseconds, -1 to wait infinitely</param>
        /// <returns>return value of the exe</returns>
        public static int ExecuteExe(ProcessExecutionInfo executionInfo, StreamWriter stdoutWriter, StreamWriter stderrWriter, int timeoutInMilliseconds = Timeout.Infinite)
        {
            Guard.ArgumentNotNull(executionInfo, nameof(executionInfo));
            Guard.ArgumentNotNullOrEmpty(executionInfo.ExeFilePath, $"{nameof(executionInfo)}.{nameof(executionInfo.ExeFilePath)}");
            if (timeoutInMilliseconds < 0 && timeoutInMilliseconds != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutInMilliseconds), $"{nameof(timeoutInMilliseconds)} must be equal to or greater than 0, or equal to {Timeout.Infinite}.");
            }

            using (Process buildProcess = new Process())
            {
                buildProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(executionInfo.ExeFilePath);
                buildProcess.StartInfo.UseShellExecute = false;
                buildProcess.StartInfo.CreateNoWindow = true;
                buildProcess.StartInfo.Arguments = executionInfo.Arguments;
                buildProcess.StartInfo.WorkingDirectory = executionInfo.WorkingDirectory == null ? null : Environment.ExpandEnvironmentVariables(executionInfo.WorkingDirectory);

                // set TEMP/TMP variables
                buildProcess.StartInfo.EnvironmentVariables["TEMP"] = Environment.GetEnvironmentVariable("TEMP");
                buildProcess.StartInfo.EnvironmentVariables["TMP"] = Environment.GetEnvironmentVariable("TMP");
                buildProcess.StartInfo.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH");

                buildProcess.StartInfo.RedirectStandardOutput = true;
                buildProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                buildProcess.StartInfo.RedirectStandardError = true;
                buildProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                buildProcess.Start();

                var outputTask = Task.Factory.StartNew(() =>
                {
                    var buffer = new char[512];
                    while (true)
                    {
                        var read = buildProcess.StandardOutput.Read(buffer, 0, 512);
                        if (read > 0)
                        {
                            try
                            {
                                stdoutWriter?.Write(buffer, 0, read);
                            }
                            catch (Exception ex)
                            {
                                TraceEx.TraceError($"Unable to write standard output, details: {ex}");
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }, TaskCreationOptions.LongRunning);

                var errorTask = Task.Factory.StartNew(() =>
                {
                    var buffer = new char[512];
                    while (true)
                    {
                        var read = buildProcess.StandardError.Read(buffer, 0, 512);
                        if (read > 0)
                        {
                            try
                            {
                                stderrWriter?.Write(buffer, 0, read);
                            }
                            catch (Exception ex)
                            {
                                TraceEx.TraceError($"Unable to write error output, details: {ex}");
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }, TaskCreationOptions.LongRunning);

                try
                {
                    if (buildProcess.WaitForExit(timeoutInMilliseconds))
                    {
                        return buildProcess.ExitCode;
                    }
                    else
                    {
                        buildProcess.Kill();
                        buildProcess.WaitForExit();
                        throw new TimeoutException($"{executionInfo.ExeFilePath} (argument: {executionInfo.Arguments}, working directory: {executionInfo.WorkingDirectory}) didn't exit in {timeoutInMilliseconds} ms. Force to exit.");
                    }
                }
                finally
                {
                    outputTask.Wait();
                    errorTask.Wait();
                }
            }
        }
    }
}
