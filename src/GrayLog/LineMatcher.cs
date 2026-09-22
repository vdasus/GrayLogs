using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GrayLog
{
    /// <summary>
    /// Decides which lines are dimmed. A line is dimmed when a rule matches it, or when it belongs to
    /// a statement that starts on a matching line and continues until its closing parenthesis.
    /// </summary>
    internal static class LineMatcher
    {
        /// <summary>Maximum number of lines a matched statement may span after its start line.</summary>
        public const int MaxStatementLines = 30;

        /// <summary>Returns the zero-based style slot for the line, or -1 when the line is not dimmed.</summary>
        public static int GetSlot(IReadOnlyList<Rule> rules, Func<int, string> getLine, int lineCount, int line)
        {
            return GetSlots(rules, getLine, lineCount, line, line)[0];
        }

        /// <summary>
        /// Returns the zero-based style slot for every line in [firstLine, lastLine] (-1 = not dimmed).
        /// Each line is read and matched at most once per call; statements starting up to
        /// <see cref="MaxStatementLines"/> lines above firstLine are taken into account.
        /// </summary>
        public static int[] GetSlots(IReadOnlyList<Rule> rules, Func<int, string> getLine, int lineCount, int firstLine, int lastLine)
        {
            var slots = new int[lastLine - firstLine + 1];
            for (var i = 0; i < slots.Length; i++) slots[i] = -1;
            if (rules.Count == 0) return slots;

            // Line texts are shared between the start-line scan and the parenthesis scan.
            var cache = new Dictionary<int, string>();
            string GetLine(int number)
            {
                if (!cache.TryGetValue(number, out var text)) cache[number] = text = getLine(number);
                return text;
            }

            // Ascending order: a later start line overrides an earlier one, so the nearest statement wins.
            for (var start = Math.Max(0, firstLine - MaxStatementLines); start <= lastLine; start++)
            {
                var match = FindMatch(rules, GetLine(start), out var slot);
                if (match == null) continue;

                var end = GetStatementEnd(GetLine, lineCount, start, match.Index);
                for (var line = Math.Max(start, firstLine); line <= Math.Min(end, lastLine); line++)
                {
                    slots[line - firstLine] = slot;
                }
            }

            return slots;
        }

        private static Match FindMatch(IReadOnlyList<Rule> rules, string text, out int slot)
        {
            foreach (var rule in rules)
            {
                try
                {
                    var match = rule.Pattern.Match(text);
                    if (match.Success)
                    {
                        slot = rule.Slot;
                        return match;
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // A pathological pattern must not break the editor; treat as no match.
                }
            }

            slot = -1;
            return null;
        }

        /// <summary>
        /// Returns the line where the statement starting at (startLine, startColumn) ends: the line of the
        /// parenthesis that balances the first '(' after the match. Parentheses inside string/char literals and
        /// after "//" are ignored. Returns startLine when no parenthesis follows on the start line or the
        /// statement stays unbalanced for more than <see cref="MaxStatementLines"/> lines.
        /// </summary>
        internal static int GetStatementEnd(Func<int, string> getLine, int lineCount, int startLine, int startColumn)
        {
            var depth = 0;
            var seenOpen = false;
            var inVerbatim = false;
            var lastLine = Math.Min(lineCount - 1, startLine + MaxStatementLines);

            for (var lineNumber = startLine; lineNumber <= lastLine; lineNumber++)
            {
                var text = getLine(lineNumber);
                var quote = '\0';

                for (var i = lineNumber == startLine ? startColumn : 0; i < text.Length; i++)
                {
                    var c = text[i];
                    var next = i + 1 < text.Length ? text[i + 1] : '\0';

                    if (inVerbatim)
                    {
                        if (c == '"')
                        {
                            if (next == '"') i++;
                            else inVerbatim = false;
                        }
                        continue;
                    }

                    if (quote != '\0')
                    {
                        if (c == '\\') i++;
                        else if (c == quote) quote = '\0';
                        continue;
                    }

                    switch (c)
                    {
                        case '/' when next == '/':
                            i = text.Length;
                            break;
                        case '@' when next == '"':
                            inVerbatim = true;
                            i++;
                            break;
                        case '$' when next == '@' || next == '"':
                        case '@' when next == '$':
                            // $"..." is a regular string; $@"..." and @$"..." are verbatim.
                            if (next == '"')
                            {
                                quote = '"';
                                i++;
                            }
                            else if (i + 2 < text.Length && text[i + 2] == '"')
                            {
                                inVerbatim = true;
                                i += 2;
                            }
                            break;
                        case '"':
                        case '\'':
                            quote = c;
                            break;
                        case '(':
                            depth++;
                            seenOpen = true;
                            break;
                        case ')':
                            depth--;
                            if (depth <= 0) return lineNumber;
                            break;
                        case ';' when depth <= 0:
                            return lineNumber;
                    }
                }

                if (!seenOpen) return startLine;
            }

            return startLine;
        }
    }
}
