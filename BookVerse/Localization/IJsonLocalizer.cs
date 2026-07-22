namespace BookVerse.Localization;

public interface IJsonLocalizer
{
    string this[string key] { get; }
    string Get(string key, params object[] arguments);
    IReadOnlyDictionary<string, string> GetClientTextMap(string culture);
}
