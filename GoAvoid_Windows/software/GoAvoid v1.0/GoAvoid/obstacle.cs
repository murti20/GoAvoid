
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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using usefulFunctions;

namespace GoAvoid
{
    [Serializable]
    public class obstacle   // xm:center of the obstacle, xm1: end of the beam, xf: x coordinate of the intersected path point
    {
	    #region var
		public double x1, y1, w, h, wh, x2, y2, directionAngleDeg = 270d, directionAngleRad = 3d*Math.PI / 2d, obstacleSpeed=10, xInc, yInc, ovr = 50d, arbLen=4000;
		public int robotSpeed = 40, shortestPointToObs = 0; //smallStep= robots's step for this obstacle, smallStep and speed has the same units
		public obstacleType ot = obstacleType.rec;
		public placeHolder ph = placeHolder.empty;
		public bool drawPlaceHolder = false, intersected = false, velocityTaken = true, stationary = true, boolDone=false, directionChanged=false;
        //public double placeHolderH = 12, placeHolderW = 12, xm, ym, xm1, ym1, xf, yf;
        public double placeHolderH = 4, placeHolderW = 4, xm, ym, xm1, ym1, xmD,ymD,xf, yf;
		public int robotPosition,fixedRobotPosition; 
		public int obsPosition;
        public List<obstacle> arObsInc = new List<obstacle>();//arS=sorted obstacle array
        public double CriticalRobotSpeed;//Okan robotun engel ile kesiþtiði hýz deðeri
        #endregion
        #region constructor
        public obstacle() { } 
        #endregion
		#region drawObstacle
        /// <summary>
        /// Draws obstacle on the field
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="mf">mainForm mf</param>
		public void drawObstacle(PaintEventArgs e, mainForm mf)
		{
            if (ot == obstacleType.rec) drawRec(e, mf);  
            drawIntersectionCircle(e, mf);
            drawCircle(e);
            if(!mf.boolIsRunning) drawPlaceHolders(e);	
			if (obstacleSpeed != 0) { drawIntersection(e); drawDirectionBeam(e); }
		} 
		#endregion
        #region updateObs
        /// <summary>
        /// Updates obstacle coordinates according to the direction angle
        /// </summary>
        public void updateObs()
		{
			xm = x1 + (x2 - x1) / 2d; ym = y1 + (y2 - y1) / 2d; //the center of the obstacle
            xm1 = xm + arbLen * Math.Cos(directionAngleRad); ym1 = ym + arbLen * Math.Sin(directionAngleRad);
			w = x2 - x1; h = y2 - y1;
            wh = 1.15 * w;
		}
        #endregion
        #region setInc
        /// <summary>
        /// Increments obstacle coordinates
        /// </summary>
        public void setInc()
        {
            x1 += xInc; y1 += yInc;
            x2 += xInc; y2 += yInc;
        }
        #endregion
        #region drawRec
        /// <summary>
        /// Draws rectangle shaped obstacle on the field
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="mf">mainForm mf</param>
        public void drawRec(PaintEventArgs e, mainForm mf)// drawrec
		{

            if ((x2 - x1) < 0 && (y2 - y1) < 0)
            {
                try
                {
                    Rectangle drawArea1 = new Rectangle(f.ig(x2), f.ig(y2), f.abs(f.ig(x2 - x1)), f.abs(f.ig(y2 - y1)));
                    LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 48, 214, 200), Color.FromArgb(150, 0, 40, 255), LinearGradientMode.Vertical);
                    e.Graphics.DrawRectangle(f.pBl(3), f.c(x2), f.c(y2), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                    e.Graphics.FillRectangle(lb, f.c(x2), f.c(y2), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                }
                catch (Exception) { }
            }
			if ((x2 - x1) < 0 && (y2 - y1) > 0)
			{
                try
                {
                    Rectangle drawArea1 = new Rectangle(f.ig(x2), f.ig(y1), f.abs(f.ig(x2 - x1)), f.abs(f.ig(y2 - y1)));
                    LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 48, 214, 200), Color.FromArgb(150, 0, 40, 255), LinearGradientMode.Vertical);
                    e.Graphics.DrawRectangle(f.pBl(3), f.c(x2), f.c(y1), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                    e.Graphics.FillRectangle(lb, f.c(x2), f.c(y1), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                }
                catch (Exception) { }
			}
            if ((x2 - x1) > 0 && (y2 - y1) < 0)
            {
                try
                {
                    Rectangle drawArea1 = new Rectangle(f.ig(x1), f.ig(y2), f.abs(f.ig(x2 - x1)), f.abs(f.ig(y2 - y1)));
                    LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 48, 214, 200), Color.FromArgb(150, 0, 40, 255), LinearGradientMode.Vertical);
                    e.Graphics.DrawRectangle(f.pBl(3), f.c(x1), f.c(y2), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                    e.Graphics.FillRectangle(lb, f.c(x1), f.c(y2), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                }
                catch (Exception) { }
            }
            if ((x2 - x1) > 0 && (y2 - y1) > 0)
            {
                try
                {
                    Rectangle drawArea1 = new Rectangle(f.ig(x1), f.ig(y1), f.abs(f.ig(x2 - x1)), f.abs(f.ig(y2 - y1)));
                    LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 48, 214, 200), Color.FromArgb(150, 0, 40, 255), LinearGradientMode.Vertical);
                    e.Graphics.DrawRectangle(f.pBl(3), f.c(x1), f.c(y1), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                    e.Graphics.FillRectangle(lb, f.c(x1), f.c(y1), f.abs(f.c(x2 - x1)), f.abs(f.c(y2 - y1)));
                }
                catch (Exception) { }
            }
                try
                {
                    if (mf.boolDrawEnclosingCircle && !stationary)
                    {
                        double w = (x2 - x1) / 2d, h = (y2 - y1) / 2d;
                        double r = Math.Sqrt(w * w + h * h);
                        f.dC(e.Graphics, new Pen(Color.Red, 1), xm, ym, r);
                    }
                    if (!stationary)
                    {
                        if (shortestPointToObs > 0)
                        {
                            points p = mf.pf2D.arPathPoints[shortestPointToObs];
                            f.dL(e.Graphics, new Pen(Color.Red, 0), xm, ym, p.xf, p.yf);
                        }
                    }
                }
                catch (Exception) { }
		} 
		#endregion
		#region drawCircle
        /// <summary>
        /// Draws circle at the center of the each obstacle
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
		public void drawCircle(PaintEventArgs e)
		{
			try
			{
				double r = 5;
				Rectangle drawArea1 = new Rectangle(f.ig(x1), f.ig(y1), f.ig(x2 - x1), f.ig(y2 - y1));
				LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(152, 255, 255, 255), Color.FromArgb(183, 255, 255, 255), LinearGradientMode.Horizontal);
                if (f.ig(xm)!= 0 && f.ig(ym)!= 0)
                {
                    e.Graphics.FillEllipse(lb, f.c(xm - r), f.c(ym - r), f.c(2d * r), f.c(2d * r));
                    e.Graphics.DrawEllipse(f.pBl(1), f.c(xm - r), f.c(ym - r), f.c(2d * r), f.c(2d * r)); 
                }
			}
			catch (Exception)
			{
			}
		} 
		#endregion
		#region drawPlaceHolders
        /// <summary>
        /// Draw placeholders to four corner of the obstacle
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
		public void drawPlaceHolders(PaintEventArgs e)
		{
			if (drawPlaceHolder)// placeholder
			{
                placeHolderW = Math.Abs(x2 - x1) / 20;
                placeHolderH = Math.Abs(y2 - y1) / 20;
                if (placeHolderW < 10 || placeHolderH < 10) placeHolderW = 10; placeHolderH = 10;
                if (placeHolderW>placeHolderH) placeHolderH = placeHolderW;
                else placeHolderW = placeHolderH;
				e.Graphics.DrawRectangle(f.pBr(2), f.c(x1 - placeHolderW / 2d), f.c(y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.DrawRectangle(f.pBr(2), f.c(x1 + x2 - x1 - placeHolderW / 2d), f.c(y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.DrawRectangle(f.pBr(2), f.c(x1 - placeHolderW / 2d), f.c(y1 + y2 - y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.DrawRectangle(f.pBr(2), f.c(x1 + x2 - x1 - placeHolderW / 2d), f.c(y1 + y2 - y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));

				e.Graphics.FillRectangle(Brushes.LightGoldenrodYellow, f.c(x1 - placeHolderW / 2d), f.c(y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.FillRectangle(Brushes.LightGoldenrodYellow, f.c(x1 + x2 - x1 - placeHolderW / 2d), f.c(y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.FillRectangle(Brushes.LightGoldenrodYellow, f.c(x1 - placeHolderW / 2d), f.c(y1 + y2 - y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
				e.Graphics.FillRectangle(Brushes.LightGoldenrodYellow, f.c(x1 + x2 - x1 - placeHolderW / 2d), f.c(y1 + y2 - y1 - placeHolderH / 2d), f.abs(f.c(placeHolderW)), f.abs(f.c(placeHolderH)));
			}
		}
		#endregion
		#region drawDirectionBeam
        /// <summary>
        /// Draws direction beam to the center of the obstacle
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
		public void drawDirectionBeam(PaintEventArgs e)
		{
			Pen p2 = new Pen(Color.Black, 3);
			p2.EndCap = LineCap.Custom;
			p2.CustomEndCap = new AdjustableArrowCap(4.0f, 3.0f, true);
			f.dL(e.Graphics, p2, xm, ym, xm + wh * Math.Cos(directionAngleRad), ym + wh * Math.Sin(directionAngleRad));
		}
		#endregion
		#region drawIntersection
        /// <summary>
        /// Draws line from center of the obstacle to the intersected point of the path
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
		public void drawIntersection(PaintEventArgs e)// drawIntersection
		{
			if (intersected)
			{
				f.dL(e.Graphics, f.pGr(1), xm, ym, xf, yf);
			}
		} 
		#endregion
		#region mouseInside
        /// <summary>
        /// Checks whether mouse coordinates are inside the specified coordinates or not
        /// </summary>
        /// <param name="x">x coordinate that will be controlled</param>
        /// <param name="y">y coordinate that will be controlled</param>
        /// <returns>returns draw placeholder or not</returns>
		public bool mouseInside(double x, double y)
		{
			if (x > (x1 - placeHolderW / 2d) && x < (x1 + placeHolderW / 2d) && y > (y1 - placeHolderH / 2d) && y < (y1 + placeHolderH / 2d)) ph = placeHolder.topLeft;
			else if (x > (x1 + x2 - x1 - placeHolderW / 2d) && x < (x1 + x2 - x1 + placeHolderW / 2d) && y > (y1 - placeHolderH / 2d) && y < (y1 + placeHolderH / 2d)) ph = placeHolder.topRight;
			else if (x > (x1 - placeHolderW / 2d) && x < (x1 + placeHolderW / 2d) && y > (y1 + y2 - y1 - placeHolderH / 2d) && y < (y1 + y2 - y1 + placeHolderH / 2d)) ph = placeHolder.bottomLeft;
			else if (x > (x1 + x2 - x1 - placeHolderW / 2d) && x < (x1 + x2 - x1 + placeHolderW / 2d) && y > (y1 + y2 - y1 - placeHolderH / 2d) && y < (y1 + y2 - y1 + placeHolderH / 2d)) ph = placeHolder.bottomRight;
			else if (x > x1 && x < x2 && y > y1 && y < y2) drawPlaceHolder = true;
			else drawPlaceHolder = false;
			if (ph != placeHolder.empty) drawPlaceHolder = true;
			return drawPlaceHolder;
		} 
		#endregion
        #region drawIntersectionCircle
        /// <summary>
        /// Draws an intersection circle to the path point that intersects with the direction beam of the obstacle
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="mf">mainForm mf</param>
        public void drawIntersectionCircle(PaintEventArgs e, mainForm mf)
        {
            double r = 5;
            SolidBrush br = new SolidBrush(Color.FromArgb(160, 255, 0, 0));
            if (f.ig(xf) != 0 && f.ig(yf) != 0)
            {
                e.Graphics.FillEllipse(br, f.c(xf - r), f.c(yf - r), f.c(2d * r), f.c(2d * r));
                e.Graphics.DrawEllipse(f.pB(1), f.c(xf - r), f.c(yf - r), f.c(2d * r), f.c(2d * r));
            }
        } 
        #endregion
    }
}