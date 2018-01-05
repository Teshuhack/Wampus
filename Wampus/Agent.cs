using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Wampus
{
    class Agent
    {
        public int x = 1; //Текущее положение
        public int y = 1;
        private int x0 = 0; //Предыдущее положение
        private int y0 = 0;
        private Random R = new Random(); //Датчик случайных чисел
        private int Gun = 2; //Осталось патронов
        enum Orientation {Nord,West,South,East,Indifferent}; 
        Field World;
        public string actions = ""; //Действия агента
        public bool Auto = false; //Режим работы головного мозна агента
        public bool TheEnd = false; //Игра окончена!
        Orientation orientation = Orientation.Nord; //Ориентация агента. Кхм.
        public bool Smart = false;
        public Agent(Field w)
        {
            World = w;
        }

        public void Scout(int x,int y, Field RealWorld)
        //Узнать характеристики места, куда попал
        {
            World[x, y].Assign(RealWorld[x, y]);
            World[x, y].Scouted = true;
        }

        public void Move(Field RealWorld)
        //Перемещение агента
        {
            if (TheEnd) return;
            Scout(x, y, RealWorld); //Обновить ориентацию на местности
            //Если сработал сенсор
            if (Smart)
            if (RealWorld[x, y].SensorHole || RealWorld[x, y].SensorLight || RealWorld[x, y].SensorWampus)
                if (Auto)  actions = "";

            //Пока есть команды, исполнять
            if (actions.Length == 0)
                //Если нет и режим автоматики - включить мозг
                if (Auto) Brain();
            x0 = x;
            y0 = y;
            if (actions.Length != 0)
            {
                switch (actions[0])
                //Всего возможны команды l[eft], r[ight], g[o], s[hoot].
                {
                    case 'r':
                        orientation = ToRight(orientation);
                        break;
                    case 'l':
                        orientation = ToLeft(orientation);
                        break;
                    case 'g':
                        switch (orientation)
                        {
                            case Orientation.Nord: //На север. 
                                y--;  break;
                            case Orientation.South://На юг
                                y++;  break;
                            case Orientation.East: //На восток
                                x++;  break;
                            case Orientation.West: //На запад
                                x--;  break;
                        }
                        break;
                    case 's'://Стрельнуть
                        {
                            if (Gun == 0) break; //Стрелять то нечем :(
                            int targetx = x;
                            int targety = y;
                            if (orientation == Orientation.Nord) targety--;
                            if (orientation == Orientation.South) targety++;
                            if (orientation == Orientation.East) targetx++;
                            if (orientation == Orientation.West) targetx--;

                            Gun--; //Стрельнул
                            if (RealWorld[targetx,targety].Wampus == Square.state.True)
                            { 
                                MessageBox.Show("Вампус пришиблен насмерть. Шкурка доставлена к камину.");
                                Scout(targetx, targety, RealWorld); //Заодно разведать
                                RealWorld[targetx, targety].Wampus = Square.state.False;
                                RealWorld[targetx+1, targety].SensorWampus = false;
                                RealWorld[targetx-1, targety].SensorWampus = false;
                                RealWorld[targetx, targety+1].SensorWampus = false;
                                RealWorld[targetx, targety-1].SensorWampus = false;
                            }
                            else
                                MessageBox.Show("Не попал");
                            if (Gun==0)
                                MessageBox.Show("Патроны кончились");
                        }
                        break;
                }
                if (RealWorld[x, y].Wall == Square.state.True)
                {
                    Scout(x, y, RealWorld);
                    x = x0;
                    y = y0;
                }
                if (actions != "")
                actions = actions.Substring(1);
                Scout(x, y, RealWorld);

                //Обновить RealWorld&World
                RealWorld[x0, y0].Agent = Square.state.False;
                World[x0, y0].Agent = Square.state.False;
                RealWorld[x, y].Agent = Square.state.True;
                World[x, y].Agent = Square.state.True;
                //Если агент попадает в яму или в клетку к вампусу, то игра заканчивается.
                if (RealWorld[x, y].Hole == Square.state.True)
                {
                    TheEnd = true;
                    MessageBox.Show("Яма. Глубокая. Земля, прощай...");
                }
                if (RealWorld[x, y].Wampus == Square.state.True)
                {
                    TheEnd = true;
                    MessageBox.Show("Кому - агент, а кому - обед. (r)Wampus");
                }

                if (RealWorld[x, y].Gold == Square.state.True)
                {
                    TheEnd = true;
                    MessageBox.Show("Золота! Надо больше ЗОЛОТА!");
                    MessageBox.Show("Чтобы получить больше золота, нажмите кнопку \"Заново\"");
                }

            }
        }

        Point[] P = new Point[3];
        Point WampusPoint = new Point(0, 0);

        bool LocateWampus()
        //Иногда можно узнать, где прячется это вонючий зверек
        {
            //Вампус может быть вычислен по запаху, если есть два сработавших сенсора
            int X = World.squares.GetLength(0);
            int Y = World.squares.GetLength(1);
            int Count = 0;
            for (int xx = 0; xx < X; xx++)
                for (int yy = 0; yy < Y; yy++)
                    if (World[xx, yy].SensorWampus)
                    {
                        P[Count] = new Point(xx, yy); 
                        Count++;
                        if (Count >= 3) break; //Больше двух точек не нужно
                    }
            if (Count < 3) return false; //Нет нужного количества замеров
                                         //Есть три засечки. По условиям Вампус может быть только в таком месте, где
                                         //координата x совпадает с двумя точками из трех, то есть равна округленному среднему арифметическому
                                         //координата y аналогично
            //Выбрать x
            WampusPoint.X = (int)((P[0].X + P[1].X + P[2].X) / 3.0 + 0.5);
            //Выбрать y
            WampusPoint.Y = (int)((P[0].Y + P[1].Y + P[2].Y) / 3.0 + 0.5);
            World[WampusPoint.X, WampusPoint.Y].Wampus = Square.state.True;
            return true;
        }

        Point GoldPoint = new Point(0, 0);
        bool LocateGold()
        {
            int X = World.squares.GetLength(0);
            int Y = World.squares.GetLength(1);
            int Count = 0;
            for (int xx = 0; xx < X; xx++)
                for (int yy = 0; yy < Y; yy++)
                    if (World[xx, yy].SensorLight)
                    {
                        P[Count] = new Point(xx, yy);
                        Count++;
                        if (Count >= 3) break; //Больше двух точек не нужно
                    }
            if (Count < 3) return false; //Нет нужного количества замеров

            //Выбрать x
            GoldPoint.X = (int)((P[0].X + P[1].X + P[2].X) / 3.0 + 0.5);
            //Выбрать y
            GoldPoint.Y = (int)((P[0].Y + P[1].Y + P[2].Y) / 3.0 + 0.5);
            World[GoldPoint.X, GoldPoint.Y].Gold = Square.state.True;

            return true;
        }

        void FillHoles()
        //Мир полон опасностей. 
        //Врага надо знать!
        {
            int X = World.squares.GetLength(0);
            int Y = World.squares.GetLength(1);
            //Допустим, нет никаких ям и прочих неприятностей
            for (int xx = 0; xx < X; xx++)
                for (int yy = 0; yy < Y; yy++)
                    World[x, y].MayBeHole = 0;

            //Но на самом-то деле они есть!
            for (int xx = 1; xx < X-1; xx++)
                for (int yy = 1; yy < Y-1; yy++)
                { 
                    if (World[x, y].SensorHole)
                    //Там может быть дыра. А дыра - это нора!
                    {
                        World[x+1, y].MayBeHole++;
                        World[x-1, y].MayBeHole++;
                        World[x, y+1].MayBeHole++;
                        World[x, y-1].MayBeHole++;
                    }
                    if (World[x, y].SensorWampus)
                    {
                        //Иррациональный страх перед Вампусом
                        World[x + 1, y].MayBeHole += 2;
                        World[x - 1, y].MayBeHole += 2;
                        World[x, y + 1].MayBeHole += 2;
                        World[x, y - 1].MayBeHole += 2;
                    }

                    if (World[x, y].Wampus == Square.state.True)
                        World[x, y].MayBeHole = 99; //Вот наверняка там съедят
                }
        }

        List<Point> Path; //Рабочий путь
        List<Point> Best; //Лучший путь

        int Danger(List<Point> Path)
        //Оценить опасность пути
        {
            int result = 0;
            foreach (var p in Path)
                result += World[p.X, p.Y].MayBeHole;
            return result;
        }

        bool Better()
        //Сравнить Path и Best. true, если безопаснее, при равных опасностях короче, или Best не определен
        {
            if (Best.Count == 0) return true;
            //Вычислить опасность
            int db = Danger(Best);
            int dp = Danger(Path);
            if (dp > db) return false; //Нет, хуже
            if (dp < db) return true; //Да, лучше
            return Path.Count < Best.Count; //Зависит от длины
        }

        void CopyPath()
        {
            Best = new List<Point>(Path);
        }

        private void TraceTo(int x, int y, Point P)
        //Рекурсивный алгоритм перемещение из x,y в P
        //Формирует массив из промежуточных точек маршрута
        //по трассе с минимальной опасностью
        {
            //Если выход за пределы мира - то поиск прерывается
            if (x < 0 || x >= World.squares.GetLength(0)) return;
            if (y < 0 || y >= World.squares.GetLength(1)) return;
            if (Path.Count > World.squares.GetLength(0) + World.squares.GetLength(1) + 2) return; //Слишком длинный
            //Если цель достигнута - построение пути закончено
            if (P.X == x && P.Y == y)
            {
                //Если предлагаемый путь хоть чем-то лучше уже найденного
                if (Better()) CopyPath();
                return;
            }
            //Если точка уже проверена - ничего не ходить туда
            if (World[x, y].Mark) return;
            //Добавить в текущий путь новую точку
            World[x, y].Mark = true;
            Path.Add(new Point(x, y));
            //Для всех соседних точек
            //Сделать попытку перемещения (вызвать рекурсивно себя же)
            TraceTo(x + 1, y, P);
            TraceTo(x - 1, y, P);
            TraceTo(x , y+1, P);
            TraceTo(x , y-1, P);
            //Убрать из пути точку
            Path.RemoveAt(Path.Count - 1);
            World[x, y].Mark = false;
        }
        private void TraceTo(Point P, Orientation neworientation)
        //Проложить трассу к заданной точке 
        //С учетом опасных клеток
        //и установить в action первое действие
        {
            //Чтобы попасть из (x,y) в (P.X,P.Y)
            //Нужно перебрать все варианты маршрута и выбрать самый безопасный и короткий
            FillHoles(); //Заполнить подозрительные (опасные) места
            Path = new List<Point>(); //Путь 
            Best = new List<Point>(); //Путь наилучший
            TraceTo(x,y,P);
            //Для первой точки маршрута
            //Если она, конечно, существует
            if (Best.Count == 0) return; //Нет пути
            Best.Add(P); //Добавить конечную точку
            Best.RemoveAt(0); //Удалить исходную точку
            //Проверить необходимость поворота
            actions = OrientationFrom(x, y, Best[0].X, Best[0].Y, orientation);
            //Для первой точки маршрута сказать "go!"
            if (actions == "") actions = "g";
        }

        Orientation ToRight(Orientation current)
        {
            if (current == Orientation.Nord) return Orientation.East; else return --current;
        }

        Orientation ToLeft(Orientation current)
        {
            if (current == Orientation.East) return Orientation.Nord; else return ++current;
        }

        string OrientationFrom(int fromx,int fromy, int tox,int toy, Orientation current)
        //Как правильно развернуться из (fromx,fromy) к (tox,toy)
        {
            Orientation need = Orientation.Indifferent;
            string result="";
            if (fromx == tox && fromy == toy) return result; //Нет ориентации
            if (fromx == tox)//Ориентация или север-юг
            {
                if (toy > fromy) need = Orientation.South;
                if (toy < fromy) need = Orientation.Nord;
            }

            if (fromy == toy) //восток-запад
            {
                if (fromx > tox) need = Orientation.East;
                if (fromx < tox) need = Orientation.West;
            }

            //need - куда надо, current - текущая 
            if (need == current) return result; //Уже ориентирован верно
            if (ToLeft(current) == need)        result = "l"; //налево
            else if (ToRight(current) == need)  result = "r"; //Направо
            else                                result = "rr"; //Поворот назад

            return result;
        }

        Point LocateEmpty()
            //Найти ближайшую неразведанную точку 
        {
            Point Result = new Point();
            int D = 999; //Такой далекой точки не бывает

            for (int xx = 1; xx < World.squares.GetLength(0)-1; xx++)
                for (int yy = 1; yy < World.squares.GetLength(1)-1; yy++)
                { 
                    if (World[xx, yy].AnyBodyHere) continue; //Совсе не пустая
                    //Разведана?
                    if (World[xx, yy].Scouted) continue;
                    if (Math.Abs(x-xx)+Math.Abs(y-yy) < D)
                    {
                        Result.X = xx;
                        Result.Y = yy;
                        D = Math.Abs(x - xx) + Math.Abs(y - yy);
                    }
                }
            return Result;
        }

        private void ScullyBrain()
        //Поведение разумного и практичного агента
        {
            //Разумный агент отличается от обычного способностью оценивать ситуацию,
            //Например, может попытаться определить положение ямы или Вампуса, чтобы туда не ходить
            //и стрелять наверняка
            //И определить положение золотого слитка, чтобы наверняка его прикарманить
            //Если же ничего вычислить не удалось, то агент идет к ближайшей неразведанной клетке
            if (LocateGold())
            {
                //Грабить корован
                //MessageBox.Show("Золото найдено!");
                TraceTo(GoldPoint,Orientation.Indifferent);
                return;
            }

            if (LocateWampus())
            {
                //Подкрасться и застрелить
                TraceTo(GoldPoint, Orientation.Indifferent);
                if (actions=="") //Уже на ближайшей клетке
                    actions = OrientationFrom(x,y,WampusPoint.X, WampusPoint.Y, orientation);
                if (actions == "") //На ближайшей клетке и правильно ориентирован
                    actions = "s"; //Feuer!!
                //MessageBox.Show("Вампус найден!");
                return;
            }

            TraceTo(LocateEmpty(), Orientation.Indifferent);

        }

        private void FoolBrain()
        //Простые правила поведения агента
        {
            //Если рядом Вампус или яма - надо бежать
            if (World[x, y].SensorWampus 
                ||
                World[x, y].SensorHole                
               )
            {
                actions = "rrg"; //Удрать
                if (World[x, y].SensorWampus)
                if (R.Next(4) == 0) actions = "s" + actions; //Может, Вампус спереди
                else if (R.Next(4) == 0) actions = "rs" + actions; //Может, сбоку
                else if (R.Next(4) == 0) actions = "ls" + actions; //Или с другого
                return;
            }
            //Если рядом золото - искать его
            if (World[x, y].SensorLight)
            {
                actions = "grgrggrggrggr";
                return;
            }

            //Если положение не изменилось - надо повернуть
            if (x==x0 && y==y0)
            {
                //Агент бродит в потемках. И поворачивает случайным образом.
                switch (R.Next(3))
                {
                    case 0:
                        actions = "lg";
                        break;
                    case 1:
                        actions = "rg";
                        break;
                    case 2:
                        actions = "g"; //Или не поворачивает
                        break;
                }
                return;
            }
            //Если ничего особого
            actions = "g";
            //С небольшой вероятностью - повернуть
            if (R.Next(10) == 0) actions = "r" + actions;
            if (R.Next(10) == 0) actions = "l" + actions;
        }

        private void Brain()
        //Головной мозг у любого агента должен быть private
        {
            //Агент может быть шибко умным или не очень
            if (Smart) ScullyBrain(); else FoolBrain();

            //Цель агента - найти золото. Больше золота!
            //Для конфискации золота Вампуса достаточно оказаться на одной с ним клетке
            //После этого игра заканчивается
            //Агент не хочет попасть в яму или к Вампусу на обед
            //Можно (1) раз стрельнуть в Вампуса, если есть уверенность в успехе
            //если разрешить беспорядочную стрельбу, то это будет какой-то агент 007 :(
        }

        public void Paint(PaintEventArgs e, Rectangle r)
        {
            //Определить, где рисоваться
            int X = World.squares.GetLength(0);
            int Y = World.squares.GetLength(1);
            int W = r.Width / X;
            int H = r.Height / Y;
            int x0 = r.X + x * W + W / 2;
            int y0 = r.Y + y * H + H / 2;


            //Нарисовать свое направление
            Pen p = new Pen(Color.Red, 1);
            switch (orientation)
            {
                case Orientation.Nord: //На север. 
                    e.Graphics.DrawLine(p, x0, y0, x0, y0 - H / 2);
                    break;
                case Orientation.South:
                    e.Graphics.DrawLine(p, x0, y0, x0, y0 + H / 2);
                    break;
                case Orientation.East:
                    e.Graphics.DrawLine(p, x0, y0, x0 + W / 2, y0);
                    break;
                case Orientation.West:
                    e.Graphics.DrawLine(p, x0, y0, x0 - W / 2, y0);
                    break;
            }
        }


        public override string ToString()
        {
            return x.ToString()+":"+y.ToString()+" "+orientation.ToString();
        }

    }
}
