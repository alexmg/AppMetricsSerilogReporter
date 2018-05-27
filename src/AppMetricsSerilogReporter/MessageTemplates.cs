namespace AppMetricsSerilogReporter
{
    public class MessageTemplates
    {
        public string Fallback { get; set; }

        public string Counter { get; set; }

        public string CounterItem { get; set; }

        public string Gauge { get; set; }

        public string Histogram { get; set; }

        public string Meter { get; set; }

        public string MeterItem { get; set; }

        public string Timer { get; set; }

        public MessageTemplates()
        {
            Fallback = "[Metric] Context: {context}, Name: {name}";

            Counter = "[Counter] Context: {context}, Name: {name}, Value: {value}";

            CounterItem = "[CounterItem] Context: {context}, Name: {name}, Item: {item}, Percent: {percent}, Total: {total}";

            Gauge = "[Gauge] Context: {context}, Name: {name}, Value: {value}";

            Histogram = "[Histogram] Context: {context}, Name: {name}, Min: {min}, Mean: {mean}, Max: {max}";

            Meter = "[Meter] Context: {context}, Name: {name}, Count: {count_meter}, Mean: {rate_mean}";

            MeterItem = "[MeterItem] Context: {context}, Name: {name}, Item: {item}, Percent: {percent}, Count: {count_meter}, Mean: {rate_mean}";

            Timer = "[Timer] Context: {context}, Name: {name}, Min: {min}, Mean: {mean}, Max: {max}";
        }
    }
}
