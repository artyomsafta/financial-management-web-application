namespace Task11_DotNETBackendWebApi.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsDeleted { get; set; } = false;

    // TODO: implement the Wallet entity and uncomment the following line when will be ready to implement the relationship between User and Wallet (task №6)
    // public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
