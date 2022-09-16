using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism;
using Prism.Events;

namespace GraphGenerator.Core.Pubsub
{
    public static class PubSub
    {
        public static IEventAggregator Aggregator;

        static PubSub()
        {
            Aggregator = new EventAggregator();
        }

    }
}
