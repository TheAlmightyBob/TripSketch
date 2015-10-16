using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using TripSketch.iOS.Renderers;
using TripSketch.Controls;
using UIKit;
using CoreGraphics;

[assembly: ExportRenderer(typeof(AutoStackGrid), typeof(TransparentLayoutRenderer))]

namespace TripSketch.iOS.Renderers
{
    /// <summary>
    /// This exists because on iOS, unlike WinPhone, a view with a null background still won't
    /// allow touches to pass through to views below it.
    /// </summary>
    public class TransparentLayoutRenderer : VisualElementRenderer<Layout>
    {
        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var view = base.HitTest(point, uievent);

            return view == this ? null : view;
        }
    }
}
