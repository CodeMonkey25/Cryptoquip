using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Cryptoquip.Views;

public partial class LoadPuzzleWindow : Window
{
    public static readonly DirectProperty<LoadPuzzleWindow, string> PuzzleTextProperty = AvaloniaProperty.RegisterDirect<LoadPuzzleWindow, string>(nameof(PuzzleText), o => o.PuzzleText, (o, v) => o.PuzzleText = v);
    // private string _puzzleText = "VE TEW ZWSSEZX PAGP GHT ESAPAGKDEKENFZP'Z SBXQXBBXV UBXGCQGZP MXBXGK DFNAP UX MEBHXG QKGCXZ? <HINT>: T=Y";
    // private string _puzzleText = "CIY JVO J TBNQL IG ZBO YDNKOVMO, DI QOMM ZBJD ZBO AIID JDL ZBO MZJVM; CIY BJKO J VNEBZ ZI HO BOVO. JDL RBOZBOV IV DIZ NZ NM TQOJV ZI CIY, DI LIYHZ ZBO YDNKOVMO NM YDGIQLNDE JM NZ MBIYQL. - AJS OBVAJDD";
    private string _puzzleText = "X KYP Y DSSW SF EYEEG'K BYVU LSEYG YFE YDD SB Y KHEEUF X BSHFE CGKUDB DSSWXFN YL RXC FSL YK CG EYEEG OHL YK YFSLRUZ CYF. CYGOU X'C NZSPXFN HQ Y DXLLDU. CYGOU XL PYK LRU QYXF XF RXK BYVU. OHL XL VYCU LS CU LRYL BSZ YDD LRU QDUYKHZU CSCCY YFE EYEEG BXFE XF HK VRXDEZUF, LRUZU YZU SLRUZ LXCUK XL CHKL OU RUYZLOZUYWXFN LS LRUC. <HINT>:EYEEG'K=DADDY'S";
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