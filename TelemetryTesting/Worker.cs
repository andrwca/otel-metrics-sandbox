using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryTesting
{
    internal class Worker : BackgroundService
    {
        private static readonly Meter _testMeter = new("TestMeter");
        private static readonly Counter<long> _testCounter = _testMeter.CreateCounter<long>("test.counter");

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (true)
            {
                Console.WriteLine("Adding two to metric.");

                // Add two to the metric
                _testCounter?.Add(2);

                Thread.Sleep(1000);
            }
        }
    }
}
