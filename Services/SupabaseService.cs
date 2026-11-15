
using System;
using System.Threading.Tasks;
using Supabase;

namespace Daymark.Services;

public class SupabaseService
{
    private readonly Supabase.Client _client;

    // Expose the Supabase client.
    public Supabase.Client Client => _client;

    // Authentication service using the same client.
    public AuthService Auth { get; }

    public SupabaseService(string projectUrl, string publishableKey)
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false,
            AutoRefreshToken = true
        };

        _client = new Supabase.Client(
            projectUrl,
            publishableKey,
            options
        );

        // Share the existing client with authentication.
        Auth = new AuthService(_client);
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await InitializeAsync();

            Console.WriteLine(
                "Supabase initialized successfully."
            );

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Supabase connection failed: {ex.Message}"
            );

            return false;
        }
    }
}
