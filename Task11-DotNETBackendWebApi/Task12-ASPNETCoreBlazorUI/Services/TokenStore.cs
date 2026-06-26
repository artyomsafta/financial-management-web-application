namespace Task12_ASPNETCoreBlazorUI.Services;

public class TokenStore
{
    public string? AccessToken { get; private set; }

    public bool HasToken => !string.IsNullOrWhiteSpace(AccessToken);

    public void SetToken(string token)
    {
        AccessToken = token;
    }

    public void Clear()
    {
        AccessToken = null;
    }
}
