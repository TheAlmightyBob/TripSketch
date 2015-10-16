using Xamarin.Forms;

namespace TripSketch.Pages
{
    public class RotatingPage<TPortrait, TLandscape> : ContentPage
        where TPortrait : ContentView, new()
        where TLandscape : ContentView, new()
    {
        private bool? _landscape;

        // Re-using the EventsViewGrid worked fine on iOS but crashed on WinPhone.
        // Fortunately, creating each time doesn't seem to have a noticeable
        // performance loss on iOS simulator (but probably lags horribly before exploding on device)...
        //

        public RotatingPage()
        {
#if DEBUG
            ToolbarItems.Add(new ToolbarItem("Switch", "", () =>
            {
                Content = Content is TPortrait ? new TLandscape() as ContentView : new TPortrait() as ContentView;
            }));
#endif
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            var landscape = width > height;

            if (!_landscape.HasValue || _landscape.Value != landscape)
            {
                _landscape = landscape;

                if (_landscape.Value)
                {
                    Content = new TLandscape();
                }
                else
                {
                    Content = new TPortrait();
                }
            }


            base.OnSizeAllocated(width, height);

        }
    }
}
