using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace GrayLog.Tests
{
    public class RuleParserTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void Defaults_HaveOneEnabledLoggerRule()
        {
            var errors = new List<string>();

            var rules = RuleParser.Compile(RuleParser.CreateDefaults(), errors);

            errors.Should().BeEmpty();
            rules.Should().ContainSingle().Which.Name.Should().StartWith("Logger calls");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Compile_SkipsDisabledAndEmptyRules()
        {
            var definitions = new[]
            {
                new RuleDefinition { Pattern = "a" },
                new RuleDefinition { Pattern = "b", Enabled = false },
                new RuleDefinition { Pattern = "  " },
            };

            RuleParser.Compile(definitions).Select(r => r.Pattern.ToString()).Should().Equal("a");
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(1, 0)]
        [InlineData(3, 2)]
        [InlineData(0, 0)]
        [InlineData(9, 2)]
        public void Compile_ClampsStyleToSlot(int style, int expectedSlot)
        {
            RuleParser.Compile(new[] { new RuleDefinition { Pattern = "a", Style = style } })
                .Single().Slot.Should().Be(expectedSlot);
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(true, "LOG.Info", true)]
        [InlineData(false, "LOG.Info", false)]
        [InlineData(false, "log.Info", true)]
        public void Compile_AppliesIgnoreCase(bool ignoreCase, string text, bool expected)
        {
            var rule = RuleParser.Compile(new[] { new RuleDefinition { Pattern = @"log\.", IgnoreCase = ignoreCase } }).Single();

            rule.Pattern.IsMatch(text).Should().Be(expected);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Compile_ReportsInvalidPatternWithRuleNumber()
        {
            var errors = new List<string>();
            var definitions = new[]
            {
                new RuleDefinition { Pattern = "a" },
                new RuleDefinition { Name = "Broken", Pattern = "(unclosed" },
            };

            RuleParser.Compile(definitions, errors).Should().ContainSingle();
            errors.Should().ContainSingle().Which.Should().StartWith("Rule 2 (Broken):");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SerializeDeserialize_RoundTrips()
        {
            var definitions = RuleParser.CreateDefaults();
            definitions.Add(new RuleDefinition { Name = "Tab\tand\nnewline", Pattern = "a\tb|c", IgnoreCase = false, Style = 3 });

            var restored = RuleParser.Deserialize(RuleParser.Serialize(definitions));

            restored.Should().HaveCount(4);
            restored.Take(3).Should().BeEquivalentTo(RuleParser.CreateDefaults());
            restored[3].Should().BeEquivalentTo(new RuleDefinition
            {
                Name = "Tab and newline", Pattern = @"a\tb|c", IgnoreCase = false, Style = 3,
            });
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Deserialize_SkipsMalformedLines()
        {
            RuleParser.Deserialize("garbage\n\n1\t1\t1\tName\tlog\\.").Should().ContainSingle()
                .Which.Pattern.Should().Be(@"log\.");
        }
    }
}
