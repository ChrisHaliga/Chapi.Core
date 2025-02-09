
namespace Chapi.IntegrationTests.Spies
{
    public interface IServiceSpy
    {
        List<HistoryRecord> History { get; }
        object? GetInstanceOfHistoryItem(string key);
    }

    public record HistoryRecord(string method, string key, bool isSuccess);

}
