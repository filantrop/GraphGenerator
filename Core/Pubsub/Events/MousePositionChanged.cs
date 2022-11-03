using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Core.Pubsub.Events
{
    public class MousePositionChanged:PubSubEvent<MousePositionChangedBody>
    {
    }

    public class MousePositionChangedBody
    {
        public string Text { get; set; }
        public Point Point { get; set; }
        public MousePositionChangedBody(string text, Point p)
        {
            Text = text;
            Point = p;  
        }
    }
}
