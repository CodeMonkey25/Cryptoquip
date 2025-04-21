using Cryptoquip.Services;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    private readonly Dictionary<char[],string[]> _words;

    public WordList(HashSet<char[]>? patterns = null)
    {
        IEqualityComparer<char[]> comparer = new ArrayEqualityComparer<char>();
        
        _words = File.ReadLines(DictionaryFileName)
            .AsParallel()
            // .WithMergeOptions(ParallelMergeOptions.NotBuffered) // this makes the words not in alphabetic order
            .Select(static w => new { Word = w, Pattern = Word.MakePattern(w)})
            .Where(w => patterns == null || patterns.Contains(w.Pattern))
            .GroupBy(static w => w.Pattern, comparer)
            .ToDictionary(static g => g.Key, static g => g.Select(w => w.Word).ToArray(), comparer);
    }

    public string[] GetMatches(Word word, DecoderRingAbstract ring)
    {
        return _words
            .GetValueOrDefault(word.Pattern, [])
            .Where(w => ring.Matches(word.Text, w))
            .ToArray();
    }
}
