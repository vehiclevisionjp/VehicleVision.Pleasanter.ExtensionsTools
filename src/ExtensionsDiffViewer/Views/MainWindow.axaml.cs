using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.ViewModels;

namespace VehicleVision.Pleasanter.ExtensionsTools.DiffViewer.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        ConfigureEditor(ServerEditor);
        ConfigureEditor(LocalEditor);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != DataContextProperty)
        {
            return;
        }

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = change.NewValue as MainWindowViewModel;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private static void ConfigureEditor(TextEditor editor)
    {
        editor.Options.HighlightCurrentLine = true;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MainWindowViewModel vm)
        {
            return;
        }

        if (e.PropertyName is nameof(MainWindowViewModel.ServerContentText)
            or nameof(MainWindowViewModel.LocalContentText)
            or nameof(MainWindowViewModel.SelectedItem))
        {
            UpdateEditors(vm);
        }
    }

    private void UpdateEditors(MainWindowViewModel vm)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ServerEditor.Text = vm.ServerContentText;
            LocalEditor.Text = vm.LocalContentText;
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ServerEditor.Text = vm.ServerContentText;
                LocalEditor.Text = vm.LocalContentText;
            });
        }
    }
}
