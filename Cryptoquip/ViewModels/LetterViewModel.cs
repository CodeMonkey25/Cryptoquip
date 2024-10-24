using System.Reactive;
using System.Windows.Input;
using Cryptoquip.Extensions;
using Cryptoquip.Utility;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class LetterViewModel : ViewModelBase
{
    private char _letter = 'A';
    public char Letter
    {
        get => _letter;
        set => this.RaiseAndSetIfChanged(ref _letter, value);
    }

    private char _decodedLetter = 'W';
    public char DecodedLetter
    {
        get => _decodedLetter;
        set => this.RaiseAndSetIfChanged(ref _decodedLetter, value);
    }

    private bool _isMouseOver = false;
    public bool IsMouseOver
    {
        get => _isMouseOver;
        set => this.RaiseAndSetIfChanged(ref _isMouseOver, value);
    }
    
    private bool _wasSetFromHint;
    public bool WasSetFromHint
    {
        get => _wasSetFromHint;
        set => this.RaiseAndSetIfChanged(ref _wasSetFromHint, value);
    }
    
    private ICommand _solveLetterCommand = NullCommand.Instance;
    public ICommand SolveLetterCommand
    {
        get => _solveLetterCommand;
        set => this.RaiseAndSetIfChanged(ref _solveLetterCommand, value);
    }

    public LetterViewModel()
    {
        SolveLetterCommand = ReactiveCommand.Create<char, Unit>(SolveLetter);
    }

    public LetterViewModel(char letter) : this()
    {
        Letter = letter;
        
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();
        DecodedLetter = ring.Get(letter);
        WasSetFromHint = ring.WasSetFromHint(letter);
    }

    public Unit SolveLetter(char decodedLetter)
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();

        if (decodedLetter == ' ')
            ring.Remove(Letter);
        else 
            ring.Put(Letter, decodedLetter);
        
        DecodedLetter = ring.Get(Letter);
        return Unit.Default;
    }

    public void Update()
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();
        DecodedLetter = ring.Get(Letter);
    }
}