using System;
using App.Metrics.Filters;
using App.Metrics.Formatters.Ascii;
using FluentAssertions;
using Moq;
using Xunit;

namespace AppMetricsSerilogReporter.Tests
{
    public class SerilogMetricsReporterTests
    {
        [Fact]
        public void OptionsMustNotBeNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action constructor = () => new SerilogMetricsReporter(null);

            constructor.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("options");
        }

        [Fact]
        public void SerilogFormatterUsedByDefault()
        {
            var reporter = new SerilogMetricsReporter(new SerilogMetricsReporterOptions());

            reporter.Formatter.Should().BeOfType<SerilogMetricsOutputFormatter>();
        }

        [Fact]
        public void SerilogFormatterCannotBeReplaced()
        {
            var reporter = new SerilogMetricsReporter(new SerilogMetricsReporterOptions());

            Action set = () => reporter.Formatter = new MetricsTextOutputFormatter();

            set.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void UsesFlushIntervalFromOptions()
        {
            var tenSeconds = TimeSpan.FromSeconds(10);

            var options = new SerilogMetricsReporterOptions
            {
                FlushInterval = tenSeconds
            };

            var reporter = new SerilogMetricsReporter(options);

            reporter.FlushInterval.Should().Be(tenSeconds);
        }

        [Fact]
        public void FlushIntervalMustBeGreaterThanZero()
        {
            var options = new SerilogMetricsReporterOptions
            {
                FlushInterval = TimeSpan.MinValue
            };

            // ReSharper disable once ObjectCreationAsStatement
            Action constructor = () => new SerilogMetricsReporter(options);

            constructor.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UsesFilterFromOptions()
        {
            var filter = Mock.Of<IFilterMetrics>();

            var options = new SerilogMetricsReporterOptions
            {
                Filter = filter
            };

            var reporter = new SerilogMetricsReporter(options);

            reporter.Filter.Should().BeSameAs(filter);
        }
    }
}
