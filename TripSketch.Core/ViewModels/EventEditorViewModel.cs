using System;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class EventEditorViewModel : ModalViewModelBase
    {
        #region Fields

        private string _name;
        private DateTime _start;
        private DateTime _end;

        private Event _event;

        private Command _doneCommand;

        #endregion

        #region Properties

        public string Title { get { return "Edit Event"; } }

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

        public Event Event
        {
            get { return _event; }
            set
            {
                if (_event != value)
                {
                    _event = value;

                    Name = _event.Name;
                    Start = _event.Start;
                    End = _event.End;
                }
            }
        }

        public bool IsEditing { get { return _event != null; } }

        public ICommand DoneCommand { get { return _doneCommand; } }

        #endregion

        #region Constructor

        public EventEditorViewModel()
        {
            _doneCommand = new Command(Done, () => !string.IsNullOrWhiteSpace(_name));
            _start = DateTime.Today;
            _end = _start.AddDays(1);
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
            
            if (_event == null)
            {
                _event = new Event { AllDay = true };
            }

            _event.Name = Name;
            _event.Start = Start;
            _event.End = End;

            Navigator.PopModalAsync();
        }

        #endregion
    }
}
