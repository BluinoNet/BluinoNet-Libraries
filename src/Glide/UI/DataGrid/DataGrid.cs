////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using Glide;
using nanoFramework.Presentation.Media;
using nanoFramework.UI;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The DataGrid component is a list-based component that provides a grid of rows and columns.
    /// </summary>
    public class DataGrid : DisplayObject
    {
        private ArrayList _columns = new ArrayList();
        private ArrayList _rows = new ArrayList();

        private Bitmap _headers;
        private Bitmap _items;
        private Bitmap _DataGridIcon_Asc = Resources.GetBitmap(Resources.BitmapResources.DataGridIcon_Asc);
        private Bitmap _DataGridIcon_Desc = Resources.GetBitmap(Resources.BitmapResources.DataGridIcon_Desc);

        private bool _renderHeaders;
        private bool _renderItems;
        private int _scrollIndex;
        private bool _pressed = false;
        private bool _moving = false;
        private int _selectedIndex = -1;
        private DataGridItem _selectedItem;
        private int _selectedDataGridColumnIndex;

        private int _listY;
        private int _listMaxY;
        private int _lastListY;
        private int _lastTouchY;
        private int _ignoredTouchMoves;
        private int _maxIgnoredTouchMoves;

        private bool _showHeaders;
        private int _rowCount;
        private int _scrollbarHeight;
        private int _scrollbarTick;
        private DataGridItemComparer _comparer = new DataGridItemComparer();
        private Order _order;

        /// <summary>
        /// Creates a new DataGrid component.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="alpha">alpha</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="width">width</param>
        /// <param name="rowHeight">rowHeight</param>
        /// <param name="rowCount">rowCount</param>
        public DataGrid(string name, ushort alpha, int x, int y, int width, int rowHeight, int rowCount)
        {
            Name = name;
            Alpha = 255;
            X = x;
            Y = y;
            Width = width;
            RowHeight = rowHeight;
            // Setting the RowCount changes the Height.
            RowCount = rowCount;

            // Show headers is on by default.
            // When the headers are shown the row count is decreased by one.
            ShowHeaders = true;

            _headers = new Bitmap(Width, RowHeight);

            Clear();
        }

        // Events

        /// <summary>
        /// Tap grid event.
        /// </summary>
        public event OnTapCell TapCellEvent;

        /// <summary>
        /// Triggers a tap cell event.
        /// </summary>
        /// <param name="sender">Object associated with this event.</param>
        /// <param name="args">Tap cell event arguments.</param>
        public void TriggerTapCellEvent(object sender, TapCellEventArgs args)
        {
            if (TapCellEvent != null)
                TapCellEvent(sender, args);
        }

        private void RenderHeaders()
        {
            int x = 0;

            _headers.DrawRectangle(0, 0, x, 0, Width, _headers.Height, 0, 0, HeadersBackColor, 0, 0, 0, 0, 0, 255);

            DataGridColumn dataGridColumn;
            for (int j = 0; j < _columns.Count; j++)
            {
                dataGridColumn = (DataGridColumn)_columns[j];

                // Draw text
                _headers.DrawTextInRect(dataGridColumn.Label, x + 5, (_headers.Height - Font.Height) / 2, dataGridColumn.Width, _headers.Height, 0, HeadersFontColor, Font);

                // If we're on the selected column draw the icon.
                if (j == _selectedDataGridColumnIndex)
                {
                    int width = 0, height = 0;
                    Font.ComputeExtent(dataGridColumn.Label, out width, out height);

                    if (dataGridColumn.Order == Order.ASC)
                        _headers.DrawImage(x + 10 + width, 5, _DataGridIcon_Asc, 0, 0, _DataGridIcon_Asc.Width, _DataGridIcon_Asc.Height);
                    else
                        _headers.DrawImage(x + 10 + width, 5, _DataGridIcon_Desc, 0, 0, _DataGridIcon_Desc.Width, _DataGridIcon_Desc.Height);
                }

                x += dataGridColumn.Width;
            }
        }

        private void UpdateItem(int index, bool clear)
        {
            if (clear)
                RenderItemClear(index);

            if (index == _selectedIndex)
                RenderItem(_selectedIndex, ((DataGridItem)_rows[index]).Data, SelectedItemBackColor, SelectedItemFontColor);
            else
            {
                if (index % 2 == 0)
                    RenderItem(index, ((DataGridItem)_rows[index]).Data, ItemsBackColor, ItemsFontColor);
                else
                    RenderItem(index, ((DataGridItem)_rows[index]).Data, ItemsAltBackColor, ItemsFontColor);
            }
        }

        private void RenderItem(int index, object[] data, Color backColor, Color fontColor)
        {
            int x = 0;
            int y = index * RowHeight;

            _items.DrawRectangle(0, 0, x, y, Width, RowHeight, 0, 0, backColor, 0, 0, 0, 0, 0, 255);

            DataGridColumn dataGridColumn;
            for (int i = 0; i < _columns.Count; i++)
            {
                dataGridColumn = (DataGridColumn)_columns[i];

                _items.DrawTextInRect(data[i].ToString(), x + 5, y + (RowHeight - Font.Height) / 2, dataGridColumn.Width, RowHeight, 0, fontColor, Font);
                _items.DrawLine(GridColor, 1, x, y, x, y + RowHeight);

                x += dataGridColumn.Width;
            }
        }

        private void RenderItemClear(int index)
        {
            // HACK: This is done to prevent image/color retention.
            _items.DrawRectangle(0, 0, 0, index * RowHeight, Width, RowHeight, 0, 0, 0, 0, 0, 0, 0, 0, 255);
        }

        private void RenderEmpty()
        {
            _items.DrawRectangle(0, 0, 0, 0, _items.Width, _items.Height, 0, 0, ItemsBackColor, 0, 0, 0, 0, 0, 255);

            int x = 0;
            for (int i = 0; i < _columns.Count; i++)
            {
                _items.DrawLine(GridColor, 1, x, 0, x, _items.Height);
                x += ((DataGridColumn)_columns[i]).Width;
            }
        }

        /// <summary>
        /// Renders the DataGrid onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            if (ShowHeaders && _renderHeaders)
            {
                _renderHeaders = false;
                RenderHeaders();
            }

            if (_renderItems)
            {
                _renderItems = false;

                // Only recreate the items Bitmap if necessary
                if (_items == null || _items.Height != _rows.Count * RowHeight)
                {
                    if (_items != null)
                        _items.Dispose();

                    if (_rows.Count < _rowCount)
                    {
                        _items = new Bitmap(Width, _rowCount * RowHeight);
                        RenderEmpty();
                    }
                    else
                        _items = new Bitmap(Width, _rows.Count * RowHeight);
                }
                else
                    _items.DrawRectangle(0, 0, 0, 0, Width, _items.Height, 0, 0, 0, 0, 0, 0, 0, 0, 255);

                if (_rows.Count > 0)
                {
                    for (int i = 0; i < _rows.Count; i++)
                        UpdateItem(i, false);
                }
            }

            int x = Parent.X + X;
            int y = Parent.Y + Y;

            if (_showHeaders)
            {
                Parent.Graphics.DrawImage(x, y, _headers, 0, 0, Width, RowHeight);
                y += RowHeight;
            }

            Parent.Graphics.DrawImage(x, y, _items, 0, _listY, Width, _rowCount * RowHeight);

            if (ShowScrollbar)
            {
                _scrollbarHeight = RowHeight;
                int travel = Height - _scrollbarHeight;

                if (_rows.Count > 0)
                    _scrollbarTick = (int)System.Math.Round((double)(travel / _rows.Count));
                else
                    _scrollbarTick = 0;

                _scrollbarHeight = Height - ((_rows.Count - _rowCount) * _scrollbarTick);

                x += Width - ScrollbarWidth;
                y = Parent.Y + Y;

                Parent.Graphics.DrawRectangle(0, 0, x, y, ScrollbarWidth, Height, 0, 0, ScrollbarBackColor, 0, 0, 0, 0, 0, 255);

                // Only show the scrollbar scrubber if it's smaller than the Height
                if (_scrollbarHeight < Height)
                    Parent.Graphics.DrawRectangle(0, 0, x, y + (_scrollIndex * _scrollbarTick), ScrollbarWidth, _scrollbarHeight, 0, 0, ScrollbarScrubberColor, 0, 0, 0, 0, 0, 255);
            }
        }

        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            if (Rect.Contains(e.Point) && _rows.Count > 0)
            {
                _pressed = true;
                _lastTouchY = e.Point.Y;
                _lastListY = _listY;
                e.StopPropagation();
            }

            return e;
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchUp(TouchEventArgs e)
        {
            if (!_pressed)
                return e;

            if (!_moving && Rect.Contains(e.Point))
            {
                int x = Parent.X + X;
                int y = Parent.Y + Y;
                int index = ((_listY + e.Point.Y) - y) / RowHeight;
                int rowIndex = index;
                // If headers are present the rowIndex needs to be offset
                if (ShowHeaders)
                    rowIndex--;

                int columnIndex = 0;
                DataGridColumn dataGridColumn;
                Rectangle rect;

                for (int i = 0; i < _columns.Count; i++)
                {
                    dataGridColumn = (DataGridColumn)_columns[i];
                    rect = new Rectangle(x, y, dataGridColumn.Width, Height);

                    if (rect.Contains(e.Point))
                    {
                        columnIndex = i;
                        break;
                    }
                    
                    x += dataGridColumn.Width;
                }

                if (index == 0 && ShowHeaders && SortableHeaders)
                {
                    Sort(columnIndex);
                    Invalidate();
                }
                else
                {
                    if (TappableCells)
                    {
                        SelectedIndex = rowIndex;
                        TriggerTapCellEvent(this, new TapCellEventArgs(columnIndex, rowIndex));
                    }
                }
            }

            _pressed = false;
            _moving = false;
            _ignoredTouchMoves = 0;
            return e;
        }

        /// <summary>
        /// Handles the touch move event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchMove(TouchEventArgs e)
        {
            if (!Draggable || !_pressed || _rows.Count <= RowCount)
                return e;

            if (!_moving)
            {
                if (_ignoredTouchMoves < _maxIgnoredTouchMoves)
                    _ignoredTouchMoves++;
                else
                {
                    _ignoredTouchMoves = 0;
                    _moving = true;
                }
            }
            else
            {
                _listY = _lastListY - (e.Point.Y - _lastTouchY);
                _listY = GlideUtils.Math.MinMax(_listY, 0, _listMaxY);

                _scrollIndex = (int)System.Math.Ceiling((double)_listY / RowHeight);

                Invalidate();
                e.StopPropagation();
            }

            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            if (_headers != null)
                _headers.Dispose();

            if (_items != null)
                _items.Dispose();

            _DataGridIcon_Asc.Dispose();
            _DataGridIcon_Desc.Dispose();
        }

        private bool RowIndexIsValid(int index)
        {
            return (_rows.Count > 0 && index > -1 && index < _rows.Count);
        }

        private bool ColumnIndexIsValid(int index)
        {
            return (_columns.Count > 0 && index > -1 && index < _columns.Count);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="dataGridColumn">dataGridColumn</param>
        public void AddColumn(DataGridColumn dataGridColumn)
        {
            AddColumnAt(-1, dataGridColumn);
        }

        /// <summary>
        /// Adds a column at a specified index.
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="dataGridColumn">dataGridColumn</param>
        public void AddColumnAt(int index, DataGridColumn dataGridColumn)
        {
            if (_columns.Count == 0 || index == -1 || index > _columns.Count - 1)
                _columns.Add(dataGridColumn);
            else
                _columns.Insert(index, dataGridColumn);

            // A new column was added so we must re-render the headers.
            _renderHeaders = true;
        }

        /// <summary>
        /// Removes a column.
        /// </summary>
        /// <param name="dataGridColumn">dataGridColumn</param>
        public void RemoveColumn(DataGridColumn dataGridColumn)
        {
            int index = _columns.IndexOf(dataGridColumn);
            if (index > -1)
                RemoveColumnAt(index);
        }

        /// <summary>
        /// Removes a column at a specified index.
        /// </summary>
        /// <param name="index">index</param>
        public void RemoveColumnAt(int index)
        {
            if (ColumnIndexIsValid(index))
            {
                _columns.RemoveAt(index);
                _renderHeaders = true;
            }
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="dataGridItem">dataGridItem</param>
        public void AddItem(DataGridItem dataGridItem)
        {
            AddItemAt(-1, dataGridItem);
        }

        /// <summary>
        /// Adds an item at a specified index.
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="dataGridItem">dataGridItem</param>
        public void AddItemAt(int index, DataGridItem dataGridItem)
        {
            if (dataGridItem.Data.Length != _columns.Count)
                throw new ArgumentException("The DataGridRow data length does not match the DataGrid's column count.", "dataGridRow");

            if (_rows.Count == 0 || index == -1 || index > _rows.Count - 1)
                _rows.Add(dataGridItem);
            else
                _rows.Insert(index, dataGridItem);

            // A new row was added so we must re-render the items.
            _renderItems = true;

            // Calculate the max list Y position
            _listMaxY = _rows.Count * RowHeight;
            if (ShowHeaders) _listMaxY += RowHeight;
            _listMaxY -= Height;
        }

        /// <summary>
        /// Removes an item.
        /// </summary>
        /// <param name="dataGridItem">dataGridItem</param>
        public void RemoveItem(DataGridItem dataGridItem)
        {
            int index = _rows.IndexOf(dataGridItem);
            if (index > -1)
                RemoveItemAt(index);
        }

        /// <summary>
        /// Removes an item a specified index.
        /// </summary>
        /// <param name="index">index</param>
        public void RemoveItemAt(int index)
        {
            if (RowIndexIsValid(index))
            {
                _rows.RemoveAt(index);

                // A row was removed so we must re-render the items.
                _renderItems = true;

                // If the rows fall below the selected index
                // default to no selection
                if (_rows.Count - 1 < SelectedIndex)
                    SelectedIndex = -1;

                // If the index removed was the selected index
                // revert to no selection
                if (SelectedIndex == index)
                    SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Scroll the rows up by a specified amount.
        /// </summary>
        /// <param name="amount">amount</param>
        public void ScrollUp(int amount)
        {
            if (_rows.Count == 0)
                return;

            if (_scrollIndex - amount >= 0)
                _scrollIndex -= amount;
            else
                _scrollIndex = 0;

            _listY = _scrollIndex * RowHeight;
        }

        /// <summary>
        /// Scroll the rows down by a specified amount.
        /// </summary>
        /// <param name="amount">amount</param>
        public void ScrollDown(int amount)
        {
            if (_rows.Count == 0)
                return;

            if (_scrollIndex + amount < _rows.Count - _rowCount)
                _scrollIndex += amount;
            else
                _scrollIndex = _rows.Count - _rowCount;

            _listY = _scrollIndex * RowHeight;
        }

        /// <summary>
        /// Scroll the rows to a specified index.
        /// </summary>
        /// <param name="index">index</param>
        public void ScrollTo(int index)
        {
            if (RowIndexIsValid(index))
            {
                _scrollIndex = index;
                _listY = _scrollIndex * RowHeight;
            }
        }

        /// <summary>
        /// Sets new row data.
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="data">Data object array.</param>
        public void SetRowData(int index, object[] data)
        {
            if (RowIndexIsValid(index))
            {
                if (data.Length != _columns.Count)
                    throw new ArgumentException("The data length does not match the DataGrid's column count.", "data");

                ((DataGridItem)_rows[index]).Data = data;
                UpdateItem(index, true);
            }
        }

        /// <summary>
        /// Gets row data.
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>Data object array.</returns>
        public object[] GetRowData(int index)
        {
            if (RowIndexIsValid(index))
                return ((DataGridItem)_rows[index]).Data;
            else
                return null;
        }

        /// <summary>
        /// Sets a cell's data.
        /// </summary>
        /// <param name="columnIndex">columnIndex</param>
        /// <param name="rowIndex">rowIndex</param>
        /// <param name="data">data</param>
        public void SetCellData(int columnIndex, int rowIndex, object data)
        {
            if (ColumnIndexIsValid(columnIndex) && RowIndexIsValid(rowIndex))
            {
                ((DataGridItem)_rows[rowIndex]).Data[columnIndex] = data;
                UpdateItem(rowIndex, true);
            }
        }

        /// <summary>
        /// Get a cell's data.
        /// </summary>
        /// <param name="columnIndex">columnIndex</param>
        /// <param name="rowIndex">rowIndex</param>
        public object GetCellData(int columnIndex, int rowIndex)
        {
            if (ColumnIndexIsValid(columnIndex) && RowIndexIsValid(rowIndex))
                return ((DataGridItem)_rows[rowIndex]).Data[columnIndex];
            else
                return null;
        }

        private void PerformSort()
        {
            int order = (_order == Order.DESC) ? -1 : 1;
            object item;
            int i, j;

            for (i = 0; i < _rows.Count; i++)
            {
                item = _rows[i];
                j = i;

                while ((j > 0) && ((_comparer.Compare(_rows[j - 1], item) * order) > 0))
                {
                    _rows[j] = _rows[j - 1];
                    j--;
                }

                _rows[j] = item;
            }

            _renderHeaders = true;
            _renderItems = true;
        }

        /// <summary>
        /// Sorts the items on a specified column index.
        /// </summary>
        /// <param name="columnIndex"></param>
        public void Sort(int columnIndex)
        {
            _selectedDataGridColumnIndex = columnIndex;

            DataGridColumn dataGridColumn = (DataGridColumn)_columns[columnIndex];
            dataGridColumn.ToggleOrder();

            _order = dataGridColumn.Order;
            _comparer.ColumnIndex = columnIndex;

            PerformSort();

            for (int i = 0; i < _rows.Count; i++)
            {
                if ((DataGridItem)_rows[i] == _selectedItem)
                {
                    RenderItemClear(_selectedIndex);
                    _selectedIndex = i;
                }
            }
        }

        /// <summary>
        /// Clears all items including their data and resets the data grid.
        /// </summary>
        public void Clear()
        {
            _rows.Clear();

            _renderHeaders = true;
            _renderItems = true;
            _scrollIndex = 0;
            _pressed = false;
            _moving = false;

            _listY = 0;
            _listMaxY = 0;
            _ignoredTouchMoves = 0;
            _maxIgnoredTouchMoves = 1;

            SelectedIndex = -1;

            _selectedDataGridColumnIndex = -1;
        }

        /// <summary>
        /// Number of rows displayed.
        /// </summary>
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                if (value != _rowCount)
                {
                    _rowCount = value;
                    Height = _rowCount * RowHeight;
                }
            }
        }

        /// <summary>
        /// Font used by the text.
        /// </summary>
        public Font Font = Resources.GetFont(Resources.FontResources.droid_reg12);

        /// <summary>
        /// Row height.
        /// </summary>
        public int RowHeight = 20;

        /// <summary>
        /// The currently selected index.
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value != _selectedIndex && (value >= -1 && value < _rows.Count))
                {
                    // If a previous selection exists -- clear it
                    if (_selectedIndex > -1 && _selectedIndex < _rows.Count)
                    {
                        int oldSelectedIndex = _selectedIndex;
                        _selectedIndex = -1;
                        UpdateItem(oldSelectedIndex, true);
                    }

                    _selectedIndex = value;

                    if (_selectedIndex > -1)
                    {
                        _selectedItem = (DataGridItem)_rows[_selectedIndex];
                        RenderItem(_selectedIndex, ((DataGridItem)_rows[_selectedIndex]).Data, SelectedItemBackColor, SelectedItemFontColor);
                    }
                    else
                        _selectedItem = null;

                    // Scroll as selected index gets out of view.
                    int rowCount = RowCount - 1;
                    if (_selectedIndex <= _scrollIndex)
                        ScrollTo(_selectedIndex);
                    else if (_selectedIndex > _scrollIndex + rowCount)
                        ScrollTo(_selectedIndex - rowCount);

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Number of items in the DataGrid.
        /// </summary>
        public int NumItems
        {
            get { return _rows.Count; }
        }

        /// <summary>
        /// Indicates whether items trigger cell tap events or not.
        /// </summary>
        public bool TappableCells = true;

        /// <summary>
        /// Indicates whether or not the item list can be dragged up and down.
        /// </summary>
        public bool Draggable = true;

        /// <summary>
        /// Indicates whether the headers are shown.
        /// </summary>
        public bool ShowHeaders
        {
            get { return _showHeaders; }
            set
            {
                if (value != _showHeaders)
                {
                    _showHeaders = value;
                    if (_showHeaders)
                        _rowCount--;
                    else
                        _rowCount++;

                    _renderHeaders = true;
                    _renderItems = true;
                }
            }
        }

        /// <summary>
        /// Indicates whether the headers are sortable.
        /// </summary>
        public bool SortableHeaders = true;

        /// <summary>
        /// Headers background color.
        /// </summary>
        public Color HeadersBackColor = Colors.Black;

        /// <summary>
        /// Headers font color.
        /// </summary>
        public Color HeadersFontColor = Colors.White;

        /// <summary>
        /// Items background color.
        /// </summary>
        public Color ItemsBackColor = Colors.White;

        /// <summary>
        /// Items alternate background color.
        /// </summary>
        public Color ItemsAltBackColor = GlideUtils.Convert.ToColor("F0F0F0");

        /// <summary>
        /// Items font color.
        /// </summary>
        public Color ItemsFontColor = Colors.Black;

        /// <summary>
        /// Selected item background color.
        /// </summary>
        public Color SelectedItemBackColor = GlideUtils.Convert.ToColor("FFF299");

        /// <summary>
        /// Selected item font color.
        /// </summary>
        public Color SelectedItemFontColor = Colors.Black;

        /// <summary>
        /// Grid color.
        /// </summary>
        public Color GridColor = Colors.Gray;

        /// <summary>
        /// Indicates whether the scrollbar is shown.
        /// </summary>
        public bool ShowScrollbar = true;

        /// <summary>
        /// Scrollbar width.
        /// </summary>
        public int ScrollbarWidth = 4;

        /// <summary>
        /// Scrollbar background color.
        /// </summary>
        public Color ScrollbarBackColor = GlideUtils.Convert.ToColor("C0C0C0");

        /// <summary>
        /// Scrollbar scrubber color.
        /// </summary>
        public Color ScrollbarScrubberColor = Colors.Black;

        /// <summary>
        /// The order in which rows are sorted.
        /// </summary>
        /// <remarks>ASC stands for ascending ex: 1 to 10 or A to Z. DESC stands for descending ex: 10 to 1 or Z to A.</remarks>
        public enum Order
        {
            /// <summary>
            /// Ascending
            /// </summary>
            ASC,

            /// <summary>
            /// Descending
            /// </summary>
            DESC
        }
    }
}
