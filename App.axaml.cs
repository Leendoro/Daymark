
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Daymark.Services;
using Daymark.ViewModels;
using Daymark.Views;

namespace Daymark;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };

            // Test the Supabase connection without blocking the UI.
            _ = TestSupabaseConnectionAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async System.Threading.Tasks.Task TestSupabaseConnectionAsync()
    {
        try
        {
            var supabase = new SupabaseService(
                SupabaseConfig.ProjectUrl,
                SupabaseConfig.PublishableKey
            );

            bool connected = await supabase.TestConnectionAsync();

            Console.WriteLine(
                connected
                    ? "Daymark: Supabase initialization succeeded!"
                    : "Daymark: Supabase initialization failed."
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Daymark: Supabase error: {ex.Message}"
            );
        }
    }
}
