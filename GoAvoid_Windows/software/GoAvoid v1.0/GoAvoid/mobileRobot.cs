
//The GoAvoid® software is a path planner application for mobile robots.
//Copyright (C) 2016 Minnetoglu Okan and Conkur Erdinc Sahin

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using usefulFunctions;
using System.Collections;

namespace GoAvoid
{
    public class mobileRobot
    {
        #region var
        public double x1, y1, x2, y2, thetaReal, xa, ya, xaa, yaa, xb, yb, xbb, ybb, xc, yc, xd, yd, boundaryLength = 20d, leng = 25d;//dimension of mobile robot 
        double x11, y11, x22, y22, mag, unitX, unitY, xc1, yc1, xd1, yd1, slope = 1.0d, r, beta = 0, alfa = 0; 
        public List<point> arTrace = new List<point>();
        Pen p, pe, pe1, pc; 
        #endregion
        #region constructor
        public mobileRobot()
        {
            r = Math.Sqrt(leng * leng + leng * leng);
            p = new Pen(Color.Black, 3); p.LineJoin = LineJoin.Round; p.EndCap = LineCap.Round;
            pe = new Pen(Color.Black, 4); pe.LineJoin = LineJoin.Round; pe.EndCap = LineCap.Round;
            pe1 = new Pen(Color.Black, 14); pe1.LineJoin = LineJoin.Round; pe1.StartCap = LineCap.Round; pe1.EndCap = LineCap.Round;
            pc = new Pen(Color.Red, 0);
        } 
        #endregion
        #region drawMobileRobot
        /// <summary>
        /// Draws mobile robot
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="color">Color of the mobile robot</param>
        /// <param name="penWidth">Pen width of the mobile robot drawing</param>
        /// <param name="count">Count of the points that will be drawn on</param>
        public void drawMobileRobot(PaintEventArgs e, Color color, int penWidth, int count)
        {
            if (count > 0)
            {
                setUnitOp(x1, x2, y1, y2);
                x11 = x1 + unitX * leng; y11 = y1 + unitY * leng;
                x22 = x1 - unitX * leng; y22 = y1 - unitY * leng;
                beta = thetaReal + Math.PI / 2.0;
                alfa = thetaReal - Math.PI / 2.0;
                xa = x11 + boundaryLength * slope * Math.Cos(beta); ya = y11 + boundaryLength * slope * Math.Sin(beta);
                xb = x11 + boundaryLength * slope * Math.Cos(alfa); yb = y11 + boundaryLength * slope * Math.Sin(alfa);
                xaa = x11 + boundaryLength * Math.Cos(beta); yaa = y11 + boundaryLength * Math.Sin(beta);
                xbb = x11 + boundaryLength * Math.Cos(alfa); ybb = y11 + boundaryLength * Math.Sin(alfa);
                xc = x22 + boundaryLength * Math.Cos(beta); yc = y22 + boundaryLength * Math.Sin(beta);
                xd = x22 + boundaryLength * Math.Cos(alfa); yd = y22 + boundaryLength * Math.Sin(alfa);
                //Drawing robot wheels
                xc1 = x1 + boundaryLength * Math.Cos(beta); yc1 = y1 + boundaryLength * Math.Sin(beta);
                xd1 = x1 + boundaryLength * Math.Cos(alfa); yd1 = y1 + boundaryLength * Math.Sin(alfa);
                try
                {
                    int boun = 4;
                    PointF[] pf = new PointF[boun];
                    pf[0].X = f.si(xa); pf[0].Y = f.si(ya);
                    pf[1].X = f.si(xb); pf[1].Y = f.si(yb);
                    pf[2].X = f.si(xd); pf[2].Y = f.si(yd);
                    pf[3].X = f.si(xc); pf[3].Y = f.si(yc);
                    Byte[] ppt = new Byte[boun];
                    for (int i = 0; i < boun; i++) ppt[i] = (Byte)PathPointType.Line;
                    GraphicsPath p1 = new GraphicsPath(pf, ppt);
                    PathGradientBrush pgb = new PathGradientBrush(p1);
                    pgb.CenterColor = Color.FromArgb(200, 184, 250, 184);
                    Color[] colors = new Color[1];
                    colors[0] = Color.FromArgb(200, 0, 66, 0);
                    pgb.SurroundColors = colors;
                    Region regg = new Region(p1);
                    e.Graphics.FillRegion(pgb, regg);
                }
                catch (Exception) { }

                if (f.ig(x1) != 0 && f.ig(y1) != 0)
                {
                    e.Graphics.DrawLine(pe1, Convert.ToSingle(xaa), Convert.ToSingle(yaa), Convert.ToSingle(xc1), Convert.ToSingle(yc1));
                    e.Graphics.DrawLine(pe1, Convert.ToSingle(xbb), Convert.ToSingle(ybb), Convert.ToSingle(xd1), Convert.ToSingle(yd1));
                    e.Graphics.DrawLine(p, Convert.ToSingle(xa), Convert.ToSingle(ya), Convert.ToSingle(xc), Convert.ToSingle(yc));
                    e.Graphics.DrawLine(p, Convert.ToSingle(xb), Convert.ToSingle(yb), Convert.ToSingle(xd), Convert.ToSingle(yd));

                    e.Graphics.DrawLine(p, Convert.ToSingle(x11), Convert.ToSingle(y11), Convert.ToSingle(xa), Convert.ToSingle(ya));
                    e.Graphics.DrawLine(p, Convert.ToSingle(x11), Convert.ToSingle(y11), Convert.ToSingle(xb), Convert.ToSingle(yb));
                    e.Graphics.DrawLine(p, Convert.ToSingle(x22), Convert.ToSingle(y22), Convert.ToSingle(xc), Convert.ToSingle(yc));
                    e.Graphics.DrawLine(p, Convert.ToSingle(x22), Convert.ToSingle(y22), Convert.ToSingle(xd), Convert.ToSingle(yd));

                    f.dC(e.Graphics, pc, x1, y1, 5); 
                }
            }
        } 
        #endregion
        #region setUnitOp
        /// <summary>
        /// Evaluates the direction angle of mobile robot
        /// </summary>
        /// <param name="x2">x coordinate of second position</param>
        /// <param name="x1">x coordinate of first position</param>
        /// <param name="y2">y coordinate of second position</param>
        /// <param name="y1">y coordinate of first position</param>
        /// <returns>true</returns>
        public bool setUnitOp(double x2, double x1, double y2, double y1)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            thetaReal = Math.Atan2(y, x);
            mag = Math.Sqrt(x * x + y * y);
            if (mag == 0.0) { return false; }
            unitX = x / mag;
            unitY = y / mag;
            return true;
        } 
        #endregion
        #region setRobotPosition
        /// <summary>
        /// Sets robot position on the path
        /// </summary>
        /// <param name="p">First position coordinates</param>
        /// <param name="p1">Second position coordinates</param>
        public void setRobotPosition(points p, points p1)
        {
            x1 = p.xf; ; y1 = p.yf;
            x2 = p1.xf; y2 = p1.yf;
            arTrace.Add(new point(x1, y1));
        } 
        #endregion
    }
}
