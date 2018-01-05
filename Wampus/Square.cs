//Одна клетка игрового поля
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Wampus
{
    //Описание одной клетки игрового поля
    class Square
    {
        public enum state { False,True, Unknown};
        int x, y; //Свои координаты
        public state Wampus; //ОН ТАМ
        public state Wall; //Это стенка
        public state Hole; //Это ямка
        public state Gold; //Это золото
        public state Agent; //Здесь - агент

        public bool SensorWampus; //Пахнет Вампусом
        public bool SensorHole; //Дует ветер от ямы
        public bool SensorLight; //Светится золото
        public int MayBeHole; //Может быть ямой
        public bool Mark; //Для поиска пути
        public bool Scouted; //Для определения уже разведанных
        public Square(int x,int y)
        {
            this.x = x;
            this.y = y;
            Agent = Wampus = Wall = Hole = Gold = state.Unknown; //Ужасно интересно все то, что неизвестно
            SensorHole = SensorLight = SensorWampus = false; //И никаких признаков
            MayBeHole = 0; //И даже ямой быть не может
            Scouted = false;
        }

        public bool AnyBodyHere
        {
            get { return 
                    (Wampus == state.True)
                    ||
                    (Wall == state.True)
                    ||
                    (Agent == state.True)
                    ||
                    (Hole == state.True)
                    ||
                    (Gold == state.True)
                    ;
            }
        }

        public void Clear()
        {
            Agent = Wampus = Wall = Hole = Gold = state.False;
            SensorHole = SensorLight = SensorWampus = false;
            MayBeHole = 0;
        }
        public void Assign(Square s)
        {
            this.Agent = s.Agent;
            this.Wampus = s.Wampus;
            this.Wall = s.Wall;
            this.Hole = s.Hole;
            this.Gold = s.Gold;
            this.SensorHole = s.SensorHole;
            this.SensorLight = s.SensorLight;
            this.SensorWampus = s.SensorWampus;
            this.MayBeHole = s.MayBeHole;
        }

        public void Paint(PaintEventArgs e, Rectangle r)
        {
            Pen p = new Pen(Color.Black,1);

            Brush b = Brushes.LightGray; //Непосещенная
            if (Wall != state.Unknown) b = Brushes.DarkGray; //Посещенная
            if (Wampus == state.True) b = Brushes.Brown;
            if (Wall == state.True) b = Brushes.White;
            if (Hole == state.True) b = Brushes.Black;
            if (Gold == state.True) b = Brushes.Yellow;
            if (Agent == state.True) b = Brushes.Green;
            e.Graphics.FillRectangle(b, r);
            e.Graphics.DrawRectangle(p, r);

            if (SensorHole)
            {
                p = new Pen(Color.Black, 5);
                e.Graphics.DrawLine(p, r.Left + 10, r.Top+5, r.Right - 10, r.Top+5);
            };
            if (SensorLight)
            {
                p = new Pen(Color.Yellow, 5);
                e.Graphics.DrawLine(p, r.Left + 10, r.Top+10, r.Right - 10, r.Top+10);
            };
            if (SensorWampus)
            {
                p = new Pen(Color.Brown, 5);
                e.Graphics.DrawLine(p, r.Left+10, r.Top + 15, r.Right-10, r.Top + 15);
            };

        }

    }
}
