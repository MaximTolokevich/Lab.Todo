using Lab.Todo.CodeQualityTests.Helpers;
using System;
using System.IO;

namespace Lab.Todo.CodeQualityTests
{
    public class CodeQualityTestsFixture : IDisposable
    {
        public string OutputFilePath { get; }

        public CodeQualityTestsFixture()
        {
            OutputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "output");

            ProcessPerformer.Perform("dotnet", "new tool-manifest --force");
            ProcessPerformer.Perform("dotnet", "tool update JetBrains.ReSharper.GlobalTools");
            ProcessPerformer.Perform("dotnet", "tool restore");
        }

        public void Dispose()
        {
            if (File.Exists(OutputFilePath))
            {
                try
                {
                    File.Delete(OutputFilePath);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}