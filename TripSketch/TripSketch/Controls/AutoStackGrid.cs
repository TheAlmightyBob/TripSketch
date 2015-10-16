using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using TripSketch.Extensions;
using System.ComponentModel;

namespace TripSketch.Controls
{
    /// <summary>
    /// A Grid-like layout that automatically creates rows as necessary to prevent overlap, but tries
    /// to use the minimum required number of rows.
    /// Columns and column spans are explicitly assigned via attached properties.
    /// All rows and columns are treated as *-sized.
    /// </summary>
    public class AutoStackGrid : Layout<View>
    {
        #region Fields

        private int _cols = 1;
        private int _maxRows = 1;

        private List<int> _rowsPerCol = new List<int> { 1 };
        private Dictionary<View, int> _rowAssignments = new Dictionary<View, int>();

        #endregion

        #region Attached Properties

        // Bindable attached property for margin
        public static readonly BindableProperty MarginProperty =
          BindableProperty.CreateAttached<AutoStackGrid, Thickness>(child => GetMargin(child), default(Thickness));

        public static Thickness GetMargin(BindableObject child)
        {
            return (Thickness)child.GetValue(MarginProperty);
        }

        public static void SetMargin(BindableObject child, Thickness value)
        {
            child.SetValue(MarginProperty, value);
        }

        // Bindable attached property for column span
        public static readonly BindableProperty ColumnSpanProperty =
          BindableProperty.CreateAttached<AutoStackGrid, int>(child => GetColumnSpan(child), 1);

        public static int GetColumnSpan(BindableObject child)
        {
            return (int)child.GetValue(ColumnSpanProperty);
        }

        public static void SetColumnSpan(BindableObject child, int value)
        {
            child.SetValue(ColumnSpanProperty, value);
        }

        // Bindable attached property for column
        public static readonly BindableProperty ColumnProperty =
          BindableProperty.CreateAttached<AutoStackGrid, int>(child => GetColumn(child), 1);

        public static int GetColumn(BindableObject child)
        {
            return (int)child.GetValue(ColumnProperty);
        }

        public static void SetColumn(BindableObject child, int value)
        {
            child.SetValue(ColumnProperty, value);
        }

        #endregion

        #region Bindable Properties

        // Bindable property for column count
        public static readonly BindableProperty ColumnsProperty =
          BindableProperty.Create<AutoStackGrid, int>(p => p.Columns, 1, propertyChanged: ColumnsChanged);

        // Gets or sets the column count
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static void ColumnsChanged(BindableObject ob, int oldValue, int newValue)
        {
            var grid = ob as AutoStackGrid;

            if (grid != null)
            {
                grid.CalculateGridLayout();
            }
        }

        // Bindable property for defer layout
        public static readonly BindableProperty DeferLayoutProperty =
          BindableProperty.Create<AutoStackGrid, bool>(p => p.DeferLayout, false);

