using UIKit;
using Xamarin.Forms.Platform.iOS;
using TripSketch.iOS.Renderers;
using Xamarin.Forms;
using TripSketch.Controls;
using CoreGraphics;

[assembly: ExportRenderer(typeof(DraggableView), typeof(DraggableViewRenderer))]

namespace TripSketch.iOS.Renderers
{
    public class DraggableViewRenderer : VisualElementRenderer<DraggableView>
    {
        #region Fields

        private UIPanGestureRecognizer _panGesture;
        private CGPoint _startingPosition;
        private UIView _dragVisual;

        #endregion

        #region Overrides

        protected override void OnElementChanged(ElementChangedEventArgs<DraggableView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null && _panGesture != null)
            {
                this.RemoveGestureRecognizer(_panGesture);
                _panGesture = null;
            }

            if (e.OldElement == null)
            {
                _panGesture = new UIPanGestureRecognizer(HandlePanGesture);

                this.AddGestureRecognizer(_panGesture);
            }
        }

        #endregion

        #region Private Methods

        protected void HandlePanGesture()
        {
            switch (_panGesture.State)
            {
                case UIGestureRecognizerState.Began:
                    HandleDragBegan();
                    break;
                case UIGestureRecognizerState.Changed:
                    HandleDragChanged();
                    break;
                case UIGestureRecognizerState.Ended:
                    HandleDragEnded();
                    break;
                case UIGestureRecognizerState.Cancelled:
                    HandleDragCanceled();
                    break;
            }
        }


        /// <summary>
        /// Initialize drag (raises event and creates drag visual)
        /// </summary>
        private void HandleDragBegan()
        {
            CreateDragVisual();

            // Raise event
            if (Element != null)
            {
                Element.OnDragStart();
            }
        }

        /// <summary>
        /// Move drag visual
        /// </summary>
        private void HandleDragChanged()
        {
            // Move the drag visual with the drag
            //
            var translation = _panGesture.TranslationInView(this);
            _dragVisual.Center = new CGPoint(_startingPosition.X + translation.X, _startingPosition.Y + translation.Y);

            var draggableView = Element as DraggableView;
            if (draggableView != null)
            {
                var pt = new Xamarin.Forms.Point(draggableView.Bounds.Center.X + translation.X,
                    draggableView.Bounds.Center.Y + translation.Y);

                draggableView.OnDragging(pt);
            }
        }

        private void HandleDragEnded()
        {
            // Move the drag visual with the drag
            //
            var translation = _panGesture.TranslationInView(this);
            _dragVisual.Center = new CGPoint(_startingPosition.X + translation.X, _startingPosition.Y + translation.Y);

            // Complete the drag (i.e., drop)
            //
            if (Element != null)
            {
                var pt = new Xamarin.Forms.Point(Element.Bounds.Center.X + translation.X,
                    Element.Bounds.Center.Y + translation.Y);

                Element.OnDragged(pt);

                // Reset position offset
                if (Element != null) // may have been cleared during OnDragged
                {
                    Frame = Element.Bounds.ToRectangleF();
                }
            }

            // Remove drag visual
            //
            ClearDragVisual();
        }

        private void HandleDragCanceled()
        {
            if (Element != null)
            {
                Element.OnDragCanceled();
            }

            // Remove drag visual
            //
            ClearDragVisual();
        }

        private void CreateDragVisual()
        {
            ClearDragVisual();

            _dragVisual = SnapshotView(true);

            // Adding to Window so it's on top of everything
            //
            // TODO: Test for side-effects with this strategy... e.g. rotation?
            //
            Window.Add(_dragVisual);

            // We're actually using _startingPosition to place the drag visual,
            // so adjust to the window's coordinates
            //
            _startingPosition = Superview.ConvertPointToView(Center, Window);

            // Set starting position for drag visual
            //
            _dragVisual.Center = _startingPosition;
        }

        private void ClearDragVisual()
        {
            if (_dragVisual != null)
            {
                _dragVisual.RemoveFromSuperview();
                _dragVisual = null;
            }
        }

        #endregion
    }
}