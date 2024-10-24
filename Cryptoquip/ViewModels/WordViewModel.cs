using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Utility;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class WordViewModel : ViewModelBase
{
    private ObservableCollection<LetterViewModel> _letters = new();
    public ObservableCollection<LetterViewModel> Letters
    {
        get => _letters;
        set => this.RaiseAndSetIfChanged(ref _letters, value);
    }
    
    private bool _isMouseOver = false;
    public bool IsMouseOver
    {
        get => _isMouseOver;
        set => this.RaiseAndSetIfChanged(ref _isMouseOver, value);
    }

    private ICommand _solveWordCommand = NullCommand.Instance;
    public ICommand SolveWordCommand
    {
        get => _solveWordCommand;
        set => this.RaiseAndSetIfChanged(ref _solveWordCommand, value);
    }

    public Word Word { get; set; } = new Word("");

    public WordViewModel()
    {
        SolveWordCommand = ReactiveCommand.Create<string, Unit>(SolveWord);
    }

    public WordViewModel(string word, Dictionary<char, LetterViewModel> letterMap) : this()
    {
        foreach (char c in word)
        {
            LetterViewModel vm;
            if (letterMap.TryGetValue(c, out LetterViewModel? value))
            {
                vm = value;
            }
            else
            {
                vm = new LetterViewModel(c);
                letterMap.Add(c, vm);
            }

            Letters.Add(vm);
        }

        Word = new Word(Regex.Replace(word, "[^A-Z0-9']", " ").Trim());
    }
    
    public Unit SolveWord(string decodedWord)
    {
        DecoderRingAbstract ring = Locator.Current.GetRequiredService<DecoderRingAbstract>();

        foreach ((char l, char m) in Word.Text.Zip(decodedWord))
        {
            if (!char.IsLetter(l)) continue;
            if (ring.Contains(l)) continue;
            ring.Put(l, m);
            Letters.First(lvm => lvm.Letter == l).Update();
            // Letters.First(lvm => lvm.Letter == l).RaisePropertyChanged(nameof(LetterViewModel.DecodedLetter));
        }
        
        return Unit.Default;
    }
}