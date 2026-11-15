
using System;
using System.Threading.Tasks;
using Supabase.Gotrue;

namespace Daymark.Services;

public class AuthService
{
    private readonly Supabase.Client _client;

    public AuthService(Supabase.Client client)
    {
        _client = client;
    }

    // Register a new user using an email and password.
    public async Task<bool> SignUpAsync(string email, string password)
    {
        try
        {
            var response = await _client.Auth.SignUp(email, password);

            if (response?.User == null)
                return false;

            Console.WriteLine("Registration request completed.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration failed: {ex.Message}");
            return false;
        }
    }

    // Sign in an existing user.
    public async Task<bool> SignInAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignIn(
                email,
                password
            );

            if (session?.User == null)
                return false;

            Console.WriteLine("User signed in successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sign in failed: {ex.Message}");
            return false;
        }
    }

    // Sign out the current user.
    public async Task SignOutAsync()
    {
        await _client.Auth.SignOut();

        Console.WriteLine("User signed out.");
    }

    // Retrieve the current user's unique ID.
    public string? GetCurrentUserId()
    {
        return _client.Auth.CurrentUser?.Id;
    }

    // Check whether a user is currently signed in.
    public bool IsSignedIn()
    {
        return _client.Auth.CurrentSession != null;
    }
}

