using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cryptoquip.Extensions;
using Cryptoquip.Models;
using Cryptoquip.Services;
using ReactiveUI;
using Splat;

namespace Cryptoquip.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<WordViewModel> Words
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public Puzzle? Puzzle { get; private set; }

    public void LoadPuzzle(string text)
    {
        DecoderRing ring = Locator.Current.GetRequiredService<DecoderRing>();
        ring.Clear();
        
        Puzzle = Puzzle.Parse(text);
        ring.Put(Puzzle.Hints);
        
        Words.Clear();
        Dictionary<char, LetterViewModel> letterMap = new();
        HashSet<char> hints = Puzzle.Hints.Keys.ToHashSet();
        foreach (ReadOnlyMemory<char> word in Puzzle.GetAllWords())
        {
            Words.Add(new WordViewModel(new string(word.Span), letterMap, hints));
        }
    }
}