using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace TelemetryTesting
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var configuration = builder.Configuration;
            var services = builder.Services;

            var otelEndpoint = "<azure app insights connection string>";

            _ = services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                   
                    .AddService("Testing service"))
                    .WithMetrics(metrics =>
                    {
                        // Either use the otel exporter...
                        _ = metrics.AddOtlpExporter(o =>
                        {
                            o.Protocol = OtlpExportProtocol.Grpc;
                            o.Endpoint = new Uri("http://otel_collector:4317");
                            o.ExportProcessorType = ExportProcessorType.Simple;
                        })

                        // ... or the Azure Monitor exporter
                        //_ = metrics.AddAzureMonitorMetricExporter(o =>
                        //{
                        //    o.ConnectionString = otelEndpoint;
                        //})

                        // Something to note: the Azure Monitor exporter exports metrics using delta temporality,
                        // which means that it will only export the difference between the last export and the current one.
                        // This is different to the otel SDK, which defaults for cumlative, which is a monotonic increasing value.
                        // See: https://grafana.com/blog/2023/09/26/opentelemetry-metrics-a-guide-to-delta-vs.-cumulative-temporality-trade-offs/

                        // To use delta temporality with the otel exporter, you can use the following:
                        //var options = new OtlpExporterOptions
                        //{
                        //    Protocol = OtlpExportProtocol.Grpc,
                        //    Endpoint = new Uri(otelEndpoint),
                        //    ExportProcessorType = ExportProcessorType.Simple,
                        //};

                        //var reader = new PeriodicExportingMetricReader(new OtlpMetricExporter(options))
                        //{
                        //    TemporalityPreference = MetricReaderTemporalityPreference.Delta
                        //};

                        //_ = metrics.AddReader(reader)

                        .AddMeter("*");
                    });

            _ = services.AddHostedService<Worker>();

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
