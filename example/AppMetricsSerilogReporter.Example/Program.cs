using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Scheduling;
using App.Metrics.Timer;
using Serilog;
using Serilog.Events;

namespace AppMetricsSerilogReporter.Example
{
    internal static class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341/")
                .CreateLogger();

            Log.Information("Starting AppMetrics Serilog Reporter sample");

            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.Configure(options =>
                    options.GlobalTags.Add("InstanceId", Guid.NewGuid().ToString("N")))
                .Report.ToConsole()
                .Report.ToSerilog(LogEventLevel.Information)
                .Build();

            var cts = new CancellationTokenSource();

            var scheduler = new AppMetricsTaskScheduler(
                TimeSpan.FromSeconds(3),
                () => Task.WhenAll(metrics.ReportRunner.RunAllAsync()));
            scheduler.Start();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                scheduler.Dispose();
                cts.Cancel();
            };

            var counterTask = RunCounter(metrics, cts.Token);
            var timerTask = RunTimer(metrics, cts.Token);
            var meterTask = RunMeter(metrics, cts.Token);
            var histogramTask = RunHistogram(metrics, cts.Token);
            var gaugeTask = RunGauge(metrics, cts.Token);

            await Task.WhenAll(counterTask, timerTask, meterTask, histogramTask, gaugeTask);

            Log.Information("Sample application shutdown");
            Log.CloseAndFlush();
        }

        private static async Task RunCounter(IMetrics metrics, CancellationToken token)
        {
            const int delay = 1000;

            var counter = new CounterOptions
            {
                Name = "MyCounter",
                Context = nameof(RunCounter),
                Tags = new MetricTags("delay", $"{delay}")
            };

            while (!token.IsCancellationRequested)
            {
                metrics.Measure.Counter.Increment(counter, "item1");
                metrics.Measure.Counter.Increment(counter, "item2");

                await DelayOrComplete(token, delay);
            }
        }

        private static async Task RunTimer(IMetrics metrics, CancellationToken token)
        {
            var timer = new TimerOptions
            {
                Name = "MyTimer",
                Context = nameof(RunTimer),
                Tags = new MetricTags("delay", "random")
            };

            var random = new Random();

            while (!token.IsCancellationRequested)
            {
                var delay = random.Next(100, 1000);

                using (var context = metrics.Measure.Timer.Time(timer))
                {
                    context.TrackUserValue($"/user/{delay}");

                    await DelayOrComplete(token, delay);
                }
            }
        }

        private static async Task RunMeter(IMetrics metrics, CancellationToken token)
        {
            var meter = new MeterOptions
            {
                Name = "MyMeter",
                Context = nameof(RunMeter),
                Tags = new MetricTags("delay", "random"),
                MeasurementUnit = Unit.Calls
            };

            var random = new Random();
            var counter = 0;

            while (!token.IsCancellationRequested)
            {
                var delay = random.Next(100, 200);
                var route = counter % 2 == 0 ? "/user" : "/account";
                counter++;

                metrics.Measure.Meter.Mark(meter, route);

                await DelayOrComplete(token, delay);
            }
        }

        private static async Task RunHistogram(IMetrics metrics, CancellationToken token)
        {
            var histogram = new HistogramOptions
            {
                Name = "MyHistogram",
                Context = nameof(RunHistogram),
                Tags = new MetricTags("delay", "random"),
                MeasurementUnit = Unit.Bytes
            };

            var random = new Random();
            var counter = 0;

            while (!token.IsCancellationRequested)
            {
                var delay = random.Next(100, 1000);
                var client = counter % 2 == 0 ? "client_1" : "client_2";
                counter++;

                metrics.Measure.Histogram.Update(histogram, delay, client);

                await DelayOrComplete(token, delay);
            }
        }

        private static async Task RunGauge(IMetrics metrics, CancellationToken token)
        {
            const int delay = 1000;

            var gauge = new GaugeOptions
            {
                Name = "MyGauge",
                Context = nameof(RunGauge),
                Tags = new MetricTags("delay", $"{delay}"),
                MeasurementUnit = Unit.Bytes
            };

            var process = Process.GetCurrentProcess();

            while (!token.IsCancellationRequested)
            {
                metrics.Measure.Gauge.SetValue(gauge, process.PrivateMemorySize64);

                await DelayOrComplete(token, delay);
            }
        }

        private static async Task DelayOrComplete(CancellationToken token, int delay)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (TaskCanceledException)
            {
                await Task.CompletedTask;
            }
        }
    }
}
