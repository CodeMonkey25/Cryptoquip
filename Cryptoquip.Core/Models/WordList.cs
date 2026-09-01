using Cryptoquip.Services;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    private readonly Dictionary<string,List<string>> _words = new();

    public WordList(HashSet<string>? patterns = null)
    {
        Parallel.ForEach(File.ReadLines(DictionaryFileName), word =>
        {
            string pattern = Word.MakePattern(word);
            if (patterns == null || patterns.Contains(pattern))
            {
                lock(_words)
                {
                    if (!_words.TryGetValue(pattern, out List<string>? list))
                    {
                        _words[pattern] = list = [];
                    }
                    list.Add(word);
                }
            }
        });

        // foreach (string word in File.ReadLines(WordList.DictionaryFileName))
        // {
        //     string pattern = Word.MakePattern(word);
        //     if (patterns == null || patterns.Contains(pattern))
        //     {
        //         if (!_words.TryGetValue(pattern, out List<string>? list))
        //         {
        //             _words[pattern] = list = [];
        //         }
        //         list.Add(word);
        //     }
        // }
        
        // IEqualityComparer<char[]> comparer = new ArrayEqualityComparer<char>();
        // _words = File.ReadLines(DictionaryFileName)
        //     .AsParallel()
        //     // .WithMergeOptions(ParallelMergeOptions.NotBuffered) // this makes the words not in alphabetic order
        //     .Select(static w => new { Word = w, Pattern = Word.MakePattern(w)})
        //     .Where(w => patterns == null || patterns.Contains(w.Pattern))
        //     .GroupBy(static w => w.Pattern, comparer)
        //     .ToDictionary(static g => g.Key, static g => g.Select(w => w.Word).ToArray(), comparer);
    }

    public List<string> GetMatches(Word word, DecoderRing ring)
    {
        return _words
            .GetValueOrDefault(word.Pattern, [])
            .Where(w => ring.Matches(word.Text, w))
            .ToList();
    }
}