        // Gets or sets whether to defer layout
        // (note that not all operations respect this)
        public bool DeferLayout
        {
            get { return (bool)GetValue(DeferLayoutProperty); }
            set { SetValue(DeferLayoutProperty, value); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a mapping of column numbers to the views that span them.
        /// The same view may appear multiple times according to its column span.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<View>> GroupChildrenByColumn()
        {
            var colStacks = new Dictionary<int, List<View>>();

            foreach (var child in Children)
            {
                var col = GetColumn(child);
                var colSpan = GetColumnSpan(child);

                for (int i = 0; i < colSpan; i++)
                {
                    List<View> stack = null;

                    if (colStacks.TryGetValue(col + i, out stack))
                    {
                        stack.Add(child);
                    }
                    else
                    {
                        colStacks.Add(col + i, new List<View> { child });
                    }
                }
            }

            return colStacks;
        }

        /// <summary>
        /// Convenience method for adding a child view with placement attributes
        /// </summary>
        /// <param name="child">View to add as a child of this AutoStackGrid</param>
        /// <param name="column">Column at which to place the child view</param>
        /// <param name="columnSpan">Column span for the child view</param>
        public void AddChild(View child, int column, int columnSpan)
        {
            SetColumn(child, column);
            SetColumnSpan(child, columnSpan);
            Children.Add(child);
        }

        #endregion

        #region Overrides

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            double cellWidth = width / _cols;

            // Place children
            //
            foreach (var child in Children)
            {
                var col = GetColumn(child);
                var row = _rowAssignments.GetValueOrDefault(child, 1);
                var rows = _rowsPerCol.ElementAt(col);
                var childWidth = GetColumnSpan(child) * cellWidth;
                var childHeight = height / rows;
                var offsetX = x + cellWidth * col;
                var offsetY = y + childHeight * row;
                var margin = GetMargin(child);
                
                // apply margin
                //
                offsetX += margin.Left;
                offsetY += margin.Top;
                childWidth -= margin.HorizontalThickness;
                childHeight -= margin.VerticalThickness;

                LayoutChildIntoBoundingRegion(child, new Rectangle(offsetX, offsetY, childWidth, childHeight));
            }
        }

        /// <summary>
        /// Since we use uniform row/column sizing, we just find the largest row/column height/width
        /// out of all the children.
        /// </summary>
        /// <param name="widthConstraint"></param>
        /// <param name="heightConstraint"></param>
        /// <returns></returns>
        protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
        {
            var rowHeight = 0d;
            var colWidth = 0d;

            foreach (var child in Children)
            {
                var childSizeRequest = child.GetSizeRequest(widthConstraint / _cols, heightConstraint / _maxRows);
                var margin = GetMargin(child);
                var colSpan = GetColumnSpan(child);

                rowHeight = Math.Max(rowHeight, childSizeRequest.Request.Height + margin.VerticalThickness);
                colWidth = Math.Max(colWidth, childSizeRequest.Request.Width / colSpan + margin.HorizontalThickness);
            }

            return new SizeRequest(new Size(colWidth * _cols, rowHeight * _maxRows));
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            child.PropertyChanged += Child_PropertyChanged;

            CalculateGridLayout();
        }

        protected override void OnChildRemoved(Element child)
        {
            base.OnChildRemoved(child);

            child.PropertyChanged -= Child_PropertyChanged;

            CalculateGridLayout();
        }

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return !DeferLayout;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return !DeferLayout;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines how many rows/columns are required and assigns rows to children.
        /// </summary>
        private void CalculateGridLayout()
        {
            var cols = 1;
            var rows = 1;
            var rowsPerCol = new List<int>();
            var rowAssignments = new Dictionary<View, int>();

            var colStacks = GroupChildrenByColumn();

            if (colStacks.Count > 0)
            {
                cols = colStacks.Max(kvp => kvp.Key) + 1;
                rows = colStacks.Max(kvp => kvp.Value.Count);
            }

            int currentRows = 1;
            int startingCol = 0;

            // Calculate grid layout
            //
            foreach (var kvp in colStacks.OrderBy(kvp => kvp.Key))
            {
                // We set rowsPerCol in sections... with section breaks every time we
                // reach a column that does not carry over any views from previous columns...
                // at which point we record the number of rows needed for each column in that last section
                // 
                if (!kvp.Value.Intersect(rowAssignments.Keys).Any())
                {
                    for (int col = startingCol; col < kvp.Key; col++)
                    {
                        rowsPerCol.Add(currentRows);
                    }

                    currentRows = 1;
                    startingCol = kvp.Key;
                }

                foreach (var child in kvp.Value.Except(rowAssignments.Keys))
                {
                    var colSpan = GetColumnSpan(child);
                    var row = 0;

                    var occupiedRows = rowAssignments.Where(viewRow => kvp.Value.Contains(viewRow.Key))
                        .Select(viewRow => viewRow.Value).Distinct().ToList();

                    while (occupiedRows.Contains(row))
                        row++;

                    System.Diagnostics.Debug.Assert(row < rows);

                    rowAssignments.Add(child, row);

                    currentRows = Math.Max(currentRows, row + 1);
                }
            }

            // Record the last section
            //
            for (int col = startingCol; col < cols; col++)
            {
                rowsPerCol.Add(currentRows);
            }

            _cols = Columns;
            _maxRows = rows;
            _rowsPerCol = rowsPerCol;
            _rowAssignments = rowAssignments;
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ColumnProperty.PropertyName || e.PropertyName == ColumnSpanProperty.PropertyName)
            {
                CalculateGridLayout();

                if (!DeferLayout)
                {
                    InvalidateLayout();
                }
            }
            else if (e.PropertyName == MarginProperty.PropertyName && !DeferLayout)
            {
                InvalidateLayout();
            }
        }

        #endregion
    }
}
