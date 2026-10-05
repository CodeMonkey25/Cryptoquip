using System.Runtime.InteropServices;
using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class WordList
{
    private const string DictionaryFileName = @"dictionary.txt";
    internal const int MaxWordLength = 50;
    private readonly Dictionary<string,List<string>> _words = new();

    public WordList(HashSet<string>? patterns = null)
    {
        bool[]? lengths = null;
        int maxPatternLength = WordList.MaxWordLength;
        if (patterns != null && patterns.Count > 0)
        {
            maxPatternLength = patterns.Select(pattern => pattern.Length).Max();
            lengths = new bool[maxPatternLength + 1];
            foreach (string pattern in patterns) lengths[pattern.Length] = true;
        }
        
        Parallel.ForEach(
            File.ReadLines(Path.Combine(AppContext.BaseDirectory, WordList.DictionaryFileName)),
            () => new ThreadState(patterns, lengths, maxPatternLength, _words),
            static (word, _, threadState) =>
            {
                if (word.Length < 1) return threadState;
                if (word.Length > threadState.PatternBuffer.Length) return threadState;
                if (threadState.Lengths != null && !threadState.Lengths[word.Length]) return threadState;
                
                Span<char> patternSpan = threadState.PatternBuffer.AsSpan(0, word.Length);
                Word.WritePattern(word.AsSpan(), patternSpan, threadState.LetterBuffer, threadState.TouchedBuffer);

                if (threadState.PatternsLookup is { } patternsLookup && !patternsLookup.Contains(patternSpan)) return threadState;
                
                ref List<string>? list = ref CollectionsMarshal.GetValueRefOrAddDefault(threadState.PatternMapLookup, patternSpan, out bool _);
                (list ??= []).Add(word);
                
                return threadState;
            },
            static (threadState) =>
            {
                lock (threadState.MainDict)
                {
                    foreach ((string pattern, List<string> words) in threadState.PatternMap)
                    {
                        if (threadState.MainDict.TryGetValue(pattern, out List<string>? mainList))
                        {
                            mainList.AddRange(words);
                        }
                        else
                        {
                            threadState.MainDict.Add(pattern, words);
                        }
                    }
                }
            }
        );
        
        _words.TrimExcess();
        foreach (List<string> value in _words.Values)
        {
            value.TrimExcess();
            value.Sort(StringComparer.Ordinal);
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
        return matches;
    }
    
    private sealed class ThreadState
    {
        public readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>>? PatternsLookup;
        public readonly bool[]? Lengths;
        public readonly Dictionary<string, List<string>> PatternMap;
        public readonly Dictionary<string, List<string>>.AlternateLookup<ReadOnlySpan<char>> PatternMapLookup;
        public readonly char[] PatternBuffer;
        public readonly char[] LetterBuffer = new char[26];
        public readonly int[] TouchedBuffer = new int[26];
        public readonly Dictionary<string, List<string>> MainDict;

        public ThreadState(HashSet<string>? patterns, bool[]? lengths, int maxPatternLength, Dictionary<string, List<string>> mainDict)
        {
            PatternsLookup = patterns?.GetAlternateLookup<ReadOnlySpan<char>>();
            Lengths = lengths;
            PatternMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            PatternMapLookup = PatternMap.GetAlternateLookup<ReadOnlySpan<char>>();
            PatternBuffer = new char[maxPatternLength];
            MainDict = mainDict;
        }
    }
}
