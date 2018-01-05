using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wampus
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int X = 5+2; //+2 - для стенок
        int Y = 4+2;
        Field RealWorld; //Реальный мир
        Field AgentWorld; //Мир в представлении агента
        Random R = new Random();
        Agent Mulder = null;

        void Restart()
        {
            buttonRestart.Enabled = false;
            int x, y;
            RealWorld = new Field(X, Y);
            AgentWorld = new Field(X, Y);
            //Очистить реальный мир от чудовищ
            for (x = 0; x < X; x++)
                for (y = 0; y < Y; y++)
                    RealWorld[x, y].Clear();
            //Расставить стены
            for (x = 0; x < X; x++)
            {
                RealWorld[x, 0].Wall = Square.state.True;
                RealWorld[x, Y - 1].Wall = Square.state.True;
            }
            for (y = 0; y < Y; y++)
            {
                RealWorld[0, y].Wall = Square.state.True;
                RealWorld[X - 1, y].Wall = Square.state.True;
            }
            //Разместить Агента
            RealWorld[1, Y-2].Agent = Square.state.True; //Он всегда там
            //Разместить Вампуса
            do
            {
                x = R.Next(X - 2) + 1;
                y = R.Next(Y - 2) + 1;
            }
            while (RealWorld[x, y].AnyBodyHere);
            RealWorld[x, y].Wampus = Square.state.True;

            RealWorld[x-1, y].SensorWampus = true;
            RealWorld[x+1, y].SensorWampus = true;
            RealWorld[x, y-1].SensorWampus = true;
            RealWorld[x, y+1].SensorWampus = true;


            //Разместить Золото
            do
            {
                x = R.Next(X - 2) + 1;
                y = R.Next(Y - 2) + 1;
            }
            while (RealWorld[x, y].AnyBodyHere);
            RealWorld[x, y].Gold = Square.state.True;

            RealWorld[x-1 , y].SensorLight = true;
            RealWorld[x+1, y].SensorLight = true;
            RealWorld[x, y-1].SensorLight = true;
            RealWorld[x, y+1].SensorLight = true;


            //Разместить Ямы
            for (int k = 0; k < 2; k++) //Всего нор - две
            {
                do
                {
                    x = R.Next(X - 2) + 1;
                    y = R.Next(Y - 2) + 1;
                }
                while (RealWorld[x, y].AnyBodyHere);
                RealWorld[x, y].Hole = Square.state.True;
                RealWorld[x-1, y].SensorHole = true;
                RealWorld[x+1, y].SensorHole = true;
                RealWorld[x, y-1].SensorHole = true;
                RealWorld[x, y+1].SensorHole = true;
            }

            Mulder = new Agent(AgentWorld);
            Mulder.y = Y - 2;
            Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Restart();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
            //Скрыть-показать реальный мир
        {
            pictureBox1.Visible = ((CheckBox)sender).Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
            //скрыть - показать субъективный мир
        {
            pictureBox2.Visible = ((CheckBox)sender).Checked;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            //Обновить субъективный мир
            AgentWorld.Paint(e, pictureBox2.ClientRectangle);
            Mulder.Paint(e, pictureBox1.ClientRectangle);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //обновить изображение реального мира
            RealWorld.Paint(e, pictureBox1.ClientRectangle);
            Mulder.Paint(e, pictureBox1.ClientRectangle);
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //Включить автомат
            timer1.Enabled = ((CheckBox)sender).Checked;
        }

        private void buttonStep_Click(object sender, EventArgs e)
        {
            //Один шаг автомата
            Mulder.Auto = true;
            Mulder.Move(RealWorld);
            labelPos.Text = Mulder.ToString();
            buttonRestart.Enabled = Mulder.TheEnd;
            Refresh();
        }

        void OneStep(string s)
        {
            //Ручное управление агентом
            Mulder.Auto = false;
            Mulder.actions = s;
            Mulder.Move(RealWorld);
            labelPos.Text = Mulder.ToString();
            buttonRestart.Enabled = Mulder.TheEnd;
            Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            buttonStep_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //вперед
            OneStep("g");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //стреляй
            OneStep("s");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //поверни налево
            OneStep("l");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //поверни направо
            OneStep("r");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //новая игра
            Restart();
        }

        private void checkBoxSmart_CheckedChanged(object sender, EventArgs e)
        {
            Mulder.Smart = ((CheckBox)sender).Checked;
        }
    }
}
