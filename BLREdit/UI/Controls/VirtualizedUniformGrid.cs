using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace BLREdit.UI.Controls
{
    public class VirtualizedUniformGrid : VirtualizingPanel, IScrollInfo
    {
        private Size _extent = new(0, 0);
        private Size _viewport = new(0, 0);
        private Point _offset = new(0, 0);
        private bool _canHorizontallyScroll = true;
        private bool _canVerticallyScroll = true;
        private ScrollViewer _owner;
        private int _scrollLength = 25;
        private Size _itemSize = new(0, 0);

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached("Orientation", typeof(Orientation), typeof(VirtualizedUniformGrid),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        private int _rows;
        private int _columns;

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #region VirtualizingPanel Overrides

        protected override Size MeasureOverride(Size constraint)
        {
            UpdateComputedValues();
            UpdateScrollInfo(constraint);
            Size availableSize = constraint;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex);

            firstVisibleItemIndex = Math.Min(Math.Max(firstVisibleItemIndex, 0), itemCount - 1);
            lastVisibleItemIndex = Math.Min(Math.Max(lastVisibleItemIndex, 0), itemCount - 1);

            UIElementCollection children = InternalChildren;

            IItemContainerGenerator generator = ItemContainerGenerator;

            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            int childIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; ++itemIndex, ++childIndex)
                {
                    UIElement? child = generator.GenerateNext(out bool newlyRealized) as UIElement;

                    childIndex = Math.Max(0, childIndex);

                    if (newlyRealized)
                    {
                        if (childIndex >= children.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }

                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        if (children.Count > childIndex && child != children[childIndex])
                        {
                            //Debug.Assert(false, "Wrong child was generated");
                        }
                    }
                }
            }
            foreach (UIElement element in base.InternalChildren)
            {
                element.Measure(availableSize);
            }

            UpdateComputedValues();

            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            IItemContainerGenerator generator = ItemContainerGenerator;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child);
            }
            return arrangeSize;
        }

        #endregion VirtualizingPanel Overrides

        private void ArrangeChild(int index, UIElement child)
        {
            if (_rows == 0 || _columns == 0) return;

            double xPosition, yPosition;

            if (Orientation == Orientation.Horizontal)
            {
                int row = index % _rows;
                int column = index / _rows;
                xPosition = column * _itemSize.Width - _offset.X;
                yPosition = row * _itemSize.Height - _offset.Y;
            }
            else
            {
                int row = index / _columns;
                int column = index % _columns;
                xPosition = column * _itemSize.Width - _offset.X;
                yPosition = row * _itemSize.Height - _offset.Y;
            }

            child.Arrange(new Rect(xPosition, yPosition, _itemSize.Width, _itemSize.Height));
        }

        private void UpdateComputedValues()
        {
            Size childSize = new(0,0);
            if (base.InternalChildren.Count > 0)
            {
                foreach (UIElement child in base.InternalChildren)
                {
                    childSize.Width = Math.Max(childSize.Width, child.DesiredSize.Width);
                    childSize.Height = Math.Max(childSize.Height, child.DesiredSize.Height);
                }
            }

            if (childSize.Width == 0 || childSize.Height == 0)
                return;

            _itemSize = childSize;

            _columns = (int)Math.Floor(_viewport.Width / childSize.Width);
            _rows = (int)Math.Floor(_viewport.Height / childSize.Height);

            _scrollLength = Orientation == Orientation.Horizontal ?
                (int)childSize.Width / 2 :
                (int)childSize.Height / 2;

            UpdateScrollInfo(_viewport);
            return;
        }

        private Size MeasureExtent(int itemsCount)
        {
            Size childSize = _itemSize;

            if (Orientation == Orientation.Horizontal)
            {
                var rowWidth = childSize.Width;

                var sizeWidth = rowWidth * Math.Ceiling(((double)itemsCount / _rows));
                var sizeHeight = childSize.Height * _rows;

                return new(sizeWidth, sizeHeight);
            }
            else
            {
                var rowHeight = childSize.Height;

                var sizeWidth = childSize.Width * _columns;
                var sizeHeight = rowHeight * Math.Ceiling(((double)itemsCount / _columns));

                return new(sizeWidth, sizeHeight);
            }
        }

        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            Size childSize = _itemSize;

            int pageSize = _columns * _rows;

            if (Orientation == Orientation.Horizontal)
            {
                int currentColumn = (int)Math.Floor(_offset.X / childSize.Width);

                firstVisibleItemIndex = currentColumn * _rows;
                lastVisibleItemIndex = firstVisibleItemIndex + pageSize + 2 * _rows - 1;
            }
            else
            {
                int currentRow = (int)Math.Floor(_offset.Y / childSize.Height);

                firstVisibleItemIndex = currentRow * _columns;
                lastVisibleItemIndex = firstVisibleItemIndex + pageSize + 2 * _columns - 1;
            }
        }

        #region IScrollInfo Implementation

        public bool CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        public bool CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        public void LineLeft()
        {
            SetHorizontalOffset(_offset.X - _scrollLength);
        }

        public void LineRight()
        {
            SetHorizontalOffset(_offset.X + _scrollLength);
        }

        public void LineUp()
        {
            SetVerticalOffset(_offset.Y - _scrollLength);
        }

        public void LineDown()
        {
            SetVerticalOffset(_offset.Y + _scrollLength);
        }

        public Rect MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        public void MouseWheelDown()
        {
            if (Orientation == Orientation.Horizontal)
            {
                SetHorizontalOffset(_offset.X + _scrollLength);
            }
            else
            {
                SetVerticalOffset(_offset.Y + _scrollLength);
            }
        }

        public void MouseWheelUp()
        {
            if (Orientation == Orientation.Horizontal)
            {
                SetHorizontalOffset(_offset.X - _scrollLength);
            }
            else
            {
                SetVerticalOffset(_offset.Y - _scrollLength);
            }
        }

        public void MouseWheelLeft()
        {
            return;
        }

        public void MouseWheelRight()
        {
            return;
        }

        public void PageDown()
        {
            SetVerticalOffset(_offset.Y + _viewport.Width);
        }

        public void PageUp()
        {
            SetVerticalOffset(_offset.Y - _viewport.Width);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(_offset.X - _viewport.Width);
        }

        public void PageRight()
        {
            SetHorizontalOffset(_offset.X + _viewport.Width);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset > _extent.Width - _viewport.Width)
            {
                offset = _extent.Width - _viewport.Width;
            }
            offset = Math.Max(0, offset);

            if (_offset.X == offset)
                return;

            _offset.X = offset;
            
            _owner?.InvalidateScrollInfo();

            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset > _extent.Height - _viewport.Height)
            {
                offset = _extent.Height - _viewport.Height;
            }
            offset = Math.Max(0, offset);

            if (_offset.Y == offset)
                return;

            _offset.Y = offset;

            _owner?.InvalidateScrollInfo();
            

            InvalidateMeasure();
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            Size extent = MeasureExtent(itemCount);
            if (extent != _extent)
            {
                _extent = extent;
                _offset = new Point();
                _owner?.InvalidateScrollInfo();
            }

            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                _owner?.InvalidateScrollInfo();
            }

        }

        #endregion IScrollInfo Implementation

    }
}
