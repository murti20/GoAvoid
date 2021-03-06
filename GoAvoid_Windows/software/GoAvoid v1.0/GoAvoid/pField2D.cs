
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using usefulFunctions;


namespace GoAvoid
{
    public class pField2D
    {
        #region var
        public double robx, roby, top, bottom, left, right, alfa; 
        protected int numberOfIterations;
        protected double[,] gridValue;
        protected bool[,] savePoint;
        protected int numberOfGridLinesX, numberOfGridLinesY;
        public double gridSize;
        public int goalX, goalY;
        public List<points> arPathPoints = new List<points>();
        public List<List<points>> arPathPointsAll = new List<List<points>>();
        public mainForm mf;
        public frameFormBitmap frameform1;
        #endregion
		#region calculateField
        /// <summary>
        /// Performs potential field calculations on the field
        /// </summary>
        /// <param name="numberOfGridLinesX">x grid number of the workspace</param>
        /// <param name="numberOfGridLinesY">y grid number of the workspace</param>
        /// <param name="robx">x coordinate of point that will be calculated</param>
        /// <param name="roby">y coordinate of point that will be calculated</param>
        /// <param name="goalX">x coordinate of goal point</param>
        /// <param name="goalY">y coordinate of goal point</param>
        /// <param name="gridSize">Grid size of the workspace</param>
        /// <param name="numberOfIterations">Number of iterations</param>
		public void calculateField(int numberOfGridLinesX, int numberOfGridLinesY, double robx, double roby, double goalX, double goalY, double gridSize, int numberOfIterations)
		{
			this.numberOfGridLinesX = numberOfGridLinesX;
			this.numberOfGridLinesY = numberOfGridLinesY;
			this.gridSize = gridSize;
			this.goalX = Convert.ToInt32(f.c(goalX) / gridSize);
			this.goalY = Convert.ToInt32(f.c(goalY) / gridSize);
			this.robx = robx;
			this.roby = roby;
			this.numberOfIterations = numberOfIterations;
			DateTime t1 = DateTime.Now;
			gridValue = new double[numberOfGridLinesX + 2, numberOfGridLinesY + 2];
			savePoint = new bool[numberOfGridLinesX + 2, numberOfGridLinesY + 2];
			for (int i = 0; i <= numberOfGridLinesX; i++)//grid values of edge points in workspace are set to true and zero
			{
				for (int j = 0; j <= numberOfGridLinesY; j++)
				{
					gridValue[i, j] = 0;
					savePoint[i, j] = true;
				}
			}
			for (int i = 1; i < numberOfGridLinesX; i++)//other grid values are set to false 
				for (int j = 1; j < numberOfGridLinesY; j++)
					savePoint[i, j] = false;

			for (int i = 0; i < numberOfGridLinesX; i++)
				for (int j = 0; j < numberOfGridLinesY; j++)
					savePoint[i, j] = obstacleBase.ob[i, j];

			gridValue[Convert.ToInt32(this.goalX), Convert.ToInt32(this.goalY)] = -Math.Pow(2, 124);//Goal point grid value is set to  -2¹²⁴ 
			savePoint[Convert.ToInt32(this.goalX), Convert.ToInt32(this.goalY)] = true;// Goal point grid value is set to true              
			for (int iter = 0; iter <= numberOfIterations; iter++)
			{
				for (int j = 1; j <= numberOfGridLinesY; j++)
				{
					for (int i = 1; i <= numberOfGridLinesX; i++)
					{
						if (!savePoint[i, j]) gridValue[i, j] = (gridValue[i, j - 1] + gridValue[i, j + 1] + gridValue[i - 1, j] + gridValue[i + 1, j]) / 4.0;
					}
				}
                mf.inv2(mf.progressBarIteration, numberOfIterations);
                mf.inv(mf.progressBarIteration, iter);
                string s = string.Format("{0}/{1} Iterations Completed", iter, numberOfIterations);
                mf.inv(mf.labelProgress, s);
                if (mf.boolBreakWhile) break;
			}
        } 
		#endregion
		#region field
        /// <summary>
        /// Evaluates field values of the surrounding grid points
        /// </summary>
        /// <param name="a">x coordinate of the point</param>
        /// <param name="b">y coordinate of the point</param>
        /// <returns>Grid point value</returns>
		public double field(double a, double b)
		{
            try
            {
                double dx, dy, bot, bot1, bot2; Int32 sx, sy;
                double x = Convert.ToDouble(a / gridSize);
                double y = Convert.ToDouble(b / gridSize);
                int gridx = numberOfGridLinesX + 3;
                int gridy = numberOfGridLinesY + 3;
                if ((x == gridx) || (y == gridy)) return 0;
                dx = x - Math.Floor(x);
                dy = y - Math.Floor(y);
                sx = Convert.ToInt32(Math.Floor(x));
                sy = Convert.ToInt32(Math.Floor(y));
                bot1 = (1.0 - dy) * gridValue[sx, sy] + dy * gridValue[sx, sy + 1];
                bot2 = (1.0 - dy) * gridValue[sx + 1, sy] + dy * gridValue[sx + 1, sy + 1];
                bot = dx * bot2 + (1.0 - dx) * bot1;
                return (bot);
            }
            catch (Exception)
            {
                return 0;
            }
		} 
		#endregion
		#region getArPathPointsArray
        /// <summary>
        /// Evaluates the path points coordinates
        /// </summary>
        /// <param name="lineLength">Distance between the path points</param>
        /// <param name="boolAll">Back up path points coordinates</param>
		public void getArPathPointsArray(double lineLength, bool boolAll)
		{
			double xmin, ymin;
            int n = 0;
			while (true)
			{
                    n++;
                    top = field(robx, roby - 1);  // field values of the surrounding grid points
                    bottom = field(robx, roby + 1);
                    left = field(robx - 1, roby);
                    right = field(robx + 1, roby);
                    if (top == 0 || bottom == 0 || left == 0 || right == 0)
                    {
                        mf.boolIsRunning = false;
                        mf.inv(mf.buttonGo, true);
                        mf.MessageBoxDetailed("Please try again after increasing the grid size or iteration number", "Path Calculation Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        mf.progressBarIteration.Value = 0;
                        mf.labelProgress.Text = "Operation Terminated";   
                        break;
                    }
                    alfa = Math.Atan2(top - bottom, left - right);   // direction of the largest descent
                    xmin = lineLength * Math.Cos(alfa); ymin = lineLength * Math.Sin(alfa);
                    robx = robx + xmin; roby = roby + ymin;
                    points p = new points();
                    p.x = robx; p.y = roby;
                    p.xf = robx; p.yf = roby;
                    arPathPoints.Add(p);

                    if ((f.ig(robx) > (goalX * gridSize - lineLength) && f.ig(robx) < (goalX * gridSize + lineLength)) && (f.ig(roby) > (goalY * gridSize - lineLength) && f.ig(roby) < (goalY * gridSize + lineLength)))
                    {
                        break;
                    }
              
                    if (n > 300000) break;
                }

            if(boolAll)arPathPointsAll.Add(gc<List<points>>.DeepCopy(arPathPoints));
		}
		#endregion
        #region drawPFTrace
        /// <summary>
        /// Draws potential field trace  
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="rStep">Speed of the robot</param>
        /// <param name="boolAll">Back up path points coordinates</param>
        public void drawPFTrace(PaintEventArgs e, int rStep, bool boolAll)
        {
            try
            {
                Pen pen = new Pen(Color.FromArgb(55, 55, 55), 0f);
                if (!boolAll)
                {
                    for (int i = 0; i < arPathPoints.Count - rStep; i += 50)  //draws potantial field trace filtered
                    {
                        points p = (points)arPathPoints[i];
                        points pp = (points)arPathPoints[i + 1];
                        f.dC(e.Graphics, pen, f.c(p.xf), f.c(p.yf), 1.2);
                    }
                }
                else
                {
                    for (int k = 0; k < arPathPointsAll.Count; k++)
                    {
                        List<points> arP = arPathPointsAll[k];
                        for (int i = 0; i < arP.Count - 1; i += 1)  //draws potantial field trace filtered
                        {
                            points p = (points)arP[i];
                            points pp = (points)arP[i + 1];

                            f.dC(e.Graphics, pen, f.c(p.xf), f.c(p.yf), 1.2);
                        }
                    }
                }
            }
            catch (Exception) { }
        } 
        #endregion
        #region drawPFLargeTrace
        /// <summary>
        /// Draws the large potential field trace
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        /// <param name="rStep">Speed of the robot</param>
        /// <param name="boolAll">Back up path points coordinates</param>
        public void drawPFLargeTrace(PaintEventArgs e, int majorTick)
        {
            Pen pen = new Pen(Color.FromArgb(255, 10, 10, 10), 2);
            double boundaryLength = 8, xa = 0, ya = 0, xb = 0, yb = 0, thetaReal = 0;

            if (arPathPoints.Count > majorTick)
            {
                for (int i = majorTick; i < arPathPoints.Count; i += majorTick)  //draws potantial field trace filtered
                {
                    try
                    {
                        points p = (points)arPathPoints[i];
                        points pp = (points)arPathPoints[i + 1];
                        thetaReal = Math.Atan2(pp.yf - p.yf, pp.xf - p.xf);
                        xa = p.xf + boundaryLength * Math.Cos(thetaReal + Math.PI / 2.0); ya = p.yf + boundaryLength * Math.Sin(thetaReal + Math.PI / 2.0);
                        xb = p.xf + boundaryLength * Math.Cos(thetaReal - Math.PI / 2.0); yb = p.yf + boundaryLength * Math.Sin(thetaReal - Math.PI / 2.0);
                        e.Graphics.DrawLine(pen, Convert.ToSingle(xa), Convert.ToSingle(ya), Convert.ToSingle(xb), Convert.ToSingle(yb));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        } 
        #endregion
    }
}
