using App.Metrics;
using FluentAssertions;
using Serilog.Events;
using Xunit;

namespace AppMetricsSerilogReporter.Tests
{
    public class SerilogMetricsReporterOptionsTests
    {
        private readonly SerilogMetricsReporterOptions _options;

        public SerilogMetricsReporterOptionsTests()
        {
            _options = new SerilogMetricsReporterOptions();
        }

        [Fact]
        public void DefaultLogEventLevelIsDebug()
        {
            _options.LogEventLevel.Should().Be(LogEventLevel.Debug);
        }

        [Fact]
        public void DefaultFlushIntervalIsAppMeticsConstant()
        {
            _options.FlushInterval.Should().Be(AppMetricsConstants.Reporting.DefaultFlushInterval);
        }

        [Fact]
        public void DefaultFilterIsNull()
        {
            _options.Filter.Should().BeNull();
        }

        [Fact]
        public void DefaultTemplatesIsNotNull()
        {
            _options.MessageTemplates.Should().NotBeNull();
        }
    }
}
