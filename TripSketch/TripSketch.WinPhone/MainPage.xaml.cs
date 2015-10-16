
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;


namespace TripSketch.WinPhone
{
    public partial class MainPage : FormsApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            Forms.Init();

            LoadApplication(new TripSketch.App());
        }
    }
}
