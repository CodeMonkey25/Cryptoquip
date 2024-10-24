using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Cryptoquip.Extensions;
using Cryptoquip.Utility;
using Cryptoquip.ViewModels;
using Splat;

namespace Cryptoquip.Views;

public partial class LetterView : UserControl
{
    public LetterView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is LetterViewModel vm)
        {
            vm.IsMouseOver = true;
        }
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is LetterViewModel vm)
        {
            vm.IsMouseOver = false;
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!(DataContext is LetterViewModel vm)) return;
        if (!char.IsLetter(vm.Letter)) return;
        if (vm.WasSetFromHint) return;
        
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();

        MenuFlyout flyout = new MenuFlyout()
        {
            ItemsSource = ring
                .GetUnusedLetters()
                .Prepend(' ')
                .Select(c=> new MenuItem { Header = c.ToString(), Command = vm.SolveLetterCommand, CommandParameter = c})
                .ToArray(),
            Placement = PlacementMode.Bottom
        };
        FlyoutBase.SetAttachedFlyout(this, flyout);
        FlyoutBase.ShowAttachedFlyout(this);
    }
}