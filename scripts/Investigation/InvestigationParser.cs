using dot_battle_signal_bus.scripts.Systems;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public enum InvestigationTokenType
    {
        Unknown,
        Topic,
        Location
    }

    public record InvestigationResolveResult(
        string Token,
        InvestigationTokenType Type,
        bool ExactMatch);

    public class InvestigationParser
    {
        private readonly HashSet<string> _topics = new();
        private readonly HashSet<string> _locations = new();
        private readonly HashSet<string> _evidence = new();
        private readonly Dictionary<string, string> _tokenToId = new();

        private bool clueAdded = false;

        public bool Learn(string token)
        {
            token = Normalize(token);

            if (string.IsNullOrWhiteSpace(token))
                return false;

            return token[0] switch
            {
                '@' when !_locations.Contains(token) => _locations.Add(token),

                '!' when !_evidence.Contains(token) => _evidence.Add(token),

                _ when !_topics.Contains(token) => _topics.Add("#" + token)
            };
        }

        public IEnumerable<string> Topics
    => _topics.OrderBy(x => x);

        public IEnumerable<string> Locations
            => _locations.OrderBy(x => x);

        public IEnumerable<string> Evidence
            => _evidence.OrderBy(x => x);

        public void Learn(IEnumerable<string> tokens)
        {
            bool added = false;
            foreach (var token in tokens)
            {
                added =  Learn(token);
            }
            clueAdded = added;
        }

        public InvestigationResolveResult Resolve(string input)
        {
            input = Normalize(input);

            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (input.StartsWith("#"))
            {
                var match = ResolveFromSet(input, _topics);
                return match == null
                    ? null
                    : new(match,
                          InvestigationTokenType.Topic,
                          match == input);
            }

            if (input.StartsWith("@"))
            {
                var match = ResolveFromSet(input, _locations);
                return match == null
                    ? null
                    : new(match,
                          InvestigationTokenType.Location,
                          match == input);
            }

            return null;
        }

        public void RegisterTag(ClueEntry clue)
        {
            _tokenToId[Normalize(clue.ClueId)] =
                clue.ClueId;

            Learn(clue.Topics);

            //// register the clue if it's a location
            //if (clue.Kind == ClueKind.Coord)
            //{
            //   clueAdded = Learn(clue.ClueId.Replace("@", "#"));
            //}

           //  clueAdded = Learn($"!{clue.ClueId}");

            if (clueAdded)
            {
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.InvestigationAdded,
                    clue.ClueId,
                    clue.DisplayText);

                StoryManager.Instance.RegisterClue(clue);
            }
            else
            {
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.InvestigationAlreadyKnown,
                    clue.ClueId);
            }
        }

        public IEnumerable<string> Suggestions(string partial)
        {
            partial = Normalize(partial);


            if (partial.StartsWith("#"))
            {
                if (partial.Length == 1)
                {
                    return _topics
                        .OrderBy(t => t);
                }

                return _topics
                    .Where(t => t.StartsWith(partial))
                    .OrderBy(t => t);
            }

            if (partial.StartsWith("@"))
            {
                if (partial.Length == 1)
                {
                    return _locations
                        .OrderBy(l => l);
                }

                return _locations
                    .Where(l => l.StartsWith(partial))
                    .OrderBy(l => l);
            }

            return Enumerable.Empty<string>();
        }

        private string ResolveFromSet(
            string token,
            IEnumerable<string> source)
        {
            if (source.Contains(token))
                return token;

            return NearestKnown(token, source);
        }

        private string NearestKnown(
            string token,
            IEnumerable<string> source)
        {
            string best = null;
            int bestDistance = int.MaxValue;

            foreach (var candidate in source)
            {
                int distance =
                    Levenshtein(token, candidate);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return bestDistance <= 2
                ? best
                : null;
        }

        private static string Normalize(string s)
            => s?.Trim().ToLowerInvariant();

        private static int Levenshtein(
            string a,
            string b)
        {
            var d =
                new int[a.Length + 1,
                        b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= b.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost =
                        a[i - 1] == b[j - 1] ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(
                            d[i - 1, j] + 1,
                            d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[a.Length, b.Length];
        }
    }
}