
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using TripSketch.Controls;
using TripSketch.iOS.Renderers;

[assembly: ExportRenderer(typeof(BorderView), typeof(BorderViewRenderer))]

namespace TripSketch.iOS.Renderers
{
    public class BorderViewRenderer : VisualElementRenderer<BorderView>
    {
        public override void Draw(CoreGraphics.CGRect rect)
        {
            var borderView = this.Element as BorderView;

            if (borderView == null)
            {
                return;
            }

            using (var context = UIGraphics.GetCurrentContext())
            {
                context.SetFillColor(borderView.BackgroundColor.ToCGColor());
                context.SetStrokeColor(borderView.BorderColor.ToCGColor());
                context.SetLineWidth((float)borderView.BorderThickness);
                context.AddRect(rect);
                context.DrawPath(CoreGraphics.CGPathDrawingMode.FillStroke);
            }
        }
    }
}