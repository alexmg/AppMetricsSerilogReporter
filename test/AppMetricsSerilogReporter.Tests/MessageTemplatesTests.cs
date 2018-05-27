using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace AppMetricsSerilogReporter.Tests
{
    public class MessageTemplatesTests
    {
        [Theory]
        [MemberData(nameof(GetMessageTemplates))]
        public void MessageTemplatesShouldContainContext(string messageTemplate)
        {
            messageTemplate.Should().Contain("{context}");
        }

        [Theory]
        [MemberData(nameof(GetMessageTemplates))]
        public void MessageTemplatesShouldContainName(string messageTemplate)
        {
            messageTemplate.Should().Contain("{name}");
        }

        public static IEnumerable<object[]> GetMessageTemplates()
        {
            var messageTemplates = new MessageTemplates();

            yield return new object[] {messageTemplates.Counter};
            yield return new object[] {messageTemplates.CounterItem};
            yield return new object[] {messageTemplates.Fallback};
            yield return new object[] {messageTemplates.Gauge};
            yield return new object[] {messageTemplates.Histogram};
            yield return new object[] {messageTemplates.Meter};
            yield return new object[] {messageTemplates.MeterItem};
            yield return new object[] {messageTemplates.Timer};
        }
    }
}
