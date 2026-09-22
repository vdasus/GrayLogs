using System.Linq;
using FluentAssertions;
using Xunit;

namespace GrayLog.Tests
{
    public class LineMatcherTests
    {
        private static readonly Rule[] DefaultRules = RuleParser.Compile(RuleParser.CreateDefaults()).ToArray();

        /// <summary>Returns the slot of every line of the code, -1 for lines that are not dimmed.</summary>
        private static int[] Slots(string code, Rule[] rules = null)
        {
            var lines = code.Replace("\r", "").Split('\n');
            return Enumerable.Range(0, lines.Length)
                .Select(n => LineMatcher.GetSlot(rules ?? DefaultRules, i => lines[i], lines.Length, n))
                .ToArray();
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("log.Info(\"x\");")]
        [InlineData("_logger.LogInformation(\"x\");")]
        [InlineData("Logger?.Warn(\"x\");")]
        [InlineData("    _log.Debug(x);")]
        public void DefaultRules_MatchLoggingCalls(string line)
        {
            Slots(line).Should().Equal(0);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("var catalog = new Catalog();")]
        [InlineData("dialog.Show();")]
        [InlineData("var logger = factory.CreateLogger();")]
        public void DefaultRules_IgnoreOtherCode(string line)
        {
            Slots(line).Should().Equal(-1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MultiLineCall_IsDimmedUntilClosingParenthesis()
        {
            const string code = """
                _logger.LogInformation(
                    "Order {Id} processed",
                    order.Id);
                Process(order);
                """;

            Slots(code).Should().Equal(0, 0, 0, -1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Parentheses_InLiteralsAndComments_AreIgnored()
        {
            const string code = """
                _logger.LogInformation(")(( {0}", ')', // (((
                    @"verbatim ""("" (",
                    $"interpolated ( {x}", $@"both (" );
                Process(order);
                """;

            Slots(code).Should().Equal(0, 0, 0, -1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CallWithoutParenthesis_DimsOnlyItsLine()
        {
            const string code = """
                log.Level = Level.Debug;
                Process(order);
                """;

            Slots(code).Should().Equal(0, -1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MatchInsideCondition_EndsAtOuterClosingParenthesis()
        {
            const string code = """
                if (log.IsDebugEnabled)
                    Process(order);
                """;

            Slots(code).Should().Equal(0, -1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UnbalancedStatement_DimsOnlyStartLine()
        {
            var code = "_logger.LogInformation(\n" + string.Join("\n", Enumerable.Repeat("x,", LineMatcher.MaxStatementLines + 5));

            var slots = Slots(code);

            slots[0].Should().Be(0);
            slots.Skip(1).Should().OnlyContain(slot => slot == -1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void FirstMatchingRule_DefinesSlot()
        {
            var rules = RuleParser.Compile(new[]
            {
                new RuleDefinition { Pattern = @"Console\.", Style = 2 },
                new RuleDefinition { Pattern = "Write", Style = 3 },
            }).ToArray();

            Slots("Console.WriteLine(\n  x);", rules).Should().Equal(1, 1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void NoRules_NothingIsDimmed()
        {
            Slots("log.Info(x);", new Rule[0]).Should().Equal(-1);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Category", "Performance")]
        public void GetSlots_ReadsEachLineOnce()
        {
            // Every line starts a 3-line logging call, the worst case for the parenthesis scan.
            var lines = Enumerable.Range(0, 1000)
                .Select(i => (i % 3) switch { 0 => "_logger.LogInformation(", 1 => "    \"text\",", _ => "    x);" })
                .ToArray();
            var reads = new int[lines.Length];

            LineMatcher.GetSlots(DefaultRules, i => { reads[i]++; return lines[i]; }, lines.Length, 500, 560);

            reads.Should().OnlyContain(count => count <= 1);
            reads.Sum().Should().BeLessThanOrEqualTo(61 + LineMatcher.MaxStatementLines + 3);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GetSlots_EqualsPerLineGetSlot()
        {
            const string code = """
                Process(order);
                _logger.LogInformation(
                    "a",
                    log.Format(x));
                if (log.IsDebugEnabled)
                    Console.WriteLine("(");
                log.Level = Level.Debug;
                """;
            var lines = code.Replace("\r", "").Split('\n');

            var slots = LineMatcher.GetSlots(DefaultRules, i => lines[i], lines.Length, 0, lines.Length - 1);

            slots.Should().Equal(Slots(code));
        }
    }
}
