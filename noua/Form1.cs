using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace noua
{
    public partial class Form1 : Form
    {
        private List<PointF> polygonPoints = new List<PointF>();
        private List<List<PointF>> monotonePolygons = new List<List<PointF>>();
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Paint += Form1_Paint;
            this.MouseDown += Form1_MouseDown;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                polygonPoints.Add(new PointF(e.X, e.Y));
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                PartitionPolygon();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.Black);

            if (polygonPoints.Count > 1)
                g.DrawPolygon(p, polygonPoints.ToArray());

            foreach (var polygon in monotonePolygons)
            {
                if (polygon.Count > 1)
                    g.DrawPolygon(p, polygon.ToArray());
            }
        }

        private void PartitionPolygon()
        {
            monotonePolygons.Clear();
            if (polygonPoints.Count < 3)
            {
                MessageBox.Show("Poligonul trebuie să aibă cel puțin 3 vârfuri.");
                return;
            }

            // Sortăm punctele poligonului în funcție de coordonata y și, în caz de egalitate, în funcție de coordonata x
            polygonPoints.Sort((a, b) =>
            {
                if (a.Y != b.Y)
                    return a.Y.CompareTo(b.Y);
                else
                    return a.X.CompareTo(b.X);
            });

            // Găsim vârfurile de întoarcere și împărțim poligonul în poligoane monotone
            List<int> ears = FindConvexVertices(polygonPoints);
            foreach (var ear in ears)
            {
                List<PointF> monotonePolygon = TriangulateMonotonePolygon(polygonPoints, ear);
                monotonePolygons.Add(monotonePolygon);
            }

            Invalidate();
        }

        // Funcție pentru găsirea vârfurilor de întoarcere (vârfuri convexe)
        private List<int> FindConvexVertices(List<PointF> points)
        {
            List<int> ears = new List<int>();
            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;
                int next = (i + 1) % n;
                PointF p = points[i];
                PointF pPrev = points[prev];
                PointF pNext = points[next];

                if (IsConvex(pPrev, p, pNext))
                    ears.Add(i);
            }

            return ears;
        }

        // Funcție pentru verificarea dacă un vârf este convex
        private bool IsConvex(PointF a, PointF b, PointF c)
        {
            float crossProduct = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            return crossProduct >= 0;
        }
        // Funcție pentru triunghiulare poligonului monotone pornind de la un vârf de întoarcere
        private List<PointF> TriangulateMonotonePolygon(List<PointF> points, int earIndex)
        {
            List<PointF> monotonePolygon = new List<PointF>();
            int n = points.Count;
            int prev = (earIndex - 1 + n) % n;
            int next = (earIndex + 1) % n;

            monotonePolygon.Add(points[earIndex]);
            monotonePolygon.Add(points[prev]);

            // Triunghiulare în direcție descendentă
            for (int i = prev - 1; i >= 0; i--)
            {
                monotonePolygon.Add(points[i]);
                if (i == 0 || IsConvex(points[i - 1], points[i], points[i + 1]))
                {
                    monotonePolygon.Add(points[next]);
                    break;
                }
            }

            monotonePolygon.Add(points[next]);

            // Triunghiulare în direcție ascendentă
            for (int i = next + 1; i < n; i++)
            {
                monotonePolygon.Add(points[i]);
                if (i == n - 1 || IsConvex(points[i - 1], points[i], points[i + 1]))
                {
                    monotonePolygon.Add(points[prev]);
                    break;
                }
            }

            return monotonePolygon;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PartitionPolygon();
        }
    }
}
