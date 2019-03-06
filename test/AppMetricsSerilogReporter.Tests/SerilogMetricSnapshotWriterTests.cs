using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using FluentAssertions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace AppMetricsSerilogReporter.Tests
{
    public class SerilogMetricSnapshotWriterTests
    {
        public SerilogMetricSnapshotWriterTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestCorrelator()
                .CreateLogger();
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void SingleValueLogEventsAreWrittenAtSpecifiedLevel(LogEventLevel logEventLevel)
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = logEventLevel};
                var writer = new SerilogMetricSnapshotWriter(options);
                writer.Write("context", "name", "value", 123, MetricTags.Empty, DateTime.UtcNow);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Level.Should().Be(logEventLevel);
            }
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void MultipleValueLogEventsAreWrittenAtSpecifiedLevel(LogEventLevel logEventLevel)
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = logEventLevel};
                var writer = new SerilogMetricSnapshotWriter(options);
                var columns = new [] {"A", "B", "C"};
                var values = new object[] {1, 2, 3};

                writer.Write("context", "name", columns, values, MetricTags.Empty, DateTime.UtcNow);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Level.Should().Be(logEventLevel);
            }
        }

        [Fact]
        public void SingleValueLogEventsAreWrittenAtSpecifiedTime()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = LogEventLevel.Verbose};
                var writer = new SerilogMetricSnapshotWriter(options);
                var timestamp = DateTime.UtcNow;

                writer.Write("context", "name", "value", 123, MetricTags.Empty, timestamp);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Timestamp.Should().Be(timestamp);
            }
        }

        [Fact]
        public void SingleValueLogEventsUseMessageTemplate()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions
                {
                    LogEventLevel = LogEventLevel.Verbose,
                    MessageTemplates = { Fallback = "Metric {name} in context {context}" }
                };
                var writer = new SerilogMetricSnapshotWriter(options);
                var timestamp = DateTime.UtcNow;

                writer.Write("context", "name", "value", 123, MetricTags.Empty, timestamp);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.RenderMessage().Should().Be("Metric \"name\" in context \"context\"");
            }
        }

        [Fact]
        public void SingleValueLogEventsIncludesProperties()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = LogEventLevel.Verbose};
                var writer = new SerilogMetricSnapshotWriter(options);
                var metricTags = MetricTags.FromSetItemString("foo:10,bar:20");

                writer.Write("context", "name", "value", 123, metricTags, DateTime.UtcNow);

                var tagsProperty = metricTags.Keys.Zip(metricTags.Values, (key, value) =>
                    new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                        new ScalarValue(key), new ScalarValue(value)));

                var properties = new Dictionary<string, LogEventPropertyValue>()
                {
                    {"context", new ScalarValue("context")},
                    {"name", new ScalarValue("name")},
                    {"value", new ScalarValue(123)},
                    {"tags", new DictionaryValue(tagsProperty)}
                };

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Properties.Should().BeEquivalentTo(properties);
            }
        }

        [Fact]
        public void MultipleValueLogEventsAreWrittenAtSpecifiedTime()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = LogEventLevel.Verbose};
                var writer = new SerilogMetricSnapshotWriter(options);
                var columns = new [] {"A", "B", "C"};
                var values = new object[] {1, 2, 3};
                var timestamp = DateTime.UtcNow;

                writer.Write("context", "name", columns, values, MetricTags.Empty, timestamp);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Timestamp.Should().Be(timestamp);
            }
        }

        [Fact]
        public void MultipleValueLogEventsUseMessageTemplate()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions
                {
                    LogEventLevel = LogEventLevel.Verbose,
                    MessageTemplates = {Fallback = "Metric {name} in context {context}"}
                };
                var writer = new SerilogMetricSnapshotWriter(options);
                var columns = new [] {"A", "B", "C"};
                var values = new object[] {1, 2, 3};

                writer.Write("context", "name", columns, values, MetricTags.Empty, DateTime.UtcNow);

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.RenderMessage().Should().Be("Metric \"name\" in context \"context\"");
            }
        }

        [Fact]
        public void MultipleValueLogEventsIncludesProperties()
        {
            using (TestCorrelator.CreateContext())
            {
                var options = new SerilogMetricsReporterOptions {LogEventLevel = LogEventLevel.Verbose};
                var writer = new SerilogMetricSnapshotWriter(options);
                var columns = new [] {"A", "B", "C"};
                var values = new object[] {1, 2, 3};
                var metricTags = MetricTags.FromSetItemString("foo:10,bar:20");

                writer.Write("context", "name", columns, values, metricTags, DateTime.UtcNow);

                var tagsProperty = metricTags.Keys.Zip(metricTags.Values, (key, value) =>
                    new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                        new ScalarValue(key), new ScalarValue(value)));

                var properties = new Dictionary<string, LogEventPropertyValue>()
                {
                    {"context", new ScalarValue("context")},
                    {"name", new ScalarValue("name")},
                    {"A", new ScalarValue(1)},
                    {"B", new ScalarValue(2)},
                    {"C", new ScalarValue(3)},
                    {"tags", new DictionaryValue(tagsProperty)}
                };

                TestCorrelator.GetLogEventsFromCurrentContext()
                    .Should().ContainSingle()
                    .Which.Properties.Should().BeEquivalentTo(properties);
            }
        }
    }
}
