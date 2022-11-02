using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Core.Pubsub.Events
{
    public class CreateNode:PubSubEvent<CreateNodeBody>
    {
    }
    public class CreateNodeBody
    {
        public object Content { get; set; }
        public Point Point { get; set; }
    }
}
