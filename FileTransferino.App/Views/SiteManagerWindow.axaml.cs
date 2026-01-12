using Avalonia.Controls;
using Avalonia.Interactivity;
using FileTransferino.App.ViewModels;
using FileTransferino.Infrastructure;

namespace FileTransferino.App.Views;

public partial class SiteManagerWindow : Window
{
    private readonly SiteManagerViewModel _viewModel;

    // Parameterless constructor for XAML loader / design-time tooling
    public SiteManagerWindow()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            // In design mode, initialize the UI without requiring a real view model
            InitializeComponent();
            return;
        }

        // At runtime, this constructor should not be used because it cannot provide
        // the required dependencies for SiteManagerViewModel.
        throw new InvalidOperationException(
            "SiteManagerWindow must be constructed with a SiteManagerViewModel instance.");
    }
    public SiteManagerWindow(SiteManagerViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
        
        InitializeComponent();
        
        // Wire up button events
        var newButton = this.FindControl<Button>("NewButton");
        var saveButton = this.FindControl<Button>("SaveButton");
        var deleteButton = this.FindControl<Button>("DeleteButton");
        var closeButton = this.FindControl<Button>("CloseButton");
        
        if (newButton != null)
            newButton.Click += OnNewClick;
        
        if (saveButton != null)
            saveButton.Click += OnSaveClick;
        
        if (deleteButton != null)
            deleteButton.Click += OnDeleteClick;
        
        if (closeButton != null)
            closeButton.Click += OnCloseClick;
        
        // Load sites when window opens
        Opened += async (_, _) => await _viewModel.LoadSitesAsync();
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.NewSite();
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _viewModel.SaveSiteAsync();
            
            if (!success)
            {
                // Show error message
                var messageBox = new Window
                {
                    Title = "Error",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = "Failed to save site. Please ensure Name and Host are filled.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(20),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }
                };
                
                await messageBox.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving site: {ex}");
        }
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var selected = _viewModel.SelectedSite;
        if (selected == null)
            return;

        var siteName = selected.Name;

        // Confirm deletion
        var yesButton = new Button { Content = "Yes", Width = 80 };
        var noButton = new Button { Content = "No", Width = 80 };

        var confirmBox = new Window
        {
            Title = "Confirm Delete",
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Are you sure you want to delete '{siteName}'?",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 10,
                        Children =
                        {
                            yesButton,
                            noButton
                        }
                    }
                }
            }
        };

        var confirmed = false;

        yesButton.Click += (_, _) => { confirmed = true; confirmBox.Close(); };
        noButton.Click += (_, _) => confirmBox.Close();

        await confirmBox.ShowDialog(this);

        if (!confirmed)
            return;

        try
        {
            var ok = await _viewModel.DeleteSiteAsync();
            if (!ok)
            {
                var messageBox = new Window
                {
                    Title = "Delete Failed",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = "Failed to delete the selected site. See logs for details.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(20),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }
                };

                await messageBox.ShowDialog(this);
            }
            else
            {
                // Reload list to ensure UI is consistent
                await _viewModel.LoadSitesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception deleting site: {ex}");
            var messageBox = new Window
            {
                Title = "Error",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = $"An error occurred while deleting the site:\n{ex.Message}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(20),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };

            await messageBox.ShowDialog(this);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnTitleBarPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // Allow dragging the window by the title bar
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseWindowButton(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
