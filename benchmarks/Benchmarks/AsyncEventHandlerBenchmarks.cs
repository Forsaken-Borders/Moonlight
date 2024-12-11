using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Moonlight.Api.Events;

namespace Moonlight.Benchmarks
{
    public class AsyncEventHandlerBenchmarks
    {
        public IEnumerable<AsyncServerEvent<AsyncServerEventArgs>> AsyncEvents { get; }

        public AsyncEventHandlerBenchmarks()
        {
            // Generate a anonymous delegate through expressions
            Expression<Func<AsyncServerEventArgs, ValueTask<bool>>> preHandler = eventArgs => new ValueTask<bool>(true);
            Expression<Func<AsyncServerEventArgs, ValueTask>> postHandler = eventArgs => ValueTask.CompletedTask;

            List<AsyncServerEvent<AsyncServerEventArgs>> asyncEvents = [];
            for (int i = 0; i < 10; i++)
            {
                asyncEvents.Add(new AsyncServerEvent<AsyncServerEventArgs>());
                int j = 0;
                while (j < i)
                {
                    asyncEvents[i].AddPreHandler(preHandler.Compile().Method.CreateDelegate<AsyncServerEventPreHandler<AsyncServerEventArgs>>());
                    asyncEvents[i].AddPostHandler(postHandler.Compile().Method.CreateDelegate<AsyncServerEventHandler<AsyncServerEventArgs>>());
                    j++;
                }
            }

            AsyncEvents = asyncEvents;
        }

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEvents))]
        public void Prepare(AsyncServerEvent<AsyncServerEventArgs> asyncEvent) => asyncEvent.Prepare();

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEvents))]
        public async ValueTask InvokeAsync(AsyncServerEvent<AsyncServerEventArgs> asyncEvent) => await asyncEvent.InvokePostHandlersAsync(new AsyncServerEventArgs());

        [Benchmark]
        [ArgumentsSource(nameof(AsyncEvents))]
        public async ValueTask<bool> InvokePreHandlersAsync(AsyncServerEvent<AsyncServerEventArgs> asyncEvent) => await asyncEvent.InvokePreHandlersAsync(new AsyncServerEventArgs());
    }
}
