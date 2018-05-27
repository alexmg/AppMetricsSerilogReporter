using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Formatters;
using App.Metrics.Serialization;

namespace AppMetricsSerilogReporter
{
    /// <summary>
    /// A metrics output formatter for Serilog.
    /// </summary>
    public class SerilogMetricsOutputFormatter : IMetricsOutputFormatter
    {
        private readonly SerilogMetricsReporterOptions _options;
        private readonly MetricSnapshotSerializer _serializer = new MetricSnapshotSerializer();

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogMetricsOutputFormatter"/> class.
        /// </summary>
        /// <param name="options">The Serilog metrics reporter options.</param>
        public SerilogMetricsOutputFormatter(SerilogMetricsReporterOptions options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public MetricsMediaTypeValue MediaType => new MetricsMediaTypeValue("text", "vnd.appmetrics.metrics.serilog", "v1", "json");

        /// <inheritdoc />
        public Task WriteAsync(Stream output, MetricsDataValueSource metricsData, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var metricSnapshotWriter = new SerilogMetricSnapshotWriter(_options))
            {
                _serializer.Serialize(metricSnapshotWriter, metricsData);
            }

            return Task.CompletedTask;
        }
    }
}
