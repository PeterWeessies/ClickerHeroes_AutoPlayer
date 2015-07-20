using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clickerheroes.autoplayer
{
    /// <summary>
    /// Represents an OCR char
    /// </summary>
    class Char
    {
        /// <summary>
        /// The char value obtained through OCR
        /// </summary>
        public char GuessedCharacter
        {
            get;
            private set;
        }

        /// <summary>
        /// The total area, in pixels, of this char
        /// </summary>
        int TotalArea;

        /// <summary>
        /// The number of alternating white-black groups down the vertical middle
        /// </summary>
        int MaxVerticalGroups;

        /// <summary>
        /// The area of the char in its upper half
        /// </summary>
        int UpperArea;

        /// <summary>
        /// The area of the char in its lower half
        /// </summary>
        int LowerArea;

        /// <summary>
        /// The area of the char in its left half
        /// </summary>
        int LeftArea;

        /// <summary>
        /// The area of the char in its right half
        /// </summary>
        int RightArea;

        /// <summary>
        /// The rectangle which contains this char
        /// </summary>
        Rectangle BoundingRect;

        public Rectangle GetBoundingRectangle()
        {
            return BoundingRect;
        }

        /// <summary>
        /// Constructor class. Note that the char is not actually OCR'ed until DoOCR is called.
        /// </summary>
        /// <param name="boundingRect"></param>
        public Char(Rectangle boundingRect)
        {
            BoundingRect = boundingRect;
        }

        /// <summary>
        /// Performs OCR on this char
        /// </summary>
        /// <param name="b">The bitmap containing this char</param>
        /// <param name="screentotalArea">The total area of the entire playscreen, used for OCR purposes</param>
        /// <param name="specialcharsenabled">True if "e" and "." can appear in the string </param>
        public void DoOcr(LockBitmap b, int screentotalArea, bool specialcharsenabled = true) {
            int totalArea = 0, lArea = 0, tArea = 0;
            int leftArea = 0, rightArea = 0;
            int groupsDownMiddleVertical = 0;
            int midwidth = 0;
            for (int i = BoundingRect.Left; i <= BoundingRect.Right; i++)
            {
                for (int j = BoundingRect.Top; j <= BoundingRect.Bottom; j++)
                {
                    if (b.GetPixel(i, j) == Color.FromArgb(255, 0, 0, 0))
                    {
                        totalArea++;
                        if (j < (BoundingRect.Bottom - BoundingRect.Top) / 2 + BoundingRect.Top)
                        {
                            tArea++;
                        }
                        else
                        {
                            if (j == (BoundingRect.Bottom - BoundingRect.Top) / 2 + BoundingRect.Top)
                            {
                                midwidth++;
                            }
                            lArea++;
                        }

                        if (i < (BoundingRect.Right - BoundingRect.Left) / 2 + BoundingRect.Left)
                        {
                            leftArea++;
                        }
                        else
                        {
                            rightArea++;
                        }

                    }

                    
                }
            }

            bool isBlack = false;
            bool storedWhiteSubgroup = false;
            for (int j = BoundingRect.Top; j <= BoundingRect.Bottom; j++)
            {
                if (b.GetPixel((BoundingRect.Right - BoundingRect.Left) / 2 + BoundingRect.Left, j) == Color.FromArgb(255, 0, 0, 0))
                {
                    isBlack = true;
                    if (storedWhiteSubgroup)
                    {
                        groupsDownMiddleVertical++;
                        storedWhiteSubgroup = false;
                    }
                }
                else
                {
                    if (isBlack)
                    {
                        storedWhiteSubgroup = true;
                        isBlack = false;
                    }
                }
            }

            char guessedchar = '~';
            switch (groupsDownMiddleVertical)
            {
                case 0:
                    if (totalArea > 0.000157 * screentotalArea || specialcharsenabled == false)
                    {
                        guessedchar = '1';
                    }
                    else
                    {
                        guessedchar = '.';
                    }
                    break;
                case 2:
                    if (totalArea < 0.0003571 * screentotalArea && specialcharsenabled) //(double)rightArea / leftArea < 1.1) //1.1256)
                    {
                        guessedchar = 'e';
                    } else if ((double)rightArea / leftArea > 1.6160) //1.6   1.6177
                    {
                        guessedchar = '3';
                    }
                    else if ((double)midwidth / BoundingRect.Width > 0.75) //1.2597)
                    {
                        guessedchar = '5';
                    }
                    else
                    {
                        guessedchar = '8';
                    }
                    break;
                case 1:
                    if ((double)lArea / tArea > 1.6691) //1.6406 1.7420 1.6933
                    {
                        guessedchar = '6';
                    }
                    else if ((double)lArea / tArea > 1.3405) //1.2514
                    {
                        guessedchar = '4';
                    }
                    else if ((double)lArea / tArea <= 1) //0.9769
                    {
                        if ((double)midwidth / BoundingRect.Width < 0.7045)
                        {
                            guessedchar = '7';
                        }
                        else
                        {
                            guessedchar = '9';
                        }
                    }
                    else
                    {
                        if ((double)rightArea / leftArea > 1.20) // 1.1540)
                        {
                            guessedchar = '2';
                        }
                        else
                        {
                            guessedchar = '0';
                        }
                    }
                    break;

                default:
                    break;
            }

            TotalArea = totalArea;
            MaxVerticalGroups = groupsDownMiddleVertical;
            UpperArea = tArea;
            LowerArea = lArea;
            LeftArea = leftArea;
            RightArea = rightArea;
            GuessedCharacter = guessedchar;
        }
    }

    /// <summary>
    /// Represents a string of Chars on screen in a single horizontal line
    /// </summary>
    class Line
    {
        /// <summary>
        /// All the chars on this line
        /// </summary>
        private IEnumerable<Char> Characters;

        /// <summary>
        /// The line's bounding rectangle
        /// </summary>
        private Rectangle BoundingRectangle;

        /// <summary>
        /// The text representation of this line, obtained through OCR
        /// </summary>
        public string OcrString { get; private set; }


        public Rectangle GetBoundingRectangle()
        {
            return BoundingRectangle;
        }

        /// <summary>
        /// Performs OCR on this line, recursively calling each child Char's DoOCR function
        /// </summary>
        /// <param name="lb">The bitmap containing the line</param>
        /// <param name="screentotalArea">The total area of the playscreen, used for OCR purposes</param>
        /// <param name="specialcharsenabled">True if the line can contain "e" and "."</param>
        /// <param name="lthreshold">The left-threshold. Characters which are contained entirely to the left of this value are skipped</param>
        /// <param name="lskip">Left skip. Specifies a number of characters which will be skipped, when reading from left to right.</param>
        public void DoOcr(LockBitmap lb, int screentotalArea, bool specialcharsenabled = true, int lthreshold = -1, int lskip = -1)
        {
            StringBuilder sb = new StringBuilder(Characters.Count());
            int lskipped = 0;
            foreach (Char c in Characters)
            {
                if (lthreshold != -1 && c.GetBoundingRectangle().Right < lthreshold)
                {
                    continue;
                }

                if (lskip != -1 && lskipped < lskip)
                {
                    lskipped++;
                    continue;
                }

                c.DoOcr(lb, screentotalArea, specialcharsenabled);
                sb.Append(c.GuessedCharacter);
            }
            OcrString = sb.ToString();
        }


        public Line(LockBitmap b, Rectangle boundingRectangle)
        {
            BoundingRectangle = boundingRectangle;
            List<Char> characters = new List<Char>();
            int rectLeft = -1, rectTop = -1, rectBottom = -1, rectRight = -1;
            bool inRect = false;

            Color black = Color.FromArgb(255, 0, 0, 0);
            for (int x = boundingRectangle.Left; x <= boundingRectangle.Right; x++)
            {
                bool blackFound = false;
                for (int y = boundingRectangle.Top; y <= boundingRectangle.Bottom; y++)
                {
                    Color c = b.GetPixel(x, y);
                    if (c == black)
                    {
                        if (rectLeft == -1)
                        {
                            rectLeft = x;
                        }

                        if (y < rectTop || rectTop == -1)
                        {
                            rectTop = y;
                        }

                        if (y > rectBottom || rectBottom == -1)
                        {
                            rectBottom = y;
                        }
                        blackFound = true;
                        inRect = true;
                    }
                }

                if (!blackFound && inRect)
                {
                    rectRight = x - 1;
                    characters.Add(new Char(new Rectangle(rectLeft, rectTop, rectRight - rectLeft, rectBottom - rectTop)));
                    rectLeft = rectTop = rectRight = rectBottom = -1;
                    inRect = false;
                }
            }

            if (inRect)
            {
                rectRight = boundingRectangle.Right;
                characters.Add(new Char(new Rectangle(rectLeft, rectTop, rectRight - rectLeft, rectBottom - rectTop)));
            }

            Characters = characters;
        }
    }

    /// <summary>
    /// Class which defines static methods for performing OCR on a bitmap.
    /// </summary>
    class OCREngine
    {
        /// <summary>
        /// Performs OCR on a bitmap, reading the Chars and Lines present.
        /// </summary>
        /// <param name="lb">The bitmap to OCR</param>
        /// <param name="boundingRectangle">Defines the region of the bitmap to OCR. Defining a larger region than necessary will have perf consequences.</param>
        /// <param name="threshold">An enumeration of colors which are considered text colors. Every pixel which is not one of these colors will be ignored.</param>
        /// <returns>A list of lines. Note that character OCR is not performed on the returned lines -- those lines' DoOCR function needs to be called separately (for perf reasons)</returns>
        public static List<Line> OCRBitmap(LockBitmap lb, Rectangle boundingRectangle, IEnumerable<Color> threshold)
        {
            List<Line> lines = new List<Line>();

            int rectLeft = -1, rectRight = -1, rectTop = -1, rectBot = -1;
            bool inLine = false;

            for (int y = boundingRectangle.Top; y < boundingRectangle.Bottom; y++)
            {
                bool foundBlack = false;

                for (int x = boundingRectangle.Left; x < boundingRectangle.Right; x++)
                {
                    Color pixel = lb.GetPixel(x, y);
                    if (pixel == Color.FromArgb(255, 0, 0, 0))
                    {
                        lb.SetPixel(x, y, Color.White);
                    }

                    foreach (Color c in threshold)
                    {
                        if (c == pixel)
                        {
                            lb.SetPixel(x, y, Color.Black);

                            if (rectTop == -1)
                            {
                                rectTop = y;
                            }

                            if (x < rectLeft || rectLeft == -1)
                            {
                                rectLeft = x;
                            }

                            if (x > rectRight || rectRight == -1)
                            {
                                rectRight = x;
                            }

                            foundBlack = true;
                            inLine = true;
                            break;
                        }
                    }
                }

                if (inLine && !foundBlack)
                {
                    rectBot = y - 1;

                    // Minimum area requirements
                    if ((rectBot - rectTop) > 5)
                    {
                        lines.Add(new Line(lb, new Rectangle(rectLeft, rectTop, rectRight - rectLeft, rectBot - rectTop)));
                    }
                    rectLeft = rectTop = rectRight = rectBot = -1;
                    inLine = false;
                }
            }

            return lines;
        }

        /// <summary>
        /// See above.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="boundingRectangle"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static List<Line> OCRBitmap(Bitmap b, Rectangle boundingRectangle, IEnumerable<Color> threshold)
        {
            // Perf -- lock crud
            using (LockBitmap lb = new LockBitmap(b))
            {
                return OCRBitmap(lb, boundingRectangle, threshold);
            }
        }

        /// <summary>
        /// Calculates the percentage of a given bitmap's region which is populated by certain colors.
        /// </summary>
        /// <param name="lb">The bitmap to analyze</param>
        /// <param name="boundingRect">The region of the bitmap to analyze</param>
        /// <param name="colors">The colors to scan for. Every other color is ignored.</param>
        /// <returns></returns>
        public static double GetBlobDensity(LockBitmap lb, Rectangle boundingRect, IEnumerable<Color> colors)
        {
            int totalMatches = 0;

            try
            {
                for (int x = (boundingRect.Left >= 0 ? boundingRect.Left : 0); x <= boundingRect.Right; x++)
                {
                    for (int y = boundingRect.Top; y <= boundingRect.Bottom && y < lb.Height; y++)
                    {
                        Color c = lb.GetPixel(x, y);
                        foreach (Color match in colors)
                        {
                            if (c == match)
                            {
                                totalMatches++;
                                break;
                            }
                        }
                    }
                }

                return (double)totalMatches / (boundingRect.Height * boundingRect.Width);
            }
            catch (Exception)
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// See above.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="boundingRect"></param>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static double GetBlobDensity(Bitmap b, Rectangle boundingRect, IEnumerable<Color> colors) {
            int totalMatches = 0;

            try
            {
                for (int x = (boundingRect.Left >= 0 ? boundingRect.Left : 0); x <= boundingRect.Right; x++)
                {
                    for (int y = boundingRect.Top; y <= boundingRect.Bottom && y < b.Height; y++)
                    {
                        Color c = b.GetPixel(x, y);
                        foreach (Color match in colors)
                        {
                            if (c == match)
                            {
                                totalMatches++;
                                break;
                            }
                        }
                    }
                }

                return (double)totalMatches / (boundingRect.Height * boundingRect.Width);
            }
            catch (Exception)
            {
                return 0.0f;
            }
        }
    }
}
