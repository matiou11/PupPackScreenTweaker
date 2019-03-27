using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CustomPos
{
    /// <summary>
    /// A cool window you can drag and resize in all directions
    ///  - can be transparent
    ///  - moves can be limited to a zone (SetLimits)
    ///  - colors can be changed
    ///  - aspect ratio can be set and locked
    ///  - a background image can be loaded (handling transparency)
    ///  - can be selected/deselected (moves only if selected)
    ///  - can also be moved with the keyboard (arrow keys)
    /// </summary>
    public partial class MatDWindow : Form
    {
        private float aspectRatio;
        private bool lockedAspectRatio;
        private bool isTopMost;
        private bool isTransparent;

        public bool IsSelected { get; set; }

        private bool respectLimits;
        private int? minX;
        private int? maxX;
        private int? minY;
        private int? maxY;

        private string windowCaption;

        private bool areMovesConstrained()
        {
            return respectLimits && minX != null && minY != null && maxX != null && minX != null;
        }

        private const int dragMargin = 20;
          
        private System.Drawing.Color mainColor = Color.Wheat;
        private System.Drawing.Color borderColor = Color.Red;

        private enum ResizingMode { HeightTop, HeightBottom, WidthLeft, WidthRight, CornerLeftTop, CornerLeftBottom, CornerRightTop, CornerRightBottom, JustDragging }
        private static readonly IList<ResizingMode> ResizingModeImpactingWidth = new ReadOnlyCollection<ResizingMode>
            (new List<ResizingMode> { ResizingMode.CornerLeftBottom, ResizingMode.CornerLeftTop, ResizingMode.CornerRightBottom, ResizingMode.CornerRightTop, ResizingMode.WidthLeft, ResizingMode.WidthRight });

        private ResizingMode resizingMode = ResizingMode.JustDragging;
        private bool resizingOrDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private int originalWidth, originalHeight;

        public enum PictureSource { none, picture, db2s, video };
        private Bitmap bgPic;
        public PictureSource BgPictureSource { get; set; }
        public string DefaultPicName { get; set; }
        public string DefaultResName { get; set; }

        public bool HasPicture()
        {
            return !(bgPic == null);
        }

        /// <summary>
        /// raised when position/size of a window is changed
        /// </summary>
        public event EventHandler PropertiesChanged;
        /// <summary>
        /// raised when an unselected window is clicked
        /// </summary>
        public event EventHandler UnauthorizedActivation;

        protected virtual void OnPropertiesChanged(EventArgs e)
        {
            EventHandler handler = PropertiesChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUnauthorizedActivation(EventArgs e)
        {
            EventHandler handler = UnauthorizedActivation;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public MatDWindow()
        {
            InitializeComponent();
            isTopMost = true;
            respectLimits = false;
            bgPic = null;
            DefaultPicName = "";
            DefaultResName = "";
            SetColors(false, false, null, null);
        }

        public MatDWindow(bool alwaysTopMost, bool transparent, bool respectLimitsWhenMoving, Color? mainFormColor, Color? borderFormColor)
        {
            InitializeComponent();
            isTopMost = alwaysTopMost;
            respectLimits = respectLimitsWhenMoving;
            SetColors(false, transparent, mainFormColor, borderFormColor);
        }

        private void MatDWindow_Load(object sender, EventArgs e)
        {
            TopMost = isTopMost;
        }

        /// <summary>
        /// set the limits respected when moving the window (if rescectLimits is true)
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        public void SetLimits(int minX, int maxX, int minY, int maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        public void SetColors(bool random)
        {
            SetColors(random, null, null, null);
        }

        public void SetColors(bool random, bool? transparent, Color? mainFormColor, Color? borderFormColor)
        {
            if (random)
            {
                Random rnd = new Random();
                mainColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                borderColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
            else
            {
                mainColor = mainFormColor ?? Color.Wheat;
                borderColor = borderFormColor ?? Color.Red; // Color.Red;
            }
            if (transparent != null) isTransparent = (bool)transparent;
            if (isTransparent) TransparencyKey = mainColor; else TransparencyKey = Color.Empty;
            ForceRepaint();
        }

        public void SetCaption(string txt)
        {
            windowCaption = txt;
            lblCaption.BackColor = Color.White;
            lblCaption.ForeColor = Color.Black;
            lblCaption.Text = windowCaption;
            lblCaption.Left = dragMargin + 5;
        }

        public void SetAspectRatio(float value, bool locked)
        {
            aspectRatio = value;
            lockedAspectRatio = locked;
            if (lockedAspectRatio)
            {
                ChangeHeight(calculateHeightForAspectRatio(Width));
                updateAllFields();
            }
        }

        public bool IsAspectRatioLocked()
        {
            return lockedAspectRatio;
        }

        private int calculateHeightForAspectRatio(int width)
        {
            return Convert.ToInt32(width / aspectRatio);
        }

        private int calculateWidthForAspectRatio(int height)
        {
            return Convert.ToInt32(height * aspectRatio);
        }

        public bool IsAspectRatioOK()
        {
            return Math.Round(Convert.ToSingle(Width) / Convert.ToSingle(Height), 2) == Math.Round(aspectRatio, 2);
        }

        private void MatDWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsSelected && e.Button == MouseButtons.Left)
            {
                // initialize a new dragging or resizing operation
                dragCursorPoint = Cursor.Position;
                dragFormPoint = Location;
                originalWidth = Width;
                originalHeight = Height;
                resizingOrDragging = true;
            }
            else
            {
                OnUnauthorizedActivation(null);
            }
        }

        private void MatDWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSelected)
            {
                if (!resizingOrDragging)
                {
                    // Change mouse pointer and resizing mode, depending on position of cursor
                    if (Cursor.Position.X > (Left + Width - dragMargin) && Cursor.Position.Y > (Top + Height - dragMargin))
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        resizingMode = ResizingMode.CornerRightBottom;
                    }
                    else if (Cursor.Position.X < (Left + dragMargin) && Cursor.Position.Y > (Top + Height - dragMargin))
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        resizingMode = ResizingMode.CornerLeftBottom;
                    }
                    else if (Cursor.Position.X > (Left + Width - dragMargin) && Cursor.Position.Y < (Top + dragMargin))
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        resizingMode = ResizingMode.CornerRightTop;
                    }
                    else if (Cursor.Position.X < (Left + dragMargin) && Cursor.Position.Y < (Top + dragMargin))
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        resizingMode = ResizingMode.CornerLeftTop;
                    }
                    else
                    {
                        int middleY1 = Top + Height / 2 - dragMargin / 2;
                        int middleY2 = Top + Height / 2 + dragMargin / 2;
                        int middleX1 = Left + Width / 2 - dragMargin / 2;
                        int middleX2 = Left + Width / 2 + dragMargin / 2;

                        if (Cursor.Position.X > (Left + Width - dragMargin) && Cursor.Position.Y > middleY1 && Cursor.Position.Y < middleY2)
                        {
                            Cursor.Current = Cursors.SizeWE;
                            resizingMode = ResizingMode.WidthRight;
                        }
                        else if (Cursor.Position.X < Left + dragMargin && Cursor.Position.Y > middleY1 && Cursor.Position.Y < middleY2)
                        {
                            Cursor.Current = Cursors.SizeWE;
                            resizingMode = ResizingMode.WidthLeft;
                        }
                        else if (Cursor.Position.Y > (Top + Height - dragMargin) && Cursor.Position.X > middleX1 && Cursor.Position.X < middleX2)
                        {
                            Cursor.Current = Cursors.SizeNS;
                            resizingMode = ResizingMode.HeightBottom;
                        }
                        else if (Cursor.Position.Y < (Top + dragMargin) && Cursor.Position.X > middleX1 && Cursor.Position.X < middleX2)
                        {
                            Cursor.Current = Cursors.SizeNS;
                            resizingMode = ResizingMode.HeightTop;
                        }
                        else
                        {
                            Cursor.Current = Cursors.Arrow;
                            resizingMode = ResizingMode.JustDragging;
                        }

                    }
                }
                else // resize or move according to mouse position
                {
                    int prevWidth = Width;
                    int prevHeight = Height;
                    int prevTop = Top;
                    int prevLeft = Left;
                    int newWidth = prevWidth;
                    int newHeight = prevHeight;
                    bool changeTop = false;
                    bool changeLeft = false;

                    Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));

                    // move
                    if (resizingMode == ResizingMode.JustDragging)
                    {
                        Point newLocation = Point.Add(dragFormPoint, new Size(dif));
                        if (areMovesConstrained())
                        {
                            if (newLocation.X < minX) newLocation.X = (int)minX;
                            if (newLocation.X + Width > maxX) newLocation.X = (int)maxX - Width;
                            if (newLocation.Y < minY) newLocation.Y = (int)minY;
                            if (newLocation.Y + Height > maxY) newLocation.Y = (int)maxY - Height;
                        }
                        Location = newLocation;
                        updateAllFields();

                    }
                    // resize
                    else
                    {
                        if (resizingMode == ResizingMode.CornerRightBottom)
                        {
                            newWidth = originalWidth + dif.X;
                            newHeight = originalHeight + dif.Y;
                        }
                        else if (resizingMode == ResizingMode.CornerLeftBottom)
                        {
                            newWidth = originalWidth - dif.X;
                            newHeight = originalHeight + dif.Y;
                            changeLeft = true;
                        }
                        else if (resizingMode == ResizingMode.CornerLeftTop)
                        {
                            newWidth = originalWidth - dif.X;
                            newHeight = originalHeight - dif.Y;
                            changeLeft = true;
                            changeTop = true;
                        }
                        else if (resizingMode == ResizingMode.CornerRightTop)
                        {
                            newWidth = originalWidth + dif.X;
                            newHeight = originalHeight - dif.Y;
                            changeTop = true;
                        }
                        else if (resizingMode == ResizingMode.HeightBottom)
                        {
                            newHeight = originalHeight + dif.Y;
                        }
                        else if (resizingMode == ResizingMode.HeightTop)
                        {
                            newHeight = originalHeight - dif.Y;
                            changeTop = true;
                        }
                        else if (resizingMode == ResizingMode.WidthRight)
                        {
                            newWidth = originalWidth + dif.X;
                        }
                        else if (resizingMode == ResizingMode.WidthLeft)
                        {
                            newWidth = originalWidth - dif.X;
                            changeLeft = true;
                        }
                        // constrain size to a minimum
                        newWidth = Math.Max(dragMargin * 3, newWidth);
                        newHeight = Math.Max(dragMargin * 3, newHeight);

                        // aspect ratio locked ?
                        if (lockedAspectRatio)
                        {
                            if (ResizingModeImpactingWidth.Contains(resizingMode))
                            {
                                newHeight = calculateHeightForAspectRatio(newWidth);
                                newWidth = calculateWidthForAspectRatio(newHeight);
                            }
                            else
                            {
                                newWidth = calculateWidthForAspectRatio(newHeight);
                            }
                        }

                        int newTop = changeTop ? prevTop - (newHeight - prevHeight) : prevTop;
                        int newLeft = changeLeft ? prevLeft - (newWidth - prevWidth) : prevLeft;

                        // check limits;
                        if (areMovesConstrained())
                        {
                            bool needToAdjustWidth = false;
                            bool needToAdjustHeight = false;
                            bool noOtherConstrainCheck = false;

                            if (changeTop && newTop < (int)minY)
                            {
                                newTop = (int)minY;
                                newHeight = prevHeight + Math.Abs(newTop - prevTop);
                                if (lockedAspectRatio) { needToAdjustWidth = true; noOtherConstrainCheck = true; }
                            }
                            if (!noOtherConstrainCheck && changeLeft && newLeft < (int)minX)
                            {
                                newLeft = (int)minX; newWidth = prevWidth;
                                newWidth = prevWidth + Math.Abs(newLeft - prevLeft);
                                if (lockedAspectRatio) { needToAdjustHeight = true; noOtherConstrainCheck = true; }
                                }
                            if (!noOtherConstrainCheck && !changeTop && newTop + newHeight > (int)maxY)
                            {
                                newHeight = (int)maxY - newTop;
                                if (lockedAspectRatio) { needToAdjustWidth = true; noOtherConstrainCheck = true; }
                            }
                            if (!noOtherConstrainCheck && !changeLeft && newLeft + newWidth > (int)maxX)
                            {
                                newWidth = (int)maxX - newLeft;
                                if (lockedAspectRatio) { needToAdjustHeight = true; noOtherConstrainCheck = true; }
                            }

                            if (needToAdjustHeight)
                            {
                                newHeight = calculateHeightForAspectRatio(newWidth);
                                if (changeTop) newTop = prevTop - (newHeight - prevHeight);
                            }
                            if (needToAdjustWidth)
                            {
                                newWidth = calculateWidthForAspectRatio(newHeight);
                                if (changeLeft) newLeft = prevLeft - (newWidth - prevWidth);
                            }
                        }

                        // redraw
                        if (newWidth != prevWidth || newHeight != prevHeight)
                        {
                            {
                                Width = newWidth;
                                Height = newHeight;
                                Top = newTop;
                                Left = newLeft;
                                updateAllFields();
                            }
                        }
                    }
                }
            }
        }

        private void MatDWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsSelected)
            {
                // end of resizing or dragging operation
                resizingOrDragging = false;
                updateAllFields();
            }
        }

        private void MatDWindow_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.SolidBrush brushBorder = new System.Drawing.SolidBrush(borderColor);
            System.Drawing.SolidBrush brushBackground = new System.Drawing.SolidBrush(mainColor);
            System.Drawing.SolidBrush brushDragBoxes = new System.Drawing.SolidBrush(Color.Orange);
            System.Drawing.Pen myPenRed = new System.Drawing.Pen(Color.Red,2);
            System.Drawing.Graphics formGraphics;
            formGraphics = CreateGraphics();
            formGraphics.FillRectangle(brushBackground, new Rectangle(0, 0, Width, Height));
            if (bgPic != null)
            {
                formGraphics.DrawImage(bgPic, new Rectangle(0, 0, Width, Height), 0, 0, bgPic.Width, bgPic.Height, GraphicsUnit.Pixel, null);
            }
            // red border
            Rectangle[] rects1 =
            {
                new Rectangle(0,0,Width,dragMargin),
                new Rectangle(0,0,dragMargin,Height),
                new Rectangle(Width - dragMargin,0,dragMargin,Height),
                new Rectangle(0,Height - dragMargin,Width,dragMargin)
            };
            formGraphics.FillRectangles(brushBorder, rects1);
            // drag boxes
            Rectangle[] rects2 =
            {
                new Rectangle(0, 0, dragMargin, dragMargin),
                new Rectangle(Width /2 - dragMargin / 2, 0, dragMargin, dragMargin),
                new Rectangle(Width - dragMargin, 0, dragMargin, dragMargin),
                new Rectangle(0, Height / 2 - dragMargin / 2, dragMargin, dragMargin),
                new Rectangle(0, Height - dragMargin, dragMargin, dragMargin),
                new Rectangle(Width /2 - dragMargin / 2, Height - dragMargin, dragMargin, dragMargin),
                new Rectangle(Width - dragMargin, Height / 2 - dragMargin / 2, dragMargin, dragMargin),
                new Rectangle(Width - dragMargin, Height - dragMargin, dragMargin, dragMargin),
                new Rectangle(0, 0, dragMargin, dragMargin)
            };
            formGraphics.FillRectangles(brushDragBoxes, rects2);

            brushBorder.Dispose();
            brushBackground.Dispose();
            brushDragBoxes.Dispose();
            myPenRed.Dispose();
            formGraphics.Dispose();
        }


        private void updateAllFields()
        {
            OnPropertiesChanged(null); // tell the main form that things have changed
            ForceRepaint(); // redraw the box
        }

        public void ForceRepaint()
        {
            MatDWindow_Paint(null, null); // redraw the box
        }

        public void ChangePosByIncrement(int difX, int difY)
        {
            int newLeft = Left + difX;
            int newTop = Top + difY;
            if (areMovesConstrained())
            {
                if (newLeft + Width > maxX) newLeft = (int)maxX - Width;
                newLeft = Math.Max((int)minX, newLeft);
                if (newTop + Height > maxY) newTop = (int)maxY - Height;
                newTop = Math.Max((int)minY, newTop);
            }
            Left = newLeft;
            Top = newTop;
            updateAllFields();
        }

        public void CenterX()
        {
            if (maxX != null && minX != null) Left = ((int)maxX - (int)minX - Width) / 2 + (int)minX;
            updateAllFields();
        }

        public void CenterY()
        {
            if (maxY != null && minY != null) Top = ((int)maxY - (int)minY - Height) / 2 + (int)minY;
            updateAllFields();
        }

        public void ChangeWidth(int targetW)
        {
            int newHeight = Height;
            int newWidth = Width;
            if (lockedAspectRatio)
            {
                newHeight = calculateHeightForAspectRatio(targetW);
                newWidth = calculateWidthForAspectRatio(newHeight);
            }
            else
            {
                newWidth = targetW;
            }
            if (areMovesConstrained())
            {
                if (Left + newWidth > maxX)
                {
                    newWidth = (int)maxX - Left;
                    if (lockedAspectRatio) newHeight = calculateHeightForAspectRatio(newWidth);
                }
                if (Top + newHeight > maxY)
                {
                    newHeight = (int)maxY - Top;
                    if (lockedAspectRatio) newWidth = calculateWidthForAspectRatio(newHeight);
                }
            }
            Width = newWidth;
            Height = newHeight;
            updateAllFields();
        }

        public void ChangeHeight(int targetH)
        {
            int newHeight = Height;
            int newWidth = Width;
            if (lockedAspectRatio)
            {
                newWidth = calculateWidthForAspectRatio(targetH);
                newHeight = calculateHeightForAspectRatio(newWidth);
            }
            else
            {
                newHeight = targetH;
            }
            if (areMovesConstrained())
            {
                if (Top + newHeight > maxY)
                {
                    newHeight = (int)maxY - Top;
                    if (lockedAspectRatio) newWidth = calculateWidthForAspectRatio(newHeight);
                }
                if (Left + newWidth > maxX)
                {
                    newWidth = (int)maxX - Left;
                    if (lockedAspectRatio) newHeight = calculateHeightForAspectRatio(newWidth);
                }
            }
            Width = newWidth;
            Height = newHeight;
            updateAllFields();
        }


        private void MatDWindow_KeyDown(object sender, KeyEventArgs e)
        {
            bool moved = false;
            switch (e.KeyCode)
            {
                case Keys.Left: ChangePosByIncrement(-1, 0); moved = true; break;
                case Keys.Right: ChangePosByIncrement(1, 0); moved = true; break;
                case Keys.Up: ChangePosByIncrement(0, -1); moved = true; break;
                case Keys.Down: ChangePosByIncrement(0, 1); moved = true; break;
            }

            if (moved) updateAllFields();
        }

        public void LoadPicture(Bitmap picture, PictureSource source, string extraInfoCaption, string defaultPicFileName, string defaultResName)
        {
            bgPic = picture;
            BgPictureSource = source;
            DefaultPicName = defaultPicFileName;
            DefaultResName = defaultResName;
            lblCaption.Text = windowCaption + " " + extraInfoCaption;
            ForceRepaint();
        }

        public void SavePicture(string fileName)
        {
            bgPic.Save(fileName, ImageFormat.Png);
        }


        public void ClearPicture()
        {
            bgPic = null;
            BgPictureSource = PictureSource.none;
            ForceRepaint();
        }

        public void ChangeSizeByIncrement(int difWidth, int difHeight)
        {
            if (difWidth != 0 || difHeight != 0)
            {
                if (difWidth != 0)
                {
                    if (lockedAspectRatio)
                    {
                        ChangeWidth(Width + difWidth * Convert.ToInt32(aspectRatio));
                        ChangeHeight(calculateHeightForAspectRatio(Width));
                    }
                    else
                    {
                        ChangeWidth(Width + difWidth);
                    }
                }
                if (difHeight !=0)
                {
                    ChangeHeight(Height + difHeight);
                    if (lockedAspectRatio)
                    {
                        ChangeWidth(calculateWidthForAspectRatio(Height));
                    }
                }
                updateAllFields();
            }
        }

        public bool HasBackgroundPic()
        {
            return bgPic != null;
        }

        public int BackgroundPicWidth()
        {
            if (HasBackgroundPic()) return bgPic.Width; else return -1;
        }

        private void lblCaption_MouseDown(object sender, MouseEventArgs e)
        {
            MatDWindow_MouseDown(sender, e);
        }

        private void lblCaption_MouseMove(object sender, MouseEventArgs e)
        {
            MatDWindow_MouseMove(sender, e);
        }

        private void lblCaption_MouseUp(object sender, MouseEventArgs e)
        {
            MatDWindow_MouseUp(sender, e);
        }

        public int BackgroundPicHeight()
        {
            if (HasBackgroundPic()) return bgPic.Height; else return -1;
        }

    }
}
