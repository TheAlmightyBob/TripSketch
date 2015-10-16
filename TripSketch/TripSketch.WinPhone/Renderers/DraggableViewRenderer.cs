using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
using TripSketch.Controls;
using TripSketch.WinPhone.Renderers;
using TripSketch.WinPhone.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(DraggableView), typeof(DraggableViewRenderer))]

namespace TripSketch.WinPhone.Renderers
{
    public class DraggableViewRenderer : ViewRenderer
    {
        private TranslateTransform _translation;
        private System.Windows.Controls.Image _dragVisual;


        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);
            
            if (e.NewElement == null)
            {
                ManipulationStarted -= DraggableViewRenderer_ManipulationStarted;
                ManipulationDelta -= DraggableViewRenderer_ManipulationDelta;
                ManipulationCompleted -= DraggableViewRenderer_ManipulationCompleted;
            }

            if (e.OldElement == null)
            {
                ManipulationStarted += DraggableViewRenderer_ManipulationStarted;
                ManipulationDelta += DraggableViewRenderer_ManipulationDelta;
                ManipulationCompleted += DraggableViewRenderer_ManipulationCompleted;

                _translation = new TranslateTransform();
                RenderTransform = _translation;
                UseOptimizedManipulationRouting = false;
            }
        }

        void DraggableViewRenderer_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var draggableView = Element as DraggableView;

            InitializeRenderTransform();

            // Create drag visual
            //
            CreateDragVisual();

            // Raise DragStart event
            //
            if (draggableView != null)
            {
                draggableView.OnDragStart();
            }

            e.Handled = true;
        }

        private void CreateDragVisual()
        {
            ClearDragVisual();

            var bitmap = new System.Windows.Media.Imaging.WriteableBitmap(this, null);
            bitmap.Render(this, null);
            bitmap.Invalidate();

            _dragVisual = new System.Windows.Controls.Image();
            _dragVisual.Source = bitmap;

            // find topmost canvas.. (so we can add the drag visual as a child
            // and ensure that it is on top of everything)
            //
            var canvas = this.GetTopmostParentOfType<Canvas>();

            canvas.Children.Add(_dragVisual);

            var point = this.GetRelativePosition(canvas);

            _dragVisual.SetValue(Canvas.TopProperty, point.Y);
            _dragVisual.SetValue(Canvas.LeftProperty, point.X);

            // Really make sure the drag visual is on top
            //
            Canvas.SetZIndex(_dragVisual, Int16.MaxValue);
        }

        private void ClearDragVisual()
        {
            if (_dragVisual != null)
            {
                var canvas = _dragVisual.Parent as Canvas;
                canvas.Children.Remove(_dragVisual);
                _dragVisual = null;
            }
        }

        void DraggableViewRenderer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // Move drag visual
            //
            var x = Canvas.GetLeft(_dragVisual);
            var y = Canvas.GetTop(_dragVisual);

            _dragVisual.SetValue(Canvas.LeftProperty, x + e.DeltaManipulation.Translation.X);
            _dragVisual.SetValue(Canvas.TopProperty, y + e.DeltaManipulation.Translation.Y);

            var draggableView = Element as DraggableView;

            if (draggableView != null)
            {
                var delta = e.CumulativeManipulation.Translation;
                var pt = new Xamarin.Forms.Point(draggableView.Bounds.Center.X + delta.X,
                    draggableView.Bounds.Center.Y + delta.Y);

                draggableView.OnDragging(pt);
            }

            e.Handled = true;
        }

        void DraggableViewRenderer_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var delta = e.TotalManipulation.Translation;
            var draggableView = Element as DraggableView;

            // Complete the drag
            //
            if (draggableView != null)
            {
                var pt = new Xamarin.Forms.Point(draggableView.Bounds.Center.X + delta.X,
                    draggableView.Bounds.Center.Y + delta.Y);

                draggableView.OnDragged(pt);

                ClearDragVisual();
            }

            e.Handled = true;
        }

        private void InitializeRenderTransform()
        {
            if (RenderTransform == _translation)
            {
                return;
            }

            // Initialize translation transform
            // (necessary because other logic is also setting RenderTransform)

            var matrixTransform = RenderTransform as MatrixTransform;
            if (matrixTransform != null && matrixTransform.Matrix == Matrix.Identity)
            {
                RenderTransform = _translation;
            }
            else
            {
                var transformGroup = RenderTransform as TransformGroup;
                if (transformGroup != null)
                {
                    if (!transformGroup.Children.Contains(_translation))
                    {
                        transformGroup.Children.Add(_translation);
                    }
                }
                else
                {
                    transformGroup = new TransformGroup();
                    transformGroup.Children.Add(RenderTransform);
                    transformGroup.Children.Add(_translation);
                    RenderTransform = transformGroup;
                }
            }
        }
    }
}
