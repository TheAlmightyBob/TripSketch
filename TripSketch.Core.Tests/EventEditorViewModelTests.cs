using NUnit.Framework;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using TripSketch.Core.ViewModels;
using TripSketch.Mocks;

namespace TripSketch.Core.Tests
{
    [TestFixture]
    public class EventEditorViewModelTests
    {
        private EventEditorViewModel _vm;
        private bool _popped;

        [SetUp]
        public void Setup()
        {
            _popped = false;

            var navigator = new MockNavigator
            {
                PopModalAsyncImpl = () => _popped = true
            };
            var reporting = new MockReportingService
            {
                ReportExceptionImpl = ex => {},
                ReportMessageImpl = (msg, deets) => {}
            };

            _vm = new EventEditorViewModel
            {
                Navigator = navigator,
                ReportingService = reporting
            };
        }

        [Test]
        public void EventEditor_Done_IsDisabledIfUnnamed()
        {
            Assert.IsFalse(_vm.DoneCommand.CanExecute(null));
        }

        [Test]
        public void EventEditor_Done_IsEnabledIfNamed()
        {
            _vm.Name = "Bob";
            Assert.IsTrue(_vm.DoneCommand.CanExecute(null));
        }

        [Test]
        public void EventEditor_Done_DismissesAndSetsResults()
        {
            _vm.Name = "Bob";
            _vm.DoneCommand.Execute(null);

            Assert.IsTrue(_popped);
            Assert.AreEqual(ModalResult.Done, _vm.Result);
            Assert.IsNotNull(_vm.Event);
            Assert.AreEqual("Bob", _vm.Event.Name);
        }

        [Test]
        public void EventEditor_Done_ValidatesDates()
        {
            _vm.Name = "Bob";
            _vm.End = _vm.Start.AddDays(-1);

            Assert.IsTrue(_vm.DoneCommand.CanExecute(null));

            _vm.DoneCommand.Execute(null);

            Assert.IsFalse(_popped);
        }

        [Test]
        public void EventEditor_Cancel_Cancels()
        {
            _vm.Name = "Bob";

            _vm.CancelCommand.Execute(null);

            Assert.IsTrue(_popped);
            Assert.AreEqual(ModalResult.Canceled, _vm.Result);
            Assert.IsNull(_vm.Event);
        }

        [Test]
        public void EventEditor_IsEditing_IsAccurate()
        {
            Assert.IsFalse(_vm.IsEditing);

            _vm.Event = new Event { Name = "Bob" };

            Assert.IsTrue(_vm.IsEditing);
        }

        [Test]
        public async void EventEditor_SettingEvent_UpdatesProperties()
        {
            var ev = new Event { Name = "Bob" };

            await _vm.WaitForPropertyChangeAsync(() => _vm.Name, () =>
            {
                _vm.Event = ev;
            });

            Assert.AreEqual(ev.Name, _vm.Name);
        }

        [Test]
        public void EventEditor_Delete_SetsResult()
        {
            _vm.Name = "Bob";

            _vm.DeleteCommand.Execute(null);

            Assert.IsTrue(_popped);
            Assert.AreEqual(ModalResult.Deleted, _vm.Result);
            Assert.IsNull(_vm.Event);
        }
    }
}
