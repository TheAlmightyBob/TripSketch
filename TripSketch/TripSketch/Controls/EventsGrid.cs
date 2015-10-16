using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TripSketch.Controls;
using TripSketch.Core.Models;
using TripSketch.Core.ViewModels;
using TripSketch.Events;
using TripSketch.Extensions;
using TripSketch.Views;
using Xamarin.Forms;


namespace TripSketch.Controls
{
    /// <summary>
    /// Displays calendar events in a traditional month-style grid display.
    /// However, the start and end are based on the selected time range, not
    /// the start/end of any given month. This supports managing events that
    /// span months as well as making more efficient use of screen space if
    /// the selected range is less than one month. 
    /// (e.g., a range of 5 days will only consume one row, even if those days
    /// span a weekend. If the user instead would prefer to see Sunday on the left
    /// and Saturday on the right, they can choose their dates accordingly)
    /// 
    /// Supports drag&drop for events.
    /// </summary>
    public class EventsGrid : ContentView
    {
        #region Constants

        private const int _daysInWeek = 7;
        private const double _minRowHeight = 100d;
        private const int _resizeHandleSize = 30;

        #endregion

        #region Fields

        private Grid _grid;
        private int _rows;
        private int _cols;

        private int _defaultMargin;

        private List<DraggableView> _labels;
        private List<AutoStackGrid> _labelContainers;

        private bool _updatingEvent;

        private int _dragLabelDayOffset;

        // Resize mode
        private Event _resizingEvent = null;
        private ContentView _leftResizeHandle = null;
        private ContentView _rightResizeHandle = null;
        private DraggableView _leftResizeHandleDragger = null;
        private DraggableView _rightResizeHandleDragger = null;
        private TimeRange _resizeRange = null;

        #endregion

        #region Bindable Properties

        // Bindable property for the Start Date
        public static readonly BindableProperty StartProperty =
          BindableProperty.Create<EventsGrid, DateTime>(p => p.Start, default(DateTime), propertyChanged: DatesChanged);

