
namespace Bookkeeping.Common.Interfaces
{
    public interface ISettingsManager
    {
        T GetValue<T>(string key, T defaultValue = default(T));
        T GetValue<T>(string key);
    }
}
