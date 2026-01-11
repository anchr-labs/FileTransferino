using Avalonia.Controls;
using Avalonia.Interactivity;
using FileTransferino.App.ViewModels;

namespace FileTransferino.App.Views;

public partial class SiteManagerView : UserControl
{
    private SiteManagerViewModel? ViewModel => DataContext as SiteManagerViewModel;

    public SiteManagerView()
    {
        InitializeComponent();
        
        // Wire up button events
        var newButton = this.FindControl<Button>("NewButton");
        var saveButton = this.FindControl<Button>("SaveButton");
        var deleteButton = this.FindControl<Button>("DeleteButton");
        
        if (newButton != null)
            newButton.Click += OnNewClick;
        
        if (saveButton != null)
            saveButton.Click += OnSaveClick;
        
        if (deleteButton != null)
            deleteButton.Click += OnDeleteClick;
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.NewSite();
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await (ViewModel?.SaveSiteAsync() ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error saving site: {ex.Message}");
        }
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await (ViewModel?.DeleteSiteAsync() ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error deleting site: {ex.Message}");
        }
    }
}
