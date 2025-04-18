using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Services;
using Cryptoquip.ViewModels;
using Splat;

namespace Cryptoquip.Views;

public partial class WordView : UserControl
{
    public WordView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is WordViewModel vm)
        {
            if (vm.Word.IsSolvable)
            {
                vm.IsMouseOver = true;
            }
        }
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is WordViewModel vm)
        {
            vm.IsMouseOver = false;
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!(DataContext is WordViewModel vm)) return;
        
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();
        WordList words = Locator.Current.GetRequiredService<WordList>();
        string[] candidates = words.GetMatches(vm.Word, ring);

        MenuFlyout flyout = new MenuFlyout()
        {
            ItemsSource = candidates
                .Take(20)
                .Prepend("-")
                .Select(w => new MenuItem { Header = w.ToString(), Command = vm.SolveWordCommand, CommandParameter = w })
                .Prepend(new MenuItem { Header = $"{candidates.Length} matches", IsEnabled = false })
                .ToArray(),
            Placement = PlacementMode.Bottom
        };
        FlyoutBase.SetAttachedFlyout(this, flyout);
        FlyoutBase.ShowAttachedFlyout(this);
    }
}