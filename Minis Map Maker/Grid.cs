using System.Diagnostics;

namespace Mapdoodle
{
    public class Grid<T>
    {
        readonly int width;
        readonly int height;
        readonly T[] squares;

        public Grid()
        {
        }

        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
            squares = new T[width * height];
        }

        public T this[int x, int y]
        {
            [DebuggerStepThrough]
            get
            {
                Debug.Assert(x >= 0 && x < width);
                Debug.Assert(y >= 0 && y < height);
                return squares[x + width * y];
            }
            [DebuggerStepThrough]
            set
            {
                Debug.Assert(x >= 0 && x < width);
                Debug.Assert(y >= 0 && y < height);
                squares[x + width * y] = value;
            }
        }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
    }
}