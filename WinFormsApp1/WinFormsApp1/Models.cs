using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lab8_TrianglesWinForms
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message) : base(message) { }
    }

    public static class Theme
    {
        public static Color KindColor(string kind)
        {
            if (kind == "Рівносторонній") return Color.SeaGreen;
            if (kind == "Прямокутний") return Color.SteelBlue;
            if (kind == "Рівнобедрений") return Color.DarkOrange;
            return Color.DimGray;
        }
    }

    // Базовий клас: довільний трикутник зі сторонами A, B, C
    public class TTriangle
    {
        public int Id { get; set; }
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }

        public TTriangle(double a, double b, double c)
        {
            if (a <= 0 || b <= 0 || c <= 0 || double.IsNaN(a + b + c))
                throw new InvalidInputException("Сторони трикутника мають бути додатними числами.");
            if (a + b <= c || a + c <= b || b + c <= a)
                throw new InvalidInputException("З таких сторін неможливо побудувати трикутник.");
            A = a; B = b; C = c;
        }

        // Віртуальні методи, які перевизначаються у нащадках
        public virtual string Kind { get { return "Довільний"; } }

        public virtual double CalcArea()          // формула Герона
        {
            double p = (A + B + C) / 2;
            return Math.Sqrt(p * (p - A) * (p - B) * (p - C));
        }

        public virtual double CalcPerimeter()
        {
            return A + B + C;
        }

        // Властивості для відображення в таблиці
        public double Area { get { return CalcArea(); } }
        public double Perimeter { get { return CalcPerimeter(); } }
        public string Sides
        {
            get { return string.Format("{0:F2} / {1:F2} / {2:F2}", A, B, C); }
        }
    }

    // Рівносторонній: задається стороною
    public class TEquilateral : TTriangle
    {
        public TEquilateral(double side) : base(side, side, side) { }

        public override string Kind { get { return "Рівносторонній"; } }

        public override double CalcArea()
        {
            return Math.Sqrt(3) / 4 * A * A;
        }
    }

    // Прямокутний: задається двома катетами (A, B), гіпотенуза C обчислюється
    public class TRight : TTriangle
    {
        public TRight(double leg1, double leg2)
            : base(leg1, leg2, Math.Sqrt(leg1 * leg1 + leg2 * leg2)) { }

        public override string Kind { get { return "Прямокутний"; } }

        public override double CalcArea()
        {
            return A * B / 2;
        }
    }

    // Рівнобедрений: задається основою (C) та бічною стороною (A = B)
    public class TIsosceles : TTriangle
    {
        public TIsosceles(double baseSide, double leg) : base(leg, leg, baseSide) { }

        public override string Kind { get { return "Рівнобедрений"; } }

        public override double CalcArea()
        {
            return C / 2 * Math.Sqrt(A * A - C * C / 4);
        }
    }

    // Колекція трикутників
    public class TriangleCollection
    {
        private readonly List<TTriangle> items = new List<TTriangle>();
        private readonly Random rnd = new Random();

        public List<TTriangle> All { get { return items; } }

        public int NextId()
        {
            return items.Count == 0 ? 1 : items.Max(t => t.Id) + 1;
        }

        public void Add(TTriangle t) { items.Add(t); }

        public bool Remove(TTriangle t) { return items.Remove(t); }

        public void Replace(TTriangle oldT, TTriangle newT)
        {
            int i = items.IndexOf(oldT);
            if (i >= 0) items[i] = newT;
        }

        // Випадково створює n трикутників трьох видів (сума кількостей = n)
        public void Generate(int n)
        {
            items.Clear();
            int eq = rnd.Next(n + 1);
            int rt = rnd.Next(n - eq + 1);
            int iso = n - eq - rt;

            for (int i = 0; i < eq; i++)
                Add(new TEquilateral(RandSide()) { Id = NextId() });
            for (int i = 0; i < rt; i++)
                Add(new TRight(RandSide(), RandSide()) { Id = NextId() });
            for (int i = 0; i < iso; i++)
            {
                double leg = RandSide();
                double baseSide = Math.Round(leg * (0.2 + rnd.NextDouble() * 1.6), 1);
                if (baseSide <= 0) baseSide = 0.1;
                Add(new TIsosceles(baseSide, leg) { Id = NextId() });
            }
        }

        private double RandSide()
        {
            return Math.Round(1 + rnd.NextDouble() * 19, 1);
        }

        public int CountEquilateral { get { return items.OfType<TEquilateral>().Count(); } }
        public int CountRight { get { return items.OfType<TRight>().Count(); } }
        public int CountIsosceles { get { return items.OfType<TIsosceles>().Count(); } }

        // Сума площ рівносторонніх
        public double SumAreaEquilateral()
        {
            return items.OfType<TEquilateral>().Sum(t => t.CalcArea());
        }

        // Сума площ прямокутних
        public double SumAreaRight()
        {
            return items.OfType<TRight>().Sum(t => t.CalcArea());
        }

        // Сума периметрів рівнобедрених
        public double SumPerimeterIsosceles()
        {
            return items.OfType<TIsosceles>().Sum(t => t.CalcPerimeter());
        }
    }
}
