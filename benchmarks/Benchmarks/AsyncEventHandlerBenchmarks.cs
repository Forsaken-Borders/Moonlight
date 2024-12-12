using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Moonlight.Api.Events;

namespace Moonlight.Benchmarks
{
    public class AsyncEventHandlerBenchmarks
    {
        public IEnumerable<AsyncServerEvent<AsyncServerEventArgs>> AsyncEvents { get; }
        public IEnumerable<object[]> AsyncEventsWithEventArgs => AsyncEvents.Select(x => new object[] { x, new AsyncServerEventArgs() });

        public AsyncEventHandlerBenchmarks()
        {
            // Generate a anonymous delegate through expressions
            Expression<Func<AsyncServerEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);
            Expression<Func<AsyncServerEventArgs, ValueTask>> postHandler = eventArgs => ValueTask.CompletedTask;

            List<AsyncServerEvent<AsyncServerEventArgs>> asyncEvents = [];
            foreach (int i in Enumerable.Range(0, Environment.ProcessorCount + 1).Where(x => x % 4 == 0).Append(1).Append(2).Append(5))
            {
                AsyncServerEvent<AsyncServerEventArgs> asyncEvent = new(new ConfigurationBuilder().Build());
                asyncEvents.Add(asyncEvent);
                int j = 0;
                while (j < i)
                {
                    asyncEvent.AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncServerEventPreHandler<AsyncServerEventArgs>>());
                    asyncEvent.AddPostHandler(postHandler.Compile().Method.CreateDelegate<AsyncServerEventHandler<AsyncServerEventArgs>>());
                    j++;
                }
            }

            AsyncEvents = asyncEvents;
        }

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEvents))]
        public void Prepare(AsyncServerEvent<AsyncServerEventArgs> asyncEvent) => asyncEvent.Prepare();

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEventsWithEventArgs))]
        public async ValueTask InvokeAsync(AsyncServerEvent<AsyncServerEventArgs> asyncEvent, AsyncServerEventArgs eventArgs) => await asyncEvent.InvokePostHandlersAsync(eventArgs);

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEventsWithEventArgs))]
        public async ValueTask<bool> InvokePreHandlersAsync(AsyncServerEvent<AsyncServerEventArgs> asyncEvent, AsyncServerEventArgs eventArgs) => await asyncEvent.InvokePreHandlersAsync(eventArgs);
    }
}
