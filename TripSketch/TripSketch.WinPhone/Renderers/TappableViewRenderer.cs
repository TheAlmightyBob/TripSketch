using System.Windows.Input;
using TripSketch.Controls;
using TripSketch.WinPhone.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(TappableView), typeof(TappableViewRenderer))]

namespace TripSketch.WinPhone.Renderers
{
    public class TappableViewRenderer : ViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                Tap -= OverlayViewRenderer_Tap;
            }

            if (e.OldElement == null)
            {
                Tap += OverlayViewRenderer_Tap;
            }
        }

        private void OverlayViewRenderer_Tap(object sender, GestureEventArgs e)
        {
            var tappableView = Element as TappableView;

            if (tappableView != null)
            {
                var nativePoint = e.GetPosition(this);
                var xfPoint = new Xamarin.Forms.Point(nativePoint.X, nativePoint.Y);
                tappableView.OnTap(xfPoint);
            }
        }
    }
}
