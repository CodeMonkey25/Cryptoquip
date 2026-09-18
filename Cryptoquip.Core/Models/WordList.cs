using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    internal const int MaxWordLength = 50;
    private readonly Dictionary<string,List<string>> _words = new();

    public WordList(HashSet<string>? patterns = null)
    {
        HashSet<int> lengths = [];
        int maxPatternLength = WordList.MaxWordLength;
        if (patterns != null && patterns.Count > 0)
        {
            maxPatternLength = 0;
            foreach (string p in patterns)
            {
                lengths.Add(p.Length);
                if (p.Length > maxPatternLength) maxPatternLength = p.Length;
            }
        }
        
        Parallel.ForEach(
            File.ReadLines(DictionaryFileName),
            () => new ThreadState(patterns, lengths, maxPatternLength),
            static (word, _, threadState) =>
            {
                if (word.Length < 1) return threadState;
                if (word.Length > threadState.PatternBuffer.Length) return threadState;
                if (threadState.Patterns != null && !threadState.Lengths.Contains(word.Length)) return threadState;
                
                string pattern = Word.MakePattern(word, threadState.PatternBuffer.AsSpan(0, word.Length), threadState.LetterBuffer, threadState.TouchedBuffer);
                if (threadState.Patterns != null && !threadState.Patterns.Contains(pattern)) return threadState;
                
                if (threadState.PatternMap.TryGetValue(pattern, out List<string>? list))
                {
                    list.Add(word);
                }
                else
                {
                    threadState.PatternMap.Add(pattern, [word,]);
                }
                return threadState;
            },
            (threadState) =>
            {
                lock (_words)
                {
                    foreach ((string pattern, List<string> words) in threadState.PatternMap)
                    {
                        if (_words.TryGetValue(pattern, out List<string>? mainList))
                        {
                            mainList.AddRange(words);
                        }
                        else
                        {
                            _words.Add(pattern, words);
                        }
                    }
                }
            }
        );
        
        _words.TrimExcess();
        foreach (List<string> value in _words.Values)
        {
            value.TrimExcess();
        }
        
        // int[] lengths = patterns.Select(pattern => pattern.Length).Distinct().ToArray();
        // Parallel.ForEach(File.ReadLines(DictionaryFileName), word =>
        // {
        //     if (!lengths.Contains(word.Length)) return;
        //     string pattern = Word.MakePattern(word);
        //     if (patterns.Contains(pattern))
        //     {
        //         lock(_words)
        //         {
        //             if (_words.TryGetValue(pattern, out List<string>? list))
        //             {
        //                 list.Add(word);
        //             }
        //             else
        //             {
        //                 _words.Add(pattern, [word,]);
        //             }
        //         }
        //     }
        // });

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
        if (!_words.TryGetValue(word.Pattern, out List<string>? candidates))
            return [];

        List<string> matches = new(candidates.Count);
        foreach (string w in candidates)
        {
            if (ring.Matches(word.Text, w)) matches.Add(w);
        }
        matches.TrimExcess();
        return matches;
    }
    
    private class ThreadState(IReadOnlySet<string>? patterns, HashSet<int> lengths, int maxPatternLength)
    {
        public readonly IReadOnlySet<string>? Patterns = patterns;
        public readonly HashSet<int> Lengths = lengths;
        public readonly Dictionary<string, List<string>> PatternMap = new(StringComparer.Ordinal);
        public readonly char[] PatternBuffer = new char[maxPatternLength];
        public readonly char[] LetterBuffer = new char[26];
        public readonly int[] TouchedBuffer = new int[26];
    }
}