        //Gets or sets the Start Date
        public DateTime Start
        {
            get { return (DateTime)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        // Bindable property for the End Date
        public static readonly BindableProperty EndProperty =
          BindableProperty.Create<EventsGrid, DateTime>(p => p.End, default(DateTime), propertyChanged: DatesChanged);

        //Gets or sets the End Date
        public DateTime End
        {
            get { return (DateTime)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        /// <summary>
        /// When Start or End changes and both have been initialized, set up the calendar grid
        /// </summary>
        /// <param name="ob"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void DatesChanged(BindableObject ob, DateTime oldValue, DateTime newValue)
        {
            var eventsGrid = ob as EventsGrid;

            if (eventsGrid == null)
            {
                return;
            }

            if (eventsGrid.Start != default(DateTime) && eventsGrid.End != default(DateTime))
            {
                eventsGrid.BuildCalendarGrid();
            }
        }

        // Bindable property for the Events
        public static readonly BindableProperty EventsProperty =
          BindableProperty.Create<EventsGrid, ObservableCollection<Event>>(p => p.Events, null, propertyChanged: EventsChanged);

        //Gets or sets the Events
        public ObservableCollection<Event> Events
        {
            get { return (ObservableCollection<Event>)GetValue(EventsProperty); }
            set { SetValue(EventsProperty, value); }
        }

        private static void EventsChanged(BindableObject ob, ObservableCollection<Event> oldValue, ObservableCollection<Event> newValue)
        {
            var eventsGrid = ob as EventsGrid;

            if (eventsGrid == null)
            {
                return;
            }

            eventsGrid.OnEventsChange(oldValue, newValue);
        }

        // Bindable property for the Add Event Command
        public static readonly BindableProperty AddEventCommandProperty =
          BindableProperty.Create<EventsGrid, ICommand>(p => p.AddEventCommand, null);

        //Gets or sets the Add Event Command
        public ICommand AddEventCommand
        {
            get { return (ICommand)GetValue(AddEventCommandProperty); }
            set { SetValue(AddEventCommandProperty, value); }
        }

        // Bindable property for the Edit Event Command
        public static readonly BindableProperty EditEventCommandProperty =
          BindableProperty.Create<EventsGrid, ICommand>(p => p.EditEventCommand, null);

        //Gets or sets the Edit Event Command
        public ICommand EditEventCommand
        {
            get { return (ICommand)GetValue(EditEventCommandProperty); }
            set { SetValue(EditEventCommandProperty, value); }
        }

        #endregion

        #region Constructor

        public EventsGrid()
        {
            _labels = new List<DraggableView>();
            _labelContainers = new List<AutoStackGrid>();

            _defaultMargin = Device.OnPlatform(2, 2, 5);
            _dragLabelDayOffset = 0;
        }

        #endregion

        #region Setup

        /// <summary>
        /// Lay out the grid, calendar cells, days of the week. Everything but the events themselves.
        /// </summary>
        private void BuildCalendarGrid()
        {
            var startDate = Start.Date;
            var endDate = End.Date;

            var days = (endDate - startDate).Days;

            _cols = Math.Min(days, _daysInWeek);
            _rows = (int)Math.Ceiling((double)days / _daysInWeek);

            // Clear any existing events, just in case
            ClearEvents();

            _grid = new Grid();

            _grid.ColumnSpacing = 0;
            _grid.RowSpacing = 0;

            // Header
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            // Configure columns and place headers labels (days of the week)
            //
            for (int i = 0; i < _cols; i++)
            {
                // Note that the drag/drop logic has hardcoded assumptions about this configuration, so simply
                // tweaking column definitions here (e.g. to give weekends different width or just make everything
                // a fixed-width) without updating that logic would break things.
                //
                _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new Label();
                label.Text = startDate.AddDays(i).DayOfWeek.ToString();
                label.XAlign = TextAlignment.Center;
                label.LineBreakMode = LineBreakMode.TailTruncation;
                label.FontSize = Device.GetNamedSize(NamedSize.Small, label);

                _grid.Children.Add(label, i, 0);
            }

            // Configure rows
            //
            for (int i = 0; i < _rows; i++)
            {
                // RowDefinition doesn't support MinHeight property yet... 
                // (the MinHeight property is there, just marked internal...)
                //
                var height = (_rows <= 3) ? new GridLength(1, GridUnitType.Star) : new GridLength(_minRowHeight, GridUnitType.Absolute);
                _grid.RowDefinitions.Add(new RowDefinition { Height = height });
            }

            // Add calendar cells for each day
            //
            var day = startDate;

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _cols; col++)
                {
                    var calendarCell = new CalendarCellView { BindingContext = day.Date };

                    var tapGesture = new TapGestureRecognizer();
                    // Bind to the "parent" context (i.e., ours, as opposed to the cell's)
                    tapGesture.BindingContext = this;
                    tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "AddEventCommand");
                    tapGesture.CommandParameter = day.Date;
                    calendarCell.GestureRecognizers.Add(tapGesture);

                    _grid.Children.Add(calendarCell, col, row + 1);

                    day = day.AddDays(1);
                }
            }

            // Create one AutoStackGrid for each row in the main grid
            //
            for (int row = 0; row < _rows; row++)
            {
                var container = new AutoStackGrid();

                //container.IsEnabled = false;
                //container.InputTransparent = true;

                // Extra padding on top to accomodate the date display
                //
                container.Padding = new Thickness(0, 20, 0, 5);

                Grid.SetRow(container, row + 1);
                Grid.SetColumn(container, 0);
                Grid.SetColumnSpan(container, _cols);
                container.Columns = _cols;
                _grid.Children.Add(container);
                _labelContainers.Add(container);
            }

            Content = new ScrollView { Content = _grid };
        }

        #endregion

        #region Property Change Handlers (databinding)
       
        /// <summary>
        /// Clears/places events and sets collection change bindings
        /// </summary>
        /// <param name="oldEvents">Old events</param>
        /// <param name="newEvents">New events</param>
        private void OnEventsChange(ObservableCollection<Event> oldEvents, ObservableCollection<Event> newEvents)
        {
            if (oldEvents != null)
            {
                ClearEvents();

                oldEvents.CollectionChanged -= Events_CollectionChanged;

                foreach (var ev in oldEvents)
                {
                    ev.PropertyChanged -= Event_PropertyChanged;
                }
            }

            if (newEvents != null)
            {
                PlaceEvents();

                newEvents.CollectionChanged += Events_CollectionChanged;
            }
        }

        /// <summary>
        /// Clears/creates event labels according to the change, updating internal labels collection.
        /// </summary>
        /// <param name="sender">Events collection</param>
        /// <param name="e">What changed</param>
        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var oldLabels = _labels.Where(label => e.OldItems.Contains(label.BindingContext)).ToList();

                if (oldLabels.Count > 0)
                {
                    ClearEventLabels(oldLabels);
                    _labels = _labels.Except(oldLabels).ToList();
                }
            }

            if (e.NewItems != null)
            {
                PlaceEvents(e.NewItems.OfType<Event>());
            }
        }

        /// <summary>
        /// Recreates label(s) for an event whenever its Start/End dates change
        /// (unless we're the one that changed them, as specified by the _updatingEvent flag)
        /// </summary>
        /// <param name="sender">The Event</param>
        /// <param name="e">Which property changed</param>
        private void Event_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var ev = sender as Event;

            if (ev == null || _updatingEvent)
            {
                return;
            }

