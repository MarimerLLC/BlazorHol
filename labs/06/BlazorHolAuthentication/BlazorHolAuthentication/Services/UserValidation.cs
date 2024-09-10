namespace BlazorHolAuthentication.Services;

public class UserValidation
{
    public bool ValidateUser(string username, string password)
    {
        if (username == "admin" && password == "admin")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<string> GetRoles(string username)
    {
        if (username == "admin")
        {
            return new List<string> { "Admin" };
        }
        else
        {
            return new List<string> { "User" };
        }
    }
}
