using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TripSketch.WinPhone.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(Label), typeof(LabelRendererEx))]

namespace TripSketch.WinPhone.Renderers
{
    /// <summary>
    /// Overrides standard label renderer to fix issue with drawing text outside boundaries
    /// </summary>
    public class LabelRendererEx : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Tracker = new LabelTracker { Model = Element, Element = this, Child = Control };
            }
        }


        /// <summary>
        /// This is a JustDecompile export of the private class defined in the Xamarin.Forms LabelRenderer.
        /// Modified to prevent text from being drawn outside of the control boundaries.
        /// </summary>
        private class LabelTracker : VisualElementTracker<Label, FrameworkElement>
        {
            protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                base.HandlePropertyChanged(sender, e);
                if (e.PropertyName == Label.YAlignProperty.PropertyName)
                {
                    this.UpdateNativeControl();
                }
            }

            protected override void LayoutChild()
            {
                SizeRequest sizeRequest = Model.GetSizeRequest(Model.Width, Double.PositiveInfinity);
                double height = Model.Height;
                Xamarin.Forms.Size request = sizeRequest.Request;
                double num = Math.Max(0, Math.Min(height, request.Height));
                switch (Model.YAlign)
                {
                    case Xamarin.Forms.TextAlignment.Start:
                        {
                            Canvas.SetTop(Child, 0);
                            break;
                        }
                    case Xamarin.Forms.TextAlignment.Center:
                        {
                            Canvas.SetTop(Child, (double)((int)((Model.Height - num) / 2)));
                            break;
                        }
                    case Xamarin.Forms.TextAlignment.End:
                        {
                            Canvas.SetTop(Child, Model.Height - num);
                            break;
                        }
                    default:
                        {
                            goto case Xamarin.Forms.TextAlignment.Center;
                        }
                }
                Child.Height = num;
                Child.Width = Model.Width;

                var canvas = Element as Canvas;

                if (canvas != null)
                {
                    canvas.Clip = new RectangleGeometry { Rect = new Rect(0, 0, Model.Width, Model.Height) };
                }
            }
        }
    }
}
