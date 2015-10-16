using System;
using System.Threading.Tasks;
using TripSketch.Core.Services;
using TripSketch.Core.ViewModels;

namespace TripSketch.Mocks
{
    public class MockNavigator : INavigator
    {
        public Action PopModalAsyncImpl { get; set; }


        public Task PushAsync<TViewModel>(TViewModel viewmodel) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task PushAsync<TViewModel>(Action<TViewModel> customInit = null) where TViewModel : ViewModelBase, new()
        {
            throw new NotImplementedException();
        }

        public Task PopAsync()
        {
            throw new NotImplementedException();
        }

        public Task PushModalAndWaitAsync<TViewModel>(TViewModel viewmodel) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task<TViewModel> PushModalAndWaitAsync<TViewModel>(Action<TViewModel> customInit = null) where TViewModel : ViewModelBase, new()
        {
            throw new NotImplementedException();
        }

        public Task PushModalAsync<TViewModel>(TViewModel viewmodel) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task PushModalAsync<TViewModel>(Action<TViewModel> customInit = null) where TViewModel : ViewModelBase, new()
        {
            throw new NotImplementedException();
        }

        public Task PopModalAsync()
        {
            if (PopModalAsyncImpl != null)
            {
                PopModalAsyncImpl();
                return Task.FromResult(0);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
