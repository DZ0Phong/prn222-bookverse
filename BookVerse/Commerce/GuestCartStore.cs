using System.Text.Json;

namespace BookVerse.Commerce;

public static class GuestCartStore
{
    public const string SessionKey = "BookVerse.GuestCart";

    public static Dictionary<int, int> Read(ISession session)
    {
        var json = session.GetString(SessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? new Dictionary<int, int>()
            : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
    }

    public static void Write(ISession session, Dictionary<int, int> cart) =>
        session.SetString(SessionKey, JsonSerializer.Serialize(cart));

    public static void Clear(ISession session) => session.Remove(SessionKey);
}
