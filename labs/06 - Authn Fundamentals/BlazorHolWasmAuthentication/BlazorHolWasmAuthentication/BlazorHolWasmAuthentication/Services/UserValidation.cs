namespace BlazorHolWasmAuthentication.Services;

public class UserValidation
{
    public bool ValidateUser(string username, string password)
    {
        var user = Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        return user != null;
    }

    public List<string> GetRoles(string username)
    {
        var user = Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return [];
        }
        else
        {
            return user.Roles;
        }
    }

    private class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
    }

    private readonly static List<User> Users =
    [
        new User { Username = "admin", Password = "admin", Roles = ["Admin"] },
        new User { Username = "user", Password = "user", Roles = ["User"] }
    ];
}