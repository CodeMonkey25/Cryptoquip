using Cryptoquip.Services;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    private readonly Dictionary<char[],string[]> _words;

    public WordList()
    {
        IEqualityComparer<char[]> comparer = new ArrayEqualityComparer<char>();
        
        _words = File.ReadAllLines(DictionaryFileName)
            // .AsParallel()
            // .WithMergeOptions(ParallelMergeOptions.NotBuffered)
            .GroupBy(Word.MakePattern, comparer)
            .ToDictionary(static g => g.Key, static g => g.ToArray(), comparer);
    }

    public string[] GetMatches(Word word)
    {
        return _words.GetValueOrDefault(word.Pattern, []);
    }

    public string[] GetMatches(Word word, DecoderRingAbstract ring)
    {
        return _words
            .GetValueOrDefault(word.Pattern, [])
            .Where(w => ring.Matches(word.Text, w))
            .ToArray();
    }
}
