namespace Lab.Todo.SmokeTests.Services.Interfaces
{
    public interface ITokenHolder
    {
        string? Token { get; }
        void SetToken(string token);
    }
}