﻿using Core.Pubsub;
using Core.Pubsub.Events;
using GraphSharp.Controls;
using GraphSharp.Sample.Helpers;
using GraphSharp.Sample.ViewModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphSharp.Sample.DragDrop
{
    public class DragDropManagerUtilities
    {
        private const double dragStartThreshold = 5;
        private static readonly Point EndPointOffset = new Point(3, 3);
        private bool allowDropCache;
        private Popup arrowContainer;
        private ArrowShape arrowVisual;
        private FrameworkElement rootVisual;

        public DragDropManagerUtilities()
        {
            PubSub.Aggregator.GetEvent<StartDrawArrow>().Subscribe(OnStartDrawArrowEvent,ThreadOption.UIThread);
            PubSub.Aggregator.GetEvent<OnDrawingArrow>().Subscribe(OnWindowDragOverArrowEvent, ThreadOption.UIThread);
            PubSub.Aggregator.GetEvent<StopDrawingArrow>().Subscribe(OnStopDrawArrowEvent, ThreadOption.UIThread);


            //DragDropManager.AddDragOverHandler(rootVisual, OnWindowDragOver, true);
        }
        public void SetupArrow(FrameworkElement _rootVisual)
        {
            rootVisual = _rootVisual;
            arrowVisual = new ArrowShape();
            arrowVisual.HeadHeight = 10;
            arrowVisual.HeadWidth = 10;
            arrowVisual.Stroke = Brushes.RoyalBlue;
            arrowVisual.StrokeThickness = 3;

            arrowContainer = new Popup();
            arrowContainer.AllowsTransparency = true;
            arrowContainer.Placement = PlacementMode.Relative;
            arrowContainer.PlacementTarget = rootVisual;
            arrowContainer.Child = arrowVisual;
        }


        //public static readonly DependencyProperty ShowArrowDragCueProperty =
        //    DependencyProperty.RegisterAttached(
        //        "ShowArrowDragCue",
        //        typeof(bool),
        //        typeof(DragDropManagerUtilities),
        //        new PropertyMetadata(false, OnShowArrowDragCueChanged));

        //public static bool GetShowArrowDragCue(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(ShowArrowDragCueProperty);
        //}

        //public static void SetShowArrowDragCue(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(ShowArrowDragCueProperty, value);
        //}

        //private static void OnShowArrowDragCueChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        //{
        //    var element = (FrameworkElement)target;
        //    if ((bool)args.NewValue)
        //    {
        //        DragDropManager.AddDragInitializeHandler(element, OnElementDragInitialize, true);
        //        DragDropManager.AddDragDropCompletedHandler(element, OnElementDragDropCompleted, true);
        //        element.Unloaded += OnElementUnloaded;
        //    }
        //    else
        //    {
        //        UnsubscribeFromEvents(element);
        //    }
        //}

        private void OnStartDrawArrowEvent(object sender)
        {

            if (sender is VertexControl vertexControl)
            {
                if (vertexControl.Parent is PocGraphLayout pocGraphLayout)
                {
                    rootVisual = pocGraphLayout.Parent as FrameworkElement;
                    var transform = vertexControl.TransformToAncestor(rootVisual);
                    Point topLeft = transform.Transform(new Point(0, 0));
                    //To get the right and bottom point of the element using height and width.
                    Point bottomRight = transform.Transform(new Point(vertexControl.ActualWidth, vertexControl.ActualHeight));
                    
                    var x = topLeft.X+ (bottomRight.X-topLeft.X)/2;
                    var y = topLeft.Y+ (bottomRight.Y - topLeft.Y) / 2;
                    SetupArrow(rootVisual);
                    arrowContainer.Width = rootVisual.ActualWidth;
                    arrowContainer.Height = rootVisual.ActualHeight;
                    //var position = Mouse.GetPosition(rootVisual);
                    //arrowVisual.X1 = arrowVisual.X2 = position.X;
                    //arrowVisual.Y1 = arrowVisual.Y2 = position.Y;

                    arrowVisual.X1 = arrowVisual.X2 = x;
                    arrowVisual.Y1 = arrowVisual.Y2 = y;

                    allowDropCache = rootVisual.AllowDrop;
                    rootVisual.AllowDrop = true;
                    //var rect = new Rect(topLeft, bottomRight);
                }
            }
            //if (sender is VertexControl vertexControl)
            //{
            //    arrowContainer.Width = rootVisual.ActualWidth;
            //    arrowContainer.Height = rootVisual.ActualHeight;
            //    var position = Mouse.GetPosition(rootVisual);
            //    arrowVisual.X1 = arrowVisual.X2 = position.X;
            //    arrowVisual.Y1 = arrowVisual.Y2 = position.Y;

            //    allowDropCache = rootVisual.AllowDrop;
            //    rootVisual.AllowDrop = true;
            //}
        }

        private void OnStopDrawArrowEvent(object sender)
        {
            arrowContainer.IsOpen = false;
            rootVisual.AllowDrop = allowDropCache;
        }

        private void OnWindowDragOverArrowEvent(object sender)
        {
            //if (e.AllowedEffects != DragDropEffects.None)
            //{
            

            var position = Mouse.GetPosition(rootVisual) - EndPointOffset;
            arrowVisual.X2 = position.X;
            arrowVisual.Y2 = position.Y;
            arrowVisual.UpdateGeometry();

            if (!arrowContainer.IsOpen &&
                GetDistance(arrowVisual.X1, arrowVisual.Y1, arrowVisual.X2, arrowVisual.Y2) >= dragStartThreshold)
            {
                arrowContainer.IsOpen = true;
            }

            var hit = VisualTreeHelper.HitTest(rootVisual, new Point(position.X, position.Y));

            if (hit != null)
            {
                if (hit.VisualHit is FrameworkElement frameworkElement)
                {

                    var vertexControl = VisualTreeHelperExtended.FindVisualParent<VertexControl>(frameworkElement);

                    if (vertexControl == null)
                    {
                        if (HighligtedVertexControl != null)
                        {
                            GraphElementBehaviour.SetHighlightTrigger(HighligtedVertexControl, false);
                            HighligtedVertexControl = null;
                        }
                        return;

                    }
                    HighligtedVertexControl = vertexControl;
                    GraphElementBehaviour.SetHighlightTrigger(HighligtedVertexControl, true);
                    //MouseEventArgs mouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0);
                    //mouseEventArgs.RoutedEvent = Mouse.MouseEnterEvent;
                    //vertexControl.RaiseEvent(mouseEventArgs);
                }

            }
            //MouseEventArgs mouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            //mouseEventArgs.RoutedEvent = Mouse.MouseMoveEvent;
            //rootVisual.RaiseEvent(mouseEventArgs);
            //}
        }
        private VertexControl HighligtedVertexControl = null;

        //private static void UnsubscribeFromEvents(FrameworkElement element)
        //{
        //    DragDropManager.RemoveDragInitializeHandler(element, OnElementDragInitialize);
        //    DragDropManager.RemoveDragDropCompletedHandler(element, OnElementDragDropCompleted);
        //    element.Unloaded -= OnElementUnloaded;
        //}

        //private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        //{
        //    UnsubscribeFromEvents((FrameworkElement)sender);
        //}

        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Point.Subtract(new Point(x2, y2), new Point(x1, y1)).Length;
        }
    }
}
