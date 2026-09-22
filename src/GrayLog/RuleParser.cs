using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrayLog
{
    /// <summary>A rule as the user edits it in the options page.</summary>
    public sealed class RuleDefinition
    {
        public bool Enabled { get; set; } = true;

        public string Name { get; set; } = "";

        public string Pattern { get; set; } = "";

        public bool IgnoreCase { get; set; } = true;

        /// <summary>One-based style slot, 1..<see cref="RuleParser.SlotCount"/>.</summary>
        public int Style { get; set; } = 1;

        public RuleDefinition Clone() => (RuleDefinition)MemberwiseClone();
    }

    /// <summary>A compiled, enabled rule used by <see cref="LineMatcher"/>.</summary>
    internal sealed class Rule
    {
        public Rule(string name, Regex pattern, int slot)
        {
            Name = name;
            Pattern = pattern;
            Slot = slot;
        }

        public string Name { get; }

        public Regex Pattern { get; }

        /// <summary>Zero-based style slot index.</summary>
        public int Slot { get; }
    }

    /// <summary>Default rules, storage format and compilation of rule definitions.</summary>
    internal static class RuleParser
    {
        public const int SlotCount = 3;

        private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(50);

        public static List<RuleDefinition> CreateDefaults() => new List<RuleDefinition>
        {
            new RuleDefinition { Name = "Logger calls (log., logger., _logger., Log?.)", Pattern = @"\b_?log(ger)?\??\." },
            new RuleDefinition { Enabled = false, Name = "Console output", Pattern = @"\bConsole\.Write(Line)?\(", IgnoreCase = false, Style = 2 },
            new RuleDefinition { Enabled = false, Name = "Debug / Trace output", Pattern = @"\b(Debug|Trace)\.(Write|WriteLine|Print|Assert|Fail)\w*\(", IgnoreCase = false, Style = 2 },
        };

        /// <summary>Compiles enabled rules; invalid patterns are skipped and reported in <paramref name="errors"/>.</summary>
        public static List<Rule> Compile(IEnumerable<RuleDefinition> definitions, List<string> errors = null)
        {
            var rules = new List<Rule>();
            var number = 0;
            foreach (var definition in definitions)
            {
                number++;
                if (!definition.Enabled || string.IsNullOrWhiteSpace(definition.Pattern)) continue;

                var options = RegexOptions.CultureInvariant | (definition.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                var slot = Math.Min(Math.Max(definition.Style, 1), SlotCount) - 1;
                try
                {
                    rules.Add(new Rule(definition.Name, new Regex(definition.Pattern, options, MatchTimeout), slot));
                }
                catch (ArgumentException ex)
                {
                    errors?.Add($"Rule {number} ({definition.Name}): {ex.Message}");
                }
            }

            return rules;
        }

        /// <summary>One rule per line: enabled, style, ignore case, name, pattern — separated by tabs.</summary>
        public static string Serialize(IEnumerable<RuleDefinition> definitions)
        {
            return string.Join("\n", definitions.Select(d => string.Join("\t",
                d.Enabled ? "1" : "0",
                d.Style.ToString(),
                d.IgnoreCase ? "1" : "0",
                Clean(d.Name).Replace('\t', ' '),
                // A literal tab in a regex is equivalent to the \t escape.
                Clean(d.Pattern).Replace("\t", @"\t"))));
        }

        public static List<RuleDefinition> Deserialize(string text)
        {
            var definitions = new List<RuleDefinition>();
            foreach (var line in (text ?? "").Split('\n'))
            {
                var fields = line.TrimEnd('\r').Split(new[] { '\t' }, 5);
                if (fields.Length != 5) continue;

                int.TryParse(fields[1], out var style);
                definitions.Add(new RuleDefinition
                {
                    Enabled = fields[0] == "1",
                    Style = style,
                    IgnoreCase = fields[2] == "1",
                    Name = fields[3],
                    Pattern = fields[4],
                });
            }

            return definitions;
        }

        private static string Clean(string value) => (value ?? "").Replace("\r", "").Replace("\n", " ");
    }
}
