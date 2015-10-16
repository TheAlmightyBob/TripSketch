using System;
using System.Threading.Tasks;
using TripSketch.Core.Helpers;
using TripSketch.Core.ViewModels;
using TripSketch.Core.Services;
using Xamarin.Forms;

namespace TripSketch.Helpers
{
    public class Navigator : INavigator
    {
        #region Fields

        private INavigation _navigation;
        private ViewProvider _viewProvider;
        private Page _sourcePage;

        #endregion

        #region Constructor

        public Navigator(Page sourcePage)
        {
            _navigation = sourcePage.Navigation;
            _viewProvider = DependencyService.Get<ViewProvider>();
            _sourcePage = sourcePage;
        }

        #endregion

        #region INavigator

        public async Task PushAsync<TViewModel>(TViewModel viewmodel)
            where TViewModel : ViewModelBase
        {
            var page = _viewProvider.GetView(viewmodel) as Page;

            if (page == null)
            {
                throw new ArgumentException("viewmodel does not correspond to a page that can be navigated to");
            }

            await _navigation.PushAsync(page);
        }

        public async Task PushAsync<TViewModel>(Action<TViewModel> customInit = null)
            where TViewModel : ViewModelBase, new()
        {
            var viewmodel = ViewModelProvider.GetViewModel<TViewModel>(customInit);

            // TODO: Throw new custom exception if unable to create VM?

            if (viewmodel != null)
            {
                await PushAsync(viewmodel);
            }
        }

        public async Task PopAsync()
        {
            await _navigation.PopAsync();
        }

        public async Task PushModalAndWaitAsync<TViewModel>(TViewModel viewmodel)
            where TViewModel : ViewModelBase
        {
            var page = _viewProvider.GetView(viewmodel) as Page;
            
            if (page == null)
            {
                throw new ArgumentException("viewmodel does not correspond to a page that can be navigated to");
            }

            await PushModalAndWaitAsync(page);
        }

        public async Task<TViewModel> PushModalAndWaitAsync<TViewModel>(Action<TViewModel> customInit = null)
            where TViewModel : ViewModelBase, new()
        {
            var viewmodel = ViewModelProvider.GetViewModel<TViewModel>(customInit);

            // TODO: Throw new custom exception if unable to create VM?

            if (viewmodel != null)
            {
                await PushModalAndWaitAsync(viewmodel);
            }

            return viewmodel;
        }

        public async Task PushModalAsync<TViewModel>(TViewModel viewmodel)
            where TViewModel : ViewModelBase
        {
            var page = _viewProvider.GetView(viewmodel) as Page;

            if (page == null)
            {
                throw new ArgumentException("viewmodel does not correspond to a page that can be navigated to");
            }

            await _navigation.PushModalAsync(page);
        }

        public async Task PushModalAsync<TViewModel>(Action<TViewModel> customInit = null)
            where TViewModel : ViewModelBase, new()
        {
            var viewmodel = ViewModelProvider.GetViewModel<TViewModel>(customInit);

            // TODO: Throw new custom exception if unable to create VM?

            if (viewmodel != null)
            {
                await PushModalAsync(viewmodel);
            }
        }

        public async Task PopModalAsync()
        {
            await _navigation.PopModalAsync();
        }

        #endregion

        #region Public Methods

        public async Task PushModalAndWaitAsync(Page page)
        {
            var tcs = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (s, e) =>
            {
                tcs.SetResult(null);
                _sourcePage.Appearing -= handler;
            };

            // This is risky in iOS where a ViewController can be dismissed
            // while it's presenting another view controller, but:
            // 1) Xamarin.Forms doesn't currently expose that functionality
            // 2) Don't use this function if that's a concern?
            // 
            // Could also fire a "Done" event from the viewmodel, but that
            // then requires that the viewmodels actually fire it...
            //
            _sourcePage.Appearing += handler;

            await _navigation.PushModalAsync(new NavigationPage(page));

            await tcs.Task;
        }

        #endregion
    }
}
