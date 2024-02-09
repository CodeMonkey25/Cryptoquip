using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Cryptoquip.Views;

public partial class LoadPuzzleWindow : Window
{
    public static readonly DirectProperty<LoadPuzzleWindow, string> PuzzleTextProperty = AvaloniaProperty.RegisterDirect<LoadPuzzleWindow, string>(nameof(PuzzleText), o => o.PuzzleText, (o, v) => o.PuzzleText = v);
    private string _puzzleText = "VE TEW ZWSSEZX PAGP GHT ESAPAGKDEKENFZP'Z SBXQXBBXV UBXGCQGZP MXBXGK DFNAP UX MEBHXG QKGCXZ? <HINT>: T=Y";
    public string PuzzleText
    {
        get => _puzzleText;
        set => SetAndRaise(PuzzleTextProperty, ref _puzzleText, value);
    }
    public LoadPuzzleWindow()
    {
        InitializeComponent();
    }

    private void OpenButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(PuzzleText);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void PuzzleTextBox_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PuzzleTextBox.Focus();
        PuzzleTextBox.SelectAll();
    }
}