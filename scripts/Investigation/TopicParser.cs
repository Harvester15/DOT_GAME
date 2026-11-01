using System.Collections.Generic;
using System.Linq;


namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Option A questioning: the player types a topic. The parser only resolves topics
    /// the player has ENCOUNTERED, so questioning is open but never cryptic.
    /// Supports the '#' lookup: Marked clues become quick-ask prompts (Option C bridge).
    /// </summary>
    public class TopicParser
    {
        private readonly HashSet<string> _knownTopics = new();

        // '#tag' -> clueId, so a Marked clue can be asked without retyping.
        private readonly Dictionary<string, string> _tagToClueId = new();

        public void Learn(IEnumerable<string> topics)
        {
            foreach (var t in topics)
                _knownTopics.Add(Normalize(t));
        }

        /// <summary>Register a Marked clue as a '#tag' quick-ask prompt.</summary>
        public void RegisterTag(ClueEntry clue)
        {
            _tagToClueId[Normalize(clue.ClueId)] = clue.ClueId;
            Learn(clue.Topics);
        }

        /// <summary>All '#' suggestions for the autocomplete dropdown.</summary>
        public IEnumerable<string> TagSuggestions() => _tagToClueId.Keys.OrderBy(k => k);

        /// <summary>
        /// Resolves typed input to a topic id, or null if the player can't ask that yet.
        /// '#clue_id' resolves directly to a Marked clue's topic.
        /// </summary>
        public string Resolve(string rawInput)
        {
            var token = Normalize(rawInput);

            if (token.StartsWith("#"))
            {
                var tag = token.Substring(1);
                return _tagToClueId.TryGetValue(tag, out var clueId) ? clueId : null;
            }

            if (_knownTopics.Contains(token)) return token;

            // Forgive near-misses without hinting unknown topics.
            return NearestKnown(token);
        }

        private string NearestKnown(string token)
        {
            string best = null;
            int bestDist = int.MaxValue;
            foreach (var known in _knownTopics)
            {
                int d = Levenshtein(token, known);
                if (d < bestDist) { bestDist = d; best = known; }
            }
            // Only accept a tight match (typo), never a wild guess.
            return bestDist <= 2 ? best : null;
        }

        private static string Normalize(string s) => s.Trim().ToLowerInvariant();

        private static int Levenshtein(string a, string b)
        {
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = System.Math.Min(
                        System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            return d[a.Length, b.Length];
        }
    }
}
