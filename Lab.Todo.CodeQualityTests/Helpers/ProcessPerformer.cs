using System;
using System.Diagnostics;

namespace Lab.Todo.CodeQualityTests.Helpers
{
    internal static class ProcessPerformer
    {
        public static void Perform(string fileName, string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = command,
                    RedirectStandardError = true
                }
            };

            if (!process.Start())
            {
                throw new ApplicationException("The process was not started: Start() method returned false.");
            }

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return;
            }

            var errorMessage = process.StandardError.ReadToEnd();

            throw new ApplicationException(
                $"The process was failed with exit code {process.ExitCode}. Error message: {errorMessage}.");
        }
    }
}