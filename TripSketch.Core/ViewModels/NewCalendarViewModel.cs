using System.Windows.Input;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class NewCalendarViewModel : ModalViewModelBase
    {
        #region Fields

        private string _calendarName;
        private Command _doneCommand;

        #endregion

        #region Properties

        public string Title { get { return "Add Calendar"; } }

        public string CalendarName
        {
            get { return _calendarName; }
            set
            {
                if (_calendarName != value)
                {
                    _calendarName = value;
                    HasChanged("CalendarName");
                    _doneCommand.ChangeCanExecute();
                }
            }
        }

        public ICommand DoneCommand { get { return _doneCommand; } }

        #endregion

        #region Constructor

        public NewCalendarViewModel()
        {
            _doneCommand = new Command(Done, () => !string.IsNullOrWhiteSpace(_calendarName));
        }

        #endregion
    }
}
