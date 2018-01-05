using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

//Игровое поле
namespace Wampus
{
    class Field
    {
        public Square[,] squares;
        public Field(int X,int Y)
        {
            squares = new Square[X, Y];
            for (int x = 0; x < X; x++)
                for (int y = 0; y < Y; y++)
                    squares[x, y] = new Square(x, y);
        }
        public Square this[int x, int y] //Индесатор, для удобства
        {
            get { return squares[x, y]; }
        }

        public void Paint(PaintEventArgs e, Rectangle r)
        {
            int X = squares.GetLength(0);
            int Y = squares.GetLength(1);
            int W = r.Width / X;
            int H = r.Height / Y;
            Rectangle rs = new Rectangle(0, 0, W, H);
            for (int x = 0; x< X; x++)
                for (int y=0; y< Y; y++)
                {
                    rs.X = W * x + r.Left;
                    rs.Y = H * y + r.Top;
                    squares[x, y].Paint(e, rs);
                }
        }
    }
}