            if (e.PropertyName == "Start" || e.PropertyName == "End")
            {
                // Clear old label(s)
                var labels = _labels.Where(label => label.BindingContext == ev).ToList();
                ClearEventLabels(labels);
                _labels = _labels.Except(labels).ToList();

                // Place new label(s)
                PlaceEvent(ev);
            }
        }

        #endregion

        #region Event Label Placement

        /// <summary>
        /// Remove all events from the grid, reset internal labels collection,
        /// disconnect property change handler.
        /// </summary>
        private void ClearEvents()
        {
            if (_labels != null)
            {
                ClearEventLabels(_labels);
                _labels.Clear();
            }

            if (Events != null)
            {
                foreach (var ev in Events)
                {
                    ev.PropertyChanged -= Event_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Remove the specified labels from the grid.
        /// </summary>
        /// <remarks>
        /// Does not update the internal labels collection.
        /// </remarks>
        private void ClearEventLabels(IList<DraggableView> labels)
        {
            foreach (var label in labels)
            {
                label.Dragged -= HandleDragged;
                label.DragStart -= HandleDragStart;
                label.Dragging -= HandleDragging;
                label.DragCanceled -= HandleDragCanceled;

                var container = label.ParentView as AutoStackGrid;

                if (container != null)
                {
                    container.Children.Remove(label);
                }
            }
        }

        /// <summary>
        /// Create and add labels to the grid for all events, clearing any existing labels.
        /// </summary>
        private void PlaceEvents()
        {
            if (_labels.Count > 0)
            {
                ClearEvents();
            }

            PlaceEvents(Events);
        }

        /// <summary>
        /// Creates labels for events, adds them to grid, and hooks up property change handler.
        /// </summary>
        private void PlaceEvents(IEnumerable<Event> events)
        {
            if (events == null)
            {
                return;
            }

            foreach (var ev in events)
            {
                PlaceEvent(ev);

                ev.PropertyChanged += Event_PropertyChanged;
            }
        }

        /// <summary>
        /// Create one or more labels for an event and place them on the grid.
        /// </summary>
        /// <param name="ev">Event to place</param>
        /// <returns>The placed labels</returns>
        private List<DraggableView> PlaceEvent(Event ev)
        {
            var labels = new List<DraggableView>();

            var offset = (ev.StartDate - Start.Date).Days;
            var row = (int)Math.Truncate((double)offset / _cols);

            var daySpan = ev.DaySpan;
            var rowSpan = (int)Math.Ceiling((double)(offset + daySpan) / _cols);

            // Truncate events that actually start before the selected start date
            //
            bool truncatedStart = false;

            if (offset < 0)
            {
                daySpan += offset;
                offset = 0;
                truncatedStart = true;
            }

            var col = offset % _cols;
            var colSpan = 0;

            System.Diagnostics.Debug.WriteLine($"{ev.Name} has a day span of {daySpan}");

            for (int day = 0; day < daySpan && row < _rows; day += colSpan, row++)
            {
                colSpan = Math.Min(daySpan - day, _cols - col);

                // Margin is adjusted so labels that span rows appear continuous
                //
                var margin = new Thickness(day == 0 && !truncatedStart ? _defaultMargin : 0,
                    _defaultMargin,
                    (day + colSpan < daySpan) ? 0 : _defaultMargin,
                    _defaultMargin);

                labels.Add(PlaceEventLabel(ev, row, col, colSpan, margin));

                col = 0; // only the first row has a column offset
            }

            return labels;
        }

        /// <summary>
        /// Create an individual label corresponding to an event (possibly only part of the
        /// event) and place it on the grid.
        /// </summary>
        private DraggableView PlaceEventLabel(Event ev, int row, int col, int colSpan, Thickness margin = default(Thickness))
        {
            System.Diagnostics.Debug.WriteLine($"Placing {ev.Name} with a span of {colSpan}");

            var view = new DraggableView { BackgroundColor = Color.Blue, VerticalOptions = LayoutOptions.FillAndExpand };
            view.BindingContext = ev;

            var label = new Label();

            label.SetBinding<Event>(Label.TextProperty, e => e.Name);

            label.VerticalOptions = LayoutOptions.Fill;
            label.YAlign = TextAlignment.Center;
            label.FontSize = Device.GetNamedSize(NamedSize.Small, view);
            label.HorizontalOptions = LayoutOptions.FillAndExpand;

            view.Content = label;

            AutoStackGrid.SetMargin(view, margin);

            var container = GetEventContainerForRow(row + 1);

            container.AddChild(view, col, colSpan);

            var tapGesture = new TapGestureRecognizer();
            // First binding is to the "parent" context (i.e., ours, as opposed to the label's)
            //tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, new Binding("EditEventCommand", BindingMode.Default, null, null, null, _vm));
            //tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding("."));

            // Activates resize mode instead of opening editor
            //
            tapGesture.Tapped += HandleEventTapped;

            view.GestureRecognizers.Add(tapGesture);

            view.Dragged += HandleDragged;
            view.DragStart += HandleDragStart;
            view.Dragging += HandleDragging;
            view.DragCanceled += HandleDragCanceled;

            _labels.Add(view);

            return view;
        }

        /// <summary>
        /// Returns the event container corresponding to the specified row.
        /// </summary>
        private AutoStackGrid GetEventContainerForRow(int row)
        {
            return _labelContainers.FirstOrDefault(c => Grid.GetRow(c) == row);
        }

        /// <summary>
        /// Moves an event to a new start date (preserving duration) and updates labels.
        /// </summary>
        /// <param name="ev">Event to move</param>
        /// <param name="newStartDate">New start date for event</param>
        private void MoveEvent(Event ev, DateTime newStartDate)
        {
            var newRange = new TimeRange(ev.Start, ev.End);
            newRange.MoveToDate(newStartDate);
            UpdateEvent(ev, newRange);
        }

        /// <summary>
        /// Applies the new range to the event and updates its label(s)
        /// </summary>
        /// <param name="ev">Event to update</param>
        /// <param name="newRange">Desired time range for the event</param>
        private void UpdateEvent(Event ev, TimeRange newRange)
        {
            // Q: Why don't we just update the properties on the Event and let the property change events handle this?
            // A: Mainly to avoid having to deal with multiple notifications firing for a single change (e.g., moving an event)
            //    Updating the calendar labels is a bit more expensive than just updating a text box.

            if (newRange.Start == ev.Start && newRange.End == ev.End)
            {
                // no change
                return;
            }

            var newCells = GetCellsForRange(newRange.StartDate, newRange.EndDate);

            if (!newCells.Any())
            {
                // Don't allow moving off the calendar entirely
                //
                return;
            }

            _updatingEvent = true;
            ev.Start = newRange.Start;
            ev.End = newRange.End;
            _updatingEvent = false;

            var labels = _labels.Where(l => l.BindingContext == ev).ToList();

            System.Diagnostics.Debug.Assert(labels.Any());

            var label = labels.First();

            bool wasMultiRow = labels.Count > 1;

            var newRows = newCells.Select(cell => Grid.GetRow(cell)).Distinct().ToList();

            bool willBeMultiRow = newRows.Count > 1;

            // If the event fit on a single row before and will still fit on a single row,
            // then we can "simply" move the existing label. Otherwise, we need to recreate,
            // because multiple labels are involved with changing widths etc.
            //
            if (!wasMultiRow && !willBeMultiRow && GetEventContainerForRow(newRows.First()) == label.ParentView)
            {
                var container = label.ParentView as AutoStackGrid;

                // So basically all the previous complexity is being turned into:
                // AutoStackGrids span whole week (row). If the event is being moved
                // within a row, we just reposition it in the stack grid. Otherwise
                // re-create.

                // Note that even with the stack-per-row model, moving items between
                // layout containers is problematic on WinPhone (hence re-creating instead)

                var newCols = newCells.Select(cell => Grid.GetColumn(cell)).OrderBy(col => col).ToList();

                // Defer layout until we're done updating properties
                //
                container.DeferLayout = true;

                AutoStackGrid.SetColumn(label, newCols.First());
                AutoStackGrid.SetColumnSpan(label, newCols.Last() - newCols.First() + 1);

                // Update margin. Even though we're staying on one row, there's the possibility that
                // the event actually stretches before and/or after that row and those parts just aren't visible
                //
                bool truncatedStart = ev.Start < (newCells.First().BindingContext as DateTime?).Value;

                // Subtracting 1 second because an end time of midnight is exclusive and should only
                // label the previous day
                //
                bool truncatedEnd = ev.End.AddSeconds(-1).Date > (newCells.Last().BindingContext as DateTime?).Value;

                var margin = new Thickness(!truncatedStart ? _defaultMargin : 0,
                    _defaultMargin,
                    !truncatedEnd ? _defaultMargin : 0,
                    _defaultMargin);

                AutoStackGrid.SetMargin(label, margin);

                container.DeferLayout = false;
                container.ForceLayout();
            }
            else
            {
                // Recreate labels

                // Removing a label just to create a new one in its place caused movement flicker
                // on WinPhone if we rely on adding/removing to automatically update layout.
                // So that behavior is temporarily disabled on AutoStackGrid, and instead we
                // explicitly call ForceLayout on grids that have been modified.
                //
                var oldContainers = labels.Select(l => l.ParentView).OfType<AutoStackGrid>().ToList();

                foreach (var container in oldContainers)
                {
                    container.DeferLayout = true;
                }

                ClearEventLabels(labels);
                _labels = _labels.Except(labels).ToList();
                var newLabels = PlaceEvent(ev);

                foreach (var container in oldContainers.Union(newLabels.Select(l => l.ParentView)).OfType<AutoStackGrid>())
                {
                    container.ForceLayout();
                    container.DeferLayout = false;
                }
            }
        }

        #endregion

        #region Drag Helpers

        /// <summary>
        /// Sets the opacity for all labels corresponding to a specified Event.
        /// </summary>
        /// <param name="ev">Event to set opacity for</param>
        /// <param name="opacity">Desired opacity</param>
        private void SetEventOpacity(Event ev, double opacity)
        {
            var labels = _labels.Where(l => l.BindingContext == ev).ToList();

            foreach (var label in labels)
            {
                label.Opacity = opacity;
            }
        }

        /// <summary>
        /// Takes the center point from a drag event and calculates the point to use
        /// for hit testing (e.g., we want to use the center of the first day of the
        /// label, not the center of the whole label) relative to the calendar grid.
        /// </summary>
        /// <param name="center">Center point of the dragged label, relative to its immediate parent</param>
        /// <param name="label">Label being dragged</param>
        /// <returns>Calculated destination point</returns>
        private Point GetDragDestinationPoint(Point center, DraggableView label)
        {
            var newCenterPoint = center;
            var colSpan = AutoStackGrid.GetColumnSpan(label);

            newCenterPoint = label.ParentView.TranslatePointToAncestor(newCenterPoint, _grid);

            // Adjust point for ColumnSpan (i.e., it's the starting day we care about)
            //
            if (colSpan > 1)
            {
                var labelWidth = label.Bounds.Width;
                var dayWidth = labelWidth / colSpan;
                var firstDayCenterX = newCenterPoint.X - (colSpan - 1) * dayWidth / 2d;
                newCenterPoint.X = firstDayCenterX;
            }

            return newCenterPoint;
        }

        /// <summary>
        /// Highlights calendar cells matching the specified time range,
        /// also resetting any previous highlights.
        /// </summary>
        /// <param name="start">Starting date to highlight</param>
        /// <param name="end">Ending date to highlight</param>
        private void HighlightCells(DateTime start, DateTime end)
        {
            foreach (var cell in _grid.Children.OfType<CalendarCellView>())
            {
                var date = cell.BindingContext as DateTime?;

                if (date.HasValue)
                {
                    if (date.Value.IsInRange(start, end))
                    {
                        cell.BackgroundColor = Color.Green;
                    }
                    else
                    {
                        cell.BackgroundColor = Color.Transparent;
                    }
                }
            }
        }

        /// <summary>
        /// Reset all calendar cell colors
        /// </summary>
        private void UnhighlightCells()
        {
            foreach (var cell in _grid.Children.OfType<CalendarCellView>())
            {
                cell.BackgroundColor = Color.Transparent;
            }
        }

        /// <summary>
        /// Returns all calendar cells for the specified time range
        /// </summary>
        /// <param name="start">Starting date</param>
        /// <param name="end">Ending date</param>
        private IEnumerable<CalendarCellView> GetCellsForRange(DateTime start, DateTime end)
        {
            return _grid.Children.OfType<CalendarCellView>()
                .Where(cell => (cell.BindingContext as DateTime?)?.IsInRange(start.Date, end.Date) == true);
        }

        /// <summary>
        /// Returns the date on the calendar corresponding to the point.
        /// If the point has a negative X coordinate, this will still return
        /// an adjusted date that would have been there.
        /// </summary>
        /// <param name="point">Point relative to calendar grid</param>
        /// <returns>Matching date, or null if none found (e.g., Y value outside calendar grid)</returns>
        private DateTime? GetDateAtPoint(Point point)
        {
            var colWidth = _grid.Width / _cols;
            var daysOffset = 0;

            // Allow dragging left off the screen to place on the previous row
            // (i.e. if center point is off the left side of the screen, adjust it accordingly)
            //
            while (point.X < 0)
            {
                point.X += colWidth;
                daysOffset++;
            }

            // Perform hit detection
            //
            View cell = _grid.Children.OfType<CalendarCellView>().FirstOrDefault(c => c.Bounds.Contains(point));

            var date = cell?.BindingContext as DateTime?;

            if (date.HasValue)
            {
                date = date.Value.Subtract(TimeSpan.FromDays(daysOffset));
            }

            return date;
        }

        /// <summary>
        /// Get the start date for a specified label (may be later than the start date of the event)
        /// </summary>
        /// <param name="label">Label to get start date for</param>
        /// <returns>Start date, or null if not found</returns>
        private DateTime? GetLabelStartDate(DraggableView label)
        {
            if (label == null)
            {
                return null;
            }

            var column = AutoStackGrid.GetColumn(label);
            var row = Grid.GetRow(label.ParentView);

            var calendarCell = _grid.Children.OfType<CalendarCellView>()
                .FirstOrDefault(cell => Grid.GetRow(cell) == row && Grid.GetColumn(cell) == column);

            return calendarCell?.BindingContext as DateTime?;
        }

        /// <summary>
        /// Returns how many days into an event the specified label starts
        /// (if it's not the first label of the event).
        /// </summary>
        private int GetLabelDayOffset(DraggableView label)
        {
            if (label == null)
            {
                return 0;
            }

            var ev = label.BindingContext as Event;

            if (ev == null)
            {
                return 0;
            }

            var labelDate = GetLabelStartDate(label);

            if (!labelDate.HasValue)
            {
                return 0;
            }

            return (labelDate.Value - ev.Start.Date).Days;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Dims the event's labels to show where it was as it gets dragged
        /// </summary>
        /// <param name="sender">The DraggableView being dragged</param>
        /// <param name="e">EventArgs</param>
        private void HandleDragStart(object sender, EventArgs e)
        {
            var draggedLabel = sender as DraggableView;

            if (draggedLabel != null)
            {
                var ev = draggedLabel.BindingContext as Event;

                // Dim the existing ("old") labels for the event (does not affect the drag visual)
                //
                SetEventOpacity(ev, 0.5);

                _dragLabelDayOffset = GetLabelDayOffset(draggedLabel);
            }
        }

        /// <summary>
        /// Highlights destination cells during drag
        /// </summary>
        private void HandleDragging(object sender, DragEventArgs args)
        {
            var label = sender as DraggableView;

            if (label == null)
            {
                return;
            }

            var ev = label.BindingContext as Event;

            if (ev == null)
            {
                return;
            }

            var destinationPoint = GetDragDestinationPoint(args.Center, label);

            var newDate = GetDateAtPoint(destinationPoint);

            if (newDate.HasValue)
            {
                // Offset for this not being the first label of the event
                newDate = newDate.Value.AddDays(-_dragLabelDayOffset);

                var newRange = new TimeRange(ev.Start, ev.End);
                newRange.MoveToDate(newDate.Value);

                // Highlight target cells, un-highlight previously-targeted cells
                //
                HighlightCells(newRange.StartDate, newRange.EndDate);
            }
        }

        /// <summary>
        /// Moves the dragged event to a new calendar grid cell(s) and updates its start/end dates accordingly.
        /// Uses the cell under the centerpoint of the first day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleDragged(object sender, DragEventArgs args)
        {
            // Reset cell highlights
            UnhighlightCells();

            var label = sender as DraggableView;

            if (label == null)
            {
                return;
            }

            var ev = label.BindingContext as Event;

            if (ev == null)
            {
                return;
            }

            var destinationPoint = GetDragDestinationPoint(args.Center, label);

            // Perform hit detection
            //
            var newDate = GetDateAtPoint(destinationPoint);

            if (newDate.HasValue)
            {
                // Offset for this not being the first label of the event
                newDate = newDate.Value.AddDays(-_dragLabelDayOffset);

                MoveEvent(ev, newDate.Value);
            }

            // Reset opacity
            SetEventOpacity(ev, 1.0);
        }

        /// <summary>
        /// Just reset highlights/opacity
        /// </summary>
        private void HandleDragCanceled(object sender, EventArgs e)
        {
            // Reset cell highlights
            UnhighlightCells();

            var label = sender as DraggableView;

            if (label == null)
            {
                return;
            }

            var ev = label.BindingContext as Event;

            if (ev == null)
            {
                return;
            }

            // Reset opacity
            SetEventOpacity(ev, 1.0);
        }

        /// <summary>
        /// Activates event resize mode
        /// </summary>
        /// <param name="sender">Event label</param>
        private void HandleEventTapped(object sender, EventArgs e)
        {
            var tappedEvent = (sender as View)?.BindingContext as Event;

            if (tappedEvent != null)
            {
                BeginResizeMode(tappedEvent);
            }
        }

        #endregion

        #region Resize mode

        /// <summary>
        /// Creates overlay and tracking handles, highlights cells, and dims other events.
        /// </summary>
        /// <param name="ev">Event to resize</param>
        private void BeginResizeMode(Event ev)
        {
            _resizingEvent = ev;

            _resizeRange = new TimeRange(ev.Start, ev.End);

            // Create overlay to capture taps for dismissing resize mode
            //
            var overlay = new TappableView();
            Grid.SetRowSpan(overlay, _rows + 1);
            Grid.SetColumnSpan(overlay, _cols);
            overlay.BackgroundColor = Color.Transparent;

            overlay.Tapped += HandleOverlayTapped;

            _grid.Children.Add(overlay);

            // Dim everything but this event
            //
            foreach (var label in _labels.Where(l => l.BindingContext != _resizingEvent))
            {
                label.Opacity = 0.5;
            }

            HighlightCells(_resizingEvent.StartDate, _resizingEvent.EndDate);

            var cells = GetCellsForRange(_resizingEvent.StartDate, _resizingEvent.EndDate);
            var firstCell = cells.First();
            var lastCell = cells.Last();

            _leftResizeHandle = CreateResizeHandleVisual(Grid.GetColumn(firstCell), Grid.GetRow(firstCell), false);
            _grid.Children.Add(_leftResizeHandle);
            _rightResizeHandle = CreateResizeHandleVisual(Grid.GetColumn(lastCell), Grid.GetRow(lastCell), true);
            _grid.Children.Add(_rightResizeHandle);

            _leftResizeHandleDragger = CreateResizeHandleDraggerForVisual(_leftResizeHandle);
            _grid.Children.Add(_leftResizeHandleDragger);
            _rightResizeHandleDragger = CreateResizeHandleDraggerForVisual(_rightResizeHandle);
            _grid.Children.Add(_rightResizeHandleDragger);
        }

        /// <summary>
        /// Don't exit resize mode, but reset any pending resize visuals
        /// </summary>
        private void ResetResizeMode()
        {
            _resizeRange = new TimeRange(_resizingEvent.Start, _resizingEvent.End);

            HighlightCells(_resizingEvent.StartDate, _resizingEvent.EndDate);

            var cells = GetCellsForRange(_resizingEvent.StartDate, _resizingEvent.EndDate);
            var firstCell = cells.First();
            var lastCell = cells.Last();

            Grid.SetColumn(_leftResizeHandle, Grid.GetColumn(firstCell));
            Grid.SetRow(_leftResizeHandle, Grid.GetRow(firstCell));
            Grid.SetColumn(_rightResizeHandle, Grid.GetColumn(lastCell));
            Grid.SetRow(_rightResizeHandle, Grid.GetRow(lastCell));

            SynchronizeResizeHandleDraggersWithVisuals();
        }

        /// <summary>
        /// Creates the non-interactive visual that represents the drag handle on the UI.
        /// </summary>
        /// <remarks>
        /// This is separate from the actual dragger so that we can snap its position to cells
        /// rather than following the user's finger, and so that moving it in such controlled jumps
        /// doesn't interfere with the gesture recognition (as it would if we were moving the actual
        /// dragger while it was in the middle of processing a drag).
        /// </remarks>
        /// <param name="column">Column at which to attach the handle</param>
        /// <param name="row">Row at which to attach the handle</param>
        /// <param name="isEnd">True if this is the end time handle, false for start time</param>
        /// <returns>The resize handle visual</returns>
        private ContentView CreateResizeHandleVisual(int column, int row, bool isEnd)
        {
            var handle = new ContentView();
            handle.BackgroundColor = Color.Red;
            Grid.SetColumn(handle, column);
            Grid.SetRow(handle, row);
            handle.HeightRequest = _resizeHandleSize;
            handle.WidthRequest = _resizeHandleSize;
            handle.VerticalOptions = isEnd ? LayoutOptions.End : LayoutOptions.Start;
            handle.HorizontalOptions = isEnd ? LayoutOptions.End : LayoutOptions.Start;

            return handle;
        }

        /// <summary>
        /// Creates an invisible element directly on top of the provided drag handle visual
        /// which will actually process drag gestures.
        /// </summary>
        /// <remarks>
        /// The elements are set up as siblings with matching properties because a parental
        /// relationship would cause moving the visual to break the drag.
        /// This element is invisible because we don't want anything to actually follow the user's
        /// finger... we just want the drag to be rendered in controlled increments.
        /// </remarks>
        /// <param name="handle">A resize handle visual for which to create a drag element</param>
        /// <returns>The drag element</returns>
        private DraggableView CreateResizeHandleDraggerForVisual(View handle)
        {
            var dragger = new DraggableView();
            dragger.BackgroundColor = Color.Transparent;
            dragger.Dragging += HandleResizeDragging;
            dragger.Dragged += HandleResizeDragged;
            dragger.DragCanceled += HandleResizeCanceled;
            Grid.SetColumn(dragger, Grid.GetColumn(handle));
            Grid.SetRow(dragger, Grid.GetRow(handle));
            dragger.HeightRequest = handle.HeightRequest;
            dragger.WidthRequest = handle.WidthRequest;
            dragger.VerticalOptions = handle.VerticalOptions;
            dragger.HorizontalOptions = handle.HorizontalOptions;

            return dragger;
        }

        /// <summary>
        /// Update the position of the draggers to match the new positions of the resize handle visuals
        /// </summary>
        private void SynchronizeResizeHandleDraggersWithVisuals()
        {
            Grid.SetColumn(_rightResizeHandleDragger, Grid.GetColumn(_rightResizeHandle));
            Grid.SetRow(_rightResizeHandleDragger, Grid.GetRow(_rightResizeHandle));

            Grid.SetColumn(_leftResizeHandleDragger, Grid.GetColumn(_leftResizeHandle));
            Grid.SetRow(_leftResizeHandleDragger, Grid.GetRow(_leftResizeHandle));
        }

        /// <summary>
        /// Remove resize mode views
        /// </summary>
        /// <param name="overlay">The resize mode overlay that triggered this, so we can remove it</param>
        private void EndResizeMode(View overlay)
        {
            _grid.Children.Remove(overlay);
            _grid.Children.Remove(_rightResizeHandle);
            _grid.Children.Remove(_rightResizeHandleDragger);
            _grid.Children.Remove(_leftResizeHandle);
            _grid.Children.Remove(_leftResizeHandleDragger);

            ((TappableView)overlay).Tapped -= HandleOverlayTapped;
            _rightResizeHandleDragger.Dragging -= HandleResizeDragging;
            _rightResizeHandleDragger.Dragged -= HandleResizeDragged;
            _rightResizeHandleDragger.DragCanceled -= HandleResizeCanceled;
            _leftResizeHandleDragger.Dragging -= HandleResizeDragging;
            _leftResizeHandleDragger.Dragged -= HandleResizeDragged;
            _leftResizeHandleDragger.DragCanceled -= HandleResizeCanceled;

            // Reset opacity
            foreach (var label in _labels)
            {
                label.Opacity = 1.0;
            }

            UnhighlightCells();

            _resizingEvent = null;
            _rightResizeHandle = null;
            _rightResizeHandleDragger = null;
            _leftResizeHandle = null;
            _leftResizeHandleDragger = null;
            _resizeRange = null;
        }

        /// <summary>
        /// If tap was intended for the event being resized, invokes event editor,
        /// otherwise exits resize mode.
        /// </summary>
        /// <param name="sender">Resize mode overlay</param>
        private void HandleOverlayTapped(object sender, TapEventArgs e)
        {
            var overlay = sender as View;

            if (overlay == null)
            {
                return;
            }

            // Check if this tap was actually intended for the event being resized
            //
            var pointOnGrid = overlay.TranslatePointToAncestor(e.Point, _grid);

            var rowContainer = _labelContainers.FirstOrDefault(lc => lc.Bounds.Contains(pointOnGrid));
            View label = null;

            if (rowContainer != null)
            {
                var pointOnContainer = _grid.TranslatePointToDescendent(pointOnGrid, rowContainer);
                label = rowContainer.Children.FirstOrDefault(l => l.BindingContext == _resizingEvent && l.Bounds.Contains(pointOnContainer));
            }

            if (label != null)
            {
                EditEventCommand?.Execute(label.BindingContext);
            }
            else
            {
                EndResizeMode(sender as View);
            }
        }

        /// <summary>
        /// Updates cell highlights and resize handle visual position
        /// </summary>
        /// <param name="sender">Resize handle dragger</param>
        /// <param name="args">Drag event args</param>
        private void HandleResizeDragging(object sender, DragEventArgs args)
        {
            var dragger = sender as DraggableView;

            if (dragger == null)
            {
                return;
            }

            // Actually, this time we don't care about the center so much as the edge.... but the difference isn't too noticeable

            var destinationPoint = dragger.ParentView.TranslatePointToAncestor(args.Center, _grid);

            var newDate = GetDateAtPoint(destinationPoint);

            if (newDate == null)
            {
                return;
            }

            // We re-set newDate after applying it to the resize range because TimeRange
            // may have adjusted it to preserve a valid time range
            //
            if (dragger == _rightResizeHandleDragger)
            {
                _resizeRange.EndDate = newDate.Value;
                newDate = _resizeRange.EndDate;
            }
            else
            {
                _resizeRange.StartDate = newDate.Value;
                newDate = _resizeRange.StartDate;
            }

            // Highlight new date range
            // 
            HighlightCells(_resizeRange.StartDate, _resizeRange.EndDate);

            // Move drag handle visual to the new date cell
            //
            var cell = _grid.Children.OfType<CalendarCellView>().FirstOrDefault(c => (DateTime)c.BindingContext == newDate.Value.Date);

            if (cell != null)
            {
                var handleVisual = dragger == _rightResizeHandleDragger ? _rightResizeHandle : _leftResizeHandle;
                Grid.SetColumn(handleVisual, Grid.GetColumn(cell));
                Grid.SetRow(handleVisual, Grid.GetRow(cell));
            }
        }

        /// <summary>
        /// Resize the event and reset the resize mode state
        /// </summary>
        /// <param name="sender">Resize handle dragger</param>
        /// <param name="args">Drag event args</param>
        private void HandleResizeDragged(object sender, DragEventArgs args)
        {
            var dragger = sender as DraggableView;

            if (dragger == null)
            {
                return;
            }

            SynchronizeResizeHandleDraggersWithVisuals();

            // We were already updating _resizeRange during the drag, so now we just apply
            // (even on the off-chance that the Dragged coordinate differs significantly from the last 
            //  Dragging coordinate, it seems like we should respect the resize preview visual to
            //  avoid surprising the user)
            //
            UpdateEvent(_resizingEvent, _resizeRange);
        }

        private void HandleResizeCanceled(object sender, EventArgs e)
        {
            ResetResizeMode();
        }

        #endregion
    }
}
