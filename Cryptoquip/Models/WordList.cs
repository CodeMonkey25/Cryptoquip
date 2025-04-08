using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    private readonly FrozenDictionary<char[],string[]> _words;

    public WordList()
    {
        IEqualityComparer<char[]> comparer = new ArrayEqualityComparer<char>();
        
        _words = File.ReadLines(DictionaryFileName)
            .AsParallel()
            .GroupBy(Word.MakePattern, comparer)
            .ToFrozenDictionary(static g => g.Key, static g => g.ToArray(), comparer);
    }

    public string[] GetMatches(Word word)
    {
        return _words.GetValueOrDefault(word.Pattern, Array.Empty<string>());
    }

    public string[] GetMatches(Word word, DecoderRingAbstract ring)
    {
        return _words
            .GetValueOrDefault(word.Pattern, Array.Empty<string>())
            .Where(w => ring.Matches(word.Text, w))
            .ToArray();
    }
}
