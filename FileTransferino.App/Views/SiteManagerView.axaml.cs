using Avalonia.Controls;
using Avalonia.Interactivity;
using FileTransferino.App.ViewModels;
using System;
using Avalonia.Markup.Xaml;

namespace FileTransferino.App.Views;

public partial class SiteManagerView : UserControl
{
    private SiteManagerViewModel? ViewModel => DataContext as SiteManagerViewModel;

    public SiteManagerView()
    {
        InitializeComponent();

        DataContextChanged += (s, e) => LogCommandStates();
        AttachedToVisualTree += (s, e) =>
        {
            // Just log state; do not create a ViewModel here. MainWindow will create and assign the ViewModel.
            LogCommandStates();
        };
    }

    private void LogCommandStates()
    {
        try
        {
            var vm = ViewModel;
            if (vm == null)
            {
                System.Diagnostics.Debug.WriteLine("SiteManagerView: DataContext is null");
                System.IO.File.AppendAllText("C:\\dev-priv\\FileTransferino\\debug_cmd_states.log", "DataContext null\n");
                return;
            }

            var newCs = vm.NewSiteCommand?.CanExecute(null) ?? false;
            var saveCs = vm.SaveCommand?.CanExecute(null) ?? false;
            var delCs = vm.DeleteCommand?.CanExecute(null) ?? false;

            System.Diagnostics.Debug.WriteLine($"New CanExecute: {newCs}");
            System.Diagnostics.Debug.WriteLine($"Save CanExecute: {saveCs}");
            System.Diagnostics.Debug.WriteLine($"Delete CanExecute: {delCs}");

            System.IO.File.AppendAllText("C:\\dev-priv\\FileTransferino\\debug_cmd_states.log",
                $"{DateTime.Now:O} New={newCs} Save={saveCs} Delete={delCs}\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error logging command states: {ex}");
            System.IO.File.AppendAllText("C:\\dev-priv\\FileTransferino\\debug_cmd_states.log", $"err:{ex}\n");
        }
    }

    private void OnAddSiteClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.NewSite();
            LogCommandStates();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnAddSiteClick error: {ex}");
        }
    }

    private void OnLearnMoreClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel != null)
                ViewModel.OpenDocsCommand?.Execute(null);
            else
                System.Diagnostics.Debug.WriteLine("OpenDocs invoked but ViewModel is null");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnLearnMoreClick error: {ex}");
        }
    }

    // Added a "Connect" button to the Site Manager UI
    private void OnConnectClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.ConnectToSite();
            LogCommandStates();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnConnectClick error: {ex}");
        }
    }

    // Ensure the Connect button is properly added to the layout
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var connectButton = this.FindControl<Button>("ConnectButton");
        connectButton?.Click += OnConnectClick;
    }
}
