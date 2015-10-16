using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class NewTripViewModel : ModalViewModelBase
    {
        #region Fields

        private string _name;
        private DateTime _start;
        private DateTime _end;

        private Trip _trip;

        private Command _doneCommand;

        #endregion

        #region Properties

        public string Title { get { return "New Trip"; } }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    HasChanged();
                    _doneCommand.ChangeCanExecute();
                }
            }
        }

        public DateTime Start
        {
            get { return _start; }
            set
            {
                if (_start != value)
                {
                    _start = value;
                    HasChanged();

                    if (_end <= _start)
                    {
                        End = _start.AddDays(1);
                    }
                }
            }
        }

        public DateTime End
        {
            get { return _end; }
            set
            {
                if (_end != value)
                {
                    _end = value;
                    HasChanged();
                }
            }
        }

        public Trip Trip
        {
            get { return _trip; }
            set
            {
                if (_trip != value)
                {
                    _trip = value;
                    Name = _trip.Name;
                    Start = _trip.Start;
                    End = _trip.End;
                }
            }
        }

        public ICommand DoneCommand { get { return _doneCommand; } }

        #endregion

        #region Constructor

        public NewTripViewModel()
        {
            _doneCommand = new Command(Done, () => !string.IsNullOrWhiteSpace(_name));
            _start = DateTime.Today;
            _end = DateTime.Today.AddMonths(1);
        }

        #endregion

        #region Private Methods

        protected override void Done()
        {
            if (_end <= _start)
            {
                ReportMessage("Start must precede End", "Time travel is not supported");
                return;
            }

            Result = ModalResult.Done;

            _trip = new Trip
            {
                Name = _name,
                Start = _start,
                End = _end
            };

            Navigator.PopModalAsync();
        }

        #endregion
    }
}
