using System;

namespace Lab.Todo.SmokeTests.Services.Interfaces
{
    public interface IUserContext : IDisposable
    {
        string PreviousToken { get; set; }
    }
}