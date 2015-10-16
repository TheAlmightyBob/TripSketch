using UIKit;
using TripSketch.Controls;
using TripSketch.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TappableView), typeof(TappableViewRenderer))]

namespace TripSketch.iOS.Renderers
{
    public class TappableViewRenderer : VisualElementRenderer<TappableView>
    {
        private UITapGestureRecognizer _tapGesture;

        protected override void OnElementChanged(ElementChangedEventArgs<TappableView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null && _tapGesture != null)
            {
                this.RemoveGestureRecognizer(_tapGesture);
                _tapGesture = null;
            }

            if (e.OldElement == null)
            {
                _tapGesture = new UITapGestureRecognizer(HandleTapGesture);

                this.AddGestureRecognizer(_tapGesture);
            }
        }

        private void HandleTapGesture()
        {
            if (Element != null)
            {
                var nativePoint = _tapGesture.LocationInView(this);
                var xfPoint = new Xamarin.Forms.Point(nativePoint.X, nativePoint.Y);

                Element.OnTap(xfPoint);
            }
        }
    }
}
