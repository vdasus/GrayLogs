using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace GrayLog.Tests
{
    public class RuleParserTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void Parse_SkipsCommentsAndEmptyLines()
        {
            var rules = RuleParser.Parse("# comment\n\n   \nlog\\.\r\n");

            rules.Should().ContainSingle().Which.Pattern.ToString().Should().Be("log\\.");
        }

        [Theory]
        [Trait("Category", "Unit")]
        [InlineData("log\\.", 0, "log\\.")]
        [InlineData("1: log\\.", 0, "log\\.")]
        [InlineData("2: log\\.", 1, "log\\.")]
        [InlineData("3:log\\.", 2, "log\\.")]
        [InlineData("4: log\\.", 0, "4: log\\.")]
        public void Parse_ReadsSlotPrefix(string line, int expectedSlot, string expectedPattern)
        {
            var rule = RuleParser.Parse(line).Should().ContainSingle().Subject;

            rule.Slot.Should().Be(expectedSlot);
            rule.Pattern.ToString().Should().Be(expectedPattern);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Parse_ReportsInvalidRegexWithLineNumber()
        {
            var errors = new List<string>();

            var rules = RuleParser.Parse("log\\.\n# comment\n(unclosed", errors);

            rules.Should().ContainSingle();
            errors.Should().ContainSingle().Which.Should().StartWith("Line 3:");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DefaultRules_ParseWithoutErrors()
        {
            var errors = new List<string>();

            var rules = RuleParser.Parse(RuleParser.DefaultRules, errors);

            errors.Should().BeEmpty();
            rules.Should().ContainSingle();
        }
    }
}
