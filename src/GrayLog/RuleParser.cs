using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GrayLog
{
    internal sealed class Rule
    {
        public Rule(Regex pattern, int slot)
        {
            Pattern = pattern;
            Slot = slot;
        }

        public Regex Pattern { get; }

        /// <summary>Zero-based style slot index.</summary>
        public int Slot { get; }
    }

    /// <summary>
    /// Parses the rules text: one regex per line, optional "N: " prefix selects style slot N,
    /// lines starting with '#' and empty lines are ignored.
    /// </summary>
    internal static class RuleParser
    {
        public const int SlotCount = 3;

        public const string DefaultRules =
            "# One regex per line. Optional prefix \"N: \" selects style slot 1..3 (default 1).\n" +
            "# Use (?i) for case-insensitive matching. Lines starting with '#' are comments.\n" +
            @"(?i)\b_?log(ger)?\??\." + "\n" +
            @"# 2: \bConsole\.Write(Line)?\(" + "\n" +
            @"# 2: \bDebug\.(Write|Assert)";

        private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(50);

        public static List<Rule> Parse(string text, List<string> errors = null)
        {
            var rules = new List<Rule>();
            var lines = (text ?? string.Empty).Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#') continue;

                var slot = 0;
                if (line.Length >= 2 && line[1] == ':' && line[0] >= '1' && line[0] < '1' + SlotCount)
                {
                    slot = line[0] - '1';
                    line = line.Substring(2).TrimStart();
                }

                try
                {
                    rules.Add(new Rule(new Regex(line, RegexOptions.CultureInvariant, MatchTimeout), slot));
                }
                catch (ArgumentException ex)
                {
                    errors?.Add($"Line {i + 1}: {ex.Message}");
                }
            }

            return rules;
        }
    }
}
