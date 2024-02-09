using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    private readonly Dictionary<string,string[]> _words;

    public WordList()
    {
        _words = File.ReadLines(DictionaryFileName)
            .GroupBy(Word.MakePattern)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public string[] GetMatches(Word word)
    {
        return _words.GetValueOrDefault(word.Pattern, Array.Empty<string>());
    }

    public string[] GetMatches(Word word, DecoderRing ring)
    {
        return _words
            .GetValueOrDefault(word.Pattern, Array.Empty<string>())
            .Where(w => ring.Matches(word.Text, w))
            .ToArray();
    }
}
