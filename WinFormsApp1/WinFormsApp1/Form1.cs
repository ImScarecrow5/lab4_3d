using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // Список кривых Безье. Каждая кривая хранит 4 точки: Start, Control1, Control2, Finish
        private List<BezierCurve> curves = new List<BezierCurve>();
        private Point? selectedPoint = null;
        private BezierCurve selectedCurve = null;
        private int selectedPointIndex = -1; // 0-start,1-control1,2-control2,3-finish

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.ClientSize = new Size(900, 900);
            this.Text = "Кривые Безье - Лабораторная работа";

            // Создаём несколько кривых Безье для примера (например, имитируем спортивный мяч)
            // Первая кривая - верхняя дуга
            curves.Add(new BezierCurve(
                new Point(200, 600),
                new Point(200, 400),
                new Point(400, 200),
                new Point(600, 200)
            ));
            // Вторая кривая - правая дуга
            curves.Add(new BezierCurve(
                new Point(600, 200),
                new Point(700, 200),
                new Point(750, 350),
                new Point(700, 500)
            ));
            // Третья кривая - нижняя дуга
            curves.Add(new BezierCurve(
                new Point(700, 500),
                new Point(650, 700),
                new Point(450, 750),
                new Point(300, 700)
            ));
            // Четвёртая кривая - левая дуга (замыкает)
            curves.Add(new BezierCurve(
                new Point(300, 700),
                new Point(150, 650),
                new Point(150, 500),
                new Point(200, 600)
            ));
            // Дополнительная кривая для центральной части
            curves.Add(new BezierCurve(
                new Point(350, 450),
                new Point(400, 350),
                new Point(500, 350),
                new Point(550, 450)
            ));
            curves.Add(new BezierCurve(
                new Point(550, 450),
                new Point(600, 550),
                new Point(500, 600),
                new Point(400, 550)
            ));
            curves.Add(new BezierCurve(
                new Point(400, 550),
                new Point(350, 500),
                new Point(350, 450),
                new Point(350, 450)
            ));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем все кривые Безье
            foreach (var curve in curves)
            {
                // Основная кривая
                using (Pen pen = new Pen(Color.Blue, 3))
                {
                    g.DrawBezier(pen, curve.Start, curve.Control1, curve.Control2, curve.Finish);
                }

                // Направляющие линии (пунктиром)
                using (Pen dashPen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash })
                {
                    g.DrawLine(dashPen, curve.Start, curve.Control1);
                    g.DrawLine(dashPen, curve.Finish, curve.Control2);
                }

                // Опорные точки (закрашенные кружки)
                DrawPoint(g, curve.Start, Color.Green);
                DrawPoint(g, curve.Control1, Color.Red);
                DrawPoint(g, curve.Control2, Color.Red);
                DrawPoint(g, curve.Finish, Color.Green);
            }
        }

        private void DrawPoint(Graphics g, Point p, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, p.X - 5, p.Y - 5, 10, 10);
            }
            using (Pen pen = new Pen(Color.Black, 1))
            {
                g.DrawEllipse(pen, p.X - 5, p.Y - 5, 10, 10);
            }
        }

        // Поиск ближайшей опорной точки к курсору
        private (BezierCurve curve, int pointIndex, Point point) FindNearestPoint(Point mouseLocation)
        {
            const int threshold = 10;
            BezierCurve nearestCurve = null;
            int nearestIndex = -1;
            Point nearestPoint = Point.Empty;
            double minDist = threshold;

            foreach (var curve in curves)
            {
                CheckPoint(curve.Start, 0);
                CheckPoint(curve.Control1, 1);
                CheckPoint(curve.Control2, 2);
                CheckPoint(curve.Finish, 3);

                void CheckPoint(Point p, int idx)
                {
                    double dx = p.X - mouseLocation.X;
                    double dy = p.Y - mouseLocation.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestCurve = curve;
                        nearestIndex = idx;
                        nearestPoint = p;
                    }
                }
            }
            return (nearestCurve, nearestIndex, nearestPoint);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var (curve, idx, point) = FindNearestPoint(e.Location);
            if (curve != null)
            {
                selectedCurve = curve;
                selectedPointIndex = idx;
                selectedPoint = point;
                this.Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (selectedPoint.HasValue && selectedCurve != null && selectedPointIndex != -1)
            {
                // Перемещаем выбранную точку
                Point newPoint = e.Location;
                switch (selectedPointIndex)
                {
                    case 0: selectedCurve.Start = newPoint; break;
                    case 1: selectedCurve.Control1 = newPoint; break;
                    case 2: selectedCurve.Control2 = newPoint; break;
                    case 3: selectedCurve.Finish = newPoint; break;
                }
                selectedPoint = newPoint;
                Invalidate(); // перерисовать
            }
            else
            {
                // Меняем курсор, если наведены на точку
                var (curve, idx, point) = FindNearestPoint(e.Location);
                this.Cursor = curve != null ? Cursors.Hand : Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            selectedCurve = null;
            selectedPointIndex = -1;
            selectedPoint = null;
            this.Cursor = Cursors.Default;
        }
    }

    // Класс для хранения данных одной кривой Безье
    public class BezierCurve
    {
        public Point Start { get; set; }
        public Point Control1 { get; set; }
        public Point Control2 { get; set; }
        public Point Finish { get; set; }

        public BezierCurve(Point start, Point control1, Point control2, Point finish)
        {
            Start = start;
            Control1 = control1;
            Control2 = control2;
            Finish = finish;
        }
    }
}