
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using usefulFunctions;

namespace GoAvoid
{
    public class obstacleBase
    {
        #region var
        bool newObCreated = false;
        int xGrid = 0, yGrid = 0, xGrid2 = 0, yGrid2 = 0, wGrid = 0, hGrid = 0, xGridLim = 0, yGridLim = 0;
        public int chosenShape = -1, smallStep = 1, numberOfGridLinesX, numberOfGridLinesY;
        double preX, preY;
        public double wsWidth, wsHeight, gridSize;//for grid lines in the workspace
        public static bool[,] ob, or;
        public List<obstacle> ar = new List<obstacle>(); //obstacle array
        public List<obstacle> arM = new List<obstacle>(); //obstacle array containing only moving obstacles
        public List<obstacle> arD = new List<obstacle>(); // default obstacle array
        public List<obstacle> arS = new List<obstacle>();//arS=sorted obstacle array
        public List<obstacle> arObsInc = new List<obstacle>();//arObsInc=Obs path drawing obstacle array
        public Color color, fillColor;
        mainForm mf; obstacleType ot = obstacleType.mouse;
        public pField2D pf2D;
        public bool isSnapToGrid;
        int yGridFirst, yGridLast;
        public bool boolIntersectedWithObs = false;
        public bool boolIntersectedWithObsLine = false;
        public List<int> arIntersectedPoints = new List<int>(); 
        public int cg(double x) { return Convert.ToInt32(Math.Round(x / gridSize) * gridSize); }
        public int cg(int x) { return Convert.ToInt32(Math.Round(Convert.ToDouble(x) / gridSize) * gridSize); }
        public double vm(double vx1, double vy1, double vx2, double vy2) { return vx1 * vy2 - vy1 * vx2; } //vector multiplication
        #endregion
        #region constructor
        public obstacleBase(mainForm mf)
        {
            this.mf = mf;
            color = Color.Black;
            gridSize = 20;
            isSnapToGrid = false;
            wsWidth = 1600;
            wsHeight = 900;
            numberOfGridLinesX = Convert.ToInt32((wsWidth) / gridSize);
            numberOfGridLinesY = Convert.ToInt32((wsHeight) / gridSize);
            ob = new bool[numberOfGridLinesX, numberOfGridLinesY+1];
            or = new bool[numberOfGridLinesX, numberOfGridLinesY+1];
        }
        #endregion
        #region paint
        /// <summary>
        /// Draws obstacles to the field
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        public void paint(PaintEventArgs e)
        {
            for (int i = 0; i < ar.Count; i++)
            {
                obstacle o = ar[i];
                o.drawObstacle(e, mf);
            }
        }
        #endregion
        #region obsPathDraw
        /// <summary>
        /// Draws obstacles path while it's moving
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        public void obsPathDraw(PaintEventArgs e)
        {
            for (int i = 0; i < ar.Count; i++)
            {
                for (int j = i; j < arObsInc.Count; j += ar.Count * mf.obsPathPointInc)
                {
                    obstacle oo = arObsInc[j];
                    f.dC(e.Graphics, f.pBl(0), oo.xm, oo.ym, 0.8f);
                }
            }
        }
        #endregion
        #region mouseDown
        /// <summary>
        /// Chooses or draws obstacle to the field
        /// </summary>
        /// <param name="e">MouseEventArgs e</param>
        /// <param name="eX">Mouse x coordinate</param>
        /// <param name="eY">Mouse y coordinate</param>
        public void mouseDown(MouseEventArgs e, double eX, double eY)
        {
            chosenShape = -1;
            if (e.Button == MouseButtons.Left)
            {
                if ((eX> wsWidth || eY> wsHeight ) || (eX<0 || eY<0))
                {
                    mf.mouse();
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; o.drawPlaceHolder = false; o.ph = placeHolder.empty; }
                    return;
                }

                if (ar.Count == 0) ot = obstacleType.rec; mf.rect();

                for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; if (o.mouseInside(eX, eY)) { ot = obstacleType.mouse; mf.mouse(); break; } else { ot = obstacleType.rec; mf.rect(); } }
                
                if (ot == obstacleType.mouse)
                {
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; o.drawPlaceHolder = false; o.ph = placeHolder.empty; }
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; if (o.mouseInside(eX, eY)) { chosenShape = i; break; } }
                    preX = eX; preY = eY;
                    if (chosenShape > -1)
                    {
                        obstacle o = ar[chosenShape];
                        mf.hScrollBarObstacleDirection.Value = f.ig(o.directionAngleDeg);
                        mf.hScrollBarObstacleSpeed.Value = f.ig(o.obstacleSpeed);
                        mf.textBoxObstacleDirection.Text = o.directionAngleDeg.ToString();
                        mf.textBoxObstacleSpeed.Text = o.obstacleSpeed.ToString();
                    }
                }
                else if(!mf.boolIsRunning)
                {
                    obstacle o = new obstacle();
                    newObCreated = true;
                    o.ot = ot;
                    ar.Add(o);
                    if (isSnapToGrid) { eX = cg(eX); eY = cg(eY); }
                    o.x1 = eX; o.y1 = eY;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if ((eX > wsWidth || eY > wsHeight) || (eX < 0 || eY < 0))
                {
                    mf.mouse();
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; o.drawPlaceHolder = false; o.ph = placeHolder.empty; }
                    return;
                }

                if (ar.Count == 0) ot = obstacleType.rec; mf.rect();

                for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; if (o.mouseInside(eX, eY)) { ot = obstacleType.mouse; mf.mouse(); break; } else { ot = obstacleType.rec; mf.rect(); } }

                if (ot == obstacleType.mouse)
                {
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; o.drawPlaceHolder = false; o.ph = placeHolder.empty; }
                    for (int i = 0; i < ar.Count; i++) { obstacle o = ar[i]; if (o.mouseInside(eX, eY)) { chosenShape = i; break; } }
                    preX = eX; preY = eY;
                    if (chosenShape > -1)
                    {
                        obstacle o = ar[chosenShape];
                        mf.hScrollBarObstacleDirection.Value = f.ig(o.directionAngleDeg);
                        mf.hScrollBarObstacleSpeed.Value = f.ig(o.obstacleSpeed);
                        mf.textBoxObstacleDirection.Text = o.directionAngleDeg.ToString();
                        mf.textBoxObstacleSpeed.Text = o.obstacleSpeed.ToString();
                    }
                }
            }
        }
        #endregion
        #region mouseMove
        /// <summary>
        /// Moves or draws obstacle to the field
        /// </summary>
        /// <param name="e">MouseEventArgs e</param>
        /// <param name="eX">Mouse x coordinate</param>
        /// <param name="eY">Mouse y coordinate</param>
        /// 
        double x1, y1, x2, y2;
        int newPositionX, newPositionY;
        public void mouseMove(MouseEventArgs e, double eX, double eY)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ot == obstacleType.mouse && chosenShape > -1 && !mf.boolIsRunning)
                {
                    obstacle o = ar[chosenShape];
                    x1 = o.x1; y1 = o.y1; x2 = o.x2; y2 = o.y2;

                    if (o.ph == placeHolder.topLeft)
                    {
                        o.x1 += eX - preX;
                        o.y1 += eY - preY;
                        CheckCursorIsOutside(ref eX, ref eY);
                        CheckObsIsOutsideWhileResizing(o);
                    }
                    else if (o.ph == placeHolder.topRight)
                    {
                        o.x2 += eX - preX;
                        o.y1 += eY - preY;
                        CheckCursorIsOutside(ref eX, ref eY);
                        CheckObsIsOutsideWhileResizing(o);
                    }
                    else if (o.ph == placeHolder.bottomLeft)
                    {
                        o.x1 += eX - preX;
                        o.y2 += eY - preY;
                        CheckCursorIsOutside(ref eX, ref eY);
                        CheckObsIsOutsideWhileResizing(o);
                    }
                    else if (o.ph == placeHolder.bottomRight)
                    {
                        o.x2 += eX - preX;
                        o.y2 += eY - preY;
                        CheckCursorIsOutside(ref eX, ref eY);
                        CheckObsIsOutsideWhileResizing(o);
                    }
                       
                    else if (o.drawPlaceHolder)
                    {
                        //cursor outside restriction
                        mf.groupBoxObstacleMotion.Visible = false;
                        CheckCursorIsOutside(ref eX, ref eY);
                        CheckObsIsOutsideWhileMoving(ref eX, ref eY, o);
                    }

                    if ((f.abs(o.x2 - o.x1) < 40 || f.abs(o.y2 - o.y1) < 40) || o.drawPlaceHolder == false)
                    {
                        o.x1 = x1; o.y1 = y1; o.x2 = x2; o.y2 = y2;
                    }
                    o.updateObs();
                    preX = eX; preY = eY;

                    if (mf.drawGrid) { getObstaclesForPF(); mf.Invalidate(); }      
                }
                else if (newObCreated)
                {
                    obstacle o = ar[ar.Count - 1];
                    o.x2 = eX; o.y2 = eY;
                    CheckCursorIsOutside(ref eX, ref eY);
                    mf.Invalidate();
                }
            }
        }

        #region CheckCursorIsOutside
        /// <summary>
        /// Checks whether cursor is outside of the workspace
        /// </summary>
        /// <param name="eX">x coordinate of the cursor with respect to the origin</param>
        /// <param name="eY">y coordinate of the cursor with respect to the origin</param>

        public void CheckCursorIsOutside(ref double eX, ref double eY)
        {
            if (eX < 0)
            {
                eX = 20;
                newPositionX = f.ig((eX + mf.ofx) * (mf.mw * mf.sf));
                Cursor.Position = new Point(newPositionX, Cursor.Position.Y);
            }
            if (eX > wsWidth)
            {
                eX = wsWidth;
                newPositionX = f.ig((eX + mf.ofx) * (mf.mw * mf.sf));
                Cursor.Position = new Point(newPositionX, Cursor.Position.Y);
            }
            if (eY < 0)
            {
                eY = 0;
                newPositionY = -f.ig((eY + mf.ofy) * (mf.mw * mf.sf));
                Cursor.Position = new Point(Cursor.Position.X, newPositionY + 79);//Okan form textden baþlayýp menülerin deneme sonucu 79 piksel olduðu tespit edilmiþtir. Dolayýsýyla +79 piksel aþaðý
                //Okan cursor.position fonksiyonu 0,0 ý formun textinin altýndan aldýðý için x de problem olmuyor y de 79 piksel eklemek gerekli menüler kadar
            }
            if (eY > wsHeight)
            {
                eY = wsHeight;
                newPositionY = -f.ig((eY + mf.ofy) * (mf.mw * mf.sf));
                Cursor.Position = new Point(Cursor.Position.X, newPositionY + 79);//Okan form textden baþlayýp menülerin deneme sonucu 79 piksel olduðu tespit edilmiþtir. Dolayýsýyla +79 piksel aþaðý
                //Okan cursor.position fonksiyonu 0,0 ý formun textinin altýndan aldýðý için x de problem olmuyor y de 79 piksel eklemek gerekli menüler kadar
            }
        } 
        #endregion
        #region CheckObsIsOutsideWhileResizing
        /// <summary>
        /// Checks whether obstacle is outside of the workspace while resizing it
        /// </summary>
        /// <param name="o">obstacle type object</param>
        private void CheckObsIsOutsideWhileResizing(obstacle o)
        {
            if (o.x1 < 0) o.x1 = 0;
            if (o.y1 < 0) o.y1 = 0;
            if (o.x2 > wsWidth) o.x2 = wsWidth;
            if (o.y2 > wsHeight) o.y2 = wsHeight;

            if (o.x1 > wsWidth) o.x1 = wsWidth;
            if (o.y1 > wsHeight) o.y1 = wsHeight;
            if (o.x2 < 0) o.x2 = 0;
            if (o.y2 < 0) o.y2 = 0;

            mf.Invalidate();
        } 
        #endregion
        #region CheckObsIsOutsideWhileMoving
        /// <summary>
        /// Checks whether obstacle is outside of the workspace while moving it
        /// </summary>
        /// <param name="eX">x coordinate of the cursor with respect to the origin</param>
        /// <param name="eY">y coordinate of the cursor with respect to the origin</param>
        /// <param name="o">obstacle type object</param>

        private void CheckObsIsOutsideWhileMoving(ref double eX, ref double eY, obstacle o)
        {
            if (eX <= 0) eX = 0; if (eX >= wsWidth) eX = wsWidth;
            if (eY <= 0) eY = 0; if (eY >= wsHeight) eY = wsHeight;
            if (o.x1 >= 0 && o.x1 <= wsWidth - o.w)
            {
                o.x1 += eX - preX;
                if (o.x1 < 0) o.x1 = 0;//workspace sol çizgisini geçmemesi için engeli taþýrken
                if (o.x1 > wsWidth - o.w) o.x1 = wsWidth - o.w;//workspace sað çizgisini geçmemesi için engeli taþýrken
            }
            if (o.x2 >= o.w && o.x2 <= wsWidth)
            {
                o.x2 += eX - preX;
                if (o.x2 < o.w) o.x2 = o.w;//workspace sol çizgisini geçmemesi için engeli taþýrken
                if (o.x2 > wsWidth) o.x2 = wsWidth;//workspace sað çizgisini geçmemesi için engeli taþýrken
            }
            if (o.y1 >= 0 && o.y1 <= wsHeight - o.h)
            {
                o.y1 += eY - preY;
                if (o.y1 < 0) o.y1 = 0;//workspace alt çizgisini geçmemesi için engeli taþýrken
                if (o.y1 > wsHeight - o.h) o.y1 = wsHeight - o.h;//workspace üst çizgisini geçmemesi için engeli taþýrken
            }
            if (o.y2 >= o.h && o.y2 <= wsHeight)
            {
                o.y2 += eY - preY;
                if (o.y2 < o.h) o.y2 = o.h;//workspace alt çizgisini geçmemesi için engeli taþýrken
                if (o.y2 > wsHeight) o.y2 = wsHeight;//workspace üst çizgisini geçmemesi için engeli taþýrken
            }
        } 
        #endregion
        #endregion
        #region mouseUp
        /// <summary>
        /// Completes moving or drawing obstacle operation
        /// </summary>
        /// <param name="e">MouseEventArgs e</param>
        /// <param name="eX">Mouse x coordinate</param>
        /// <param name="eY">Mouse y coordinate</param>
        /// <param name="boolFixedSize">Fixed size obstacle</param>
        /// <param name="lineLength">Distance between the path points</param>
        public void mouseUp(MouseEventArgs e, double eX, double eY, bool boolFixedSize, double lineLength)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ot == obstacleType.mouse && ar.Count > 0 && chosenShape > -1)
                {
                    obstacle o = ar[chosenShape];
                    if (isSnapToGrid) { o.x1 = cg(o.x1); o.y1 = cg(o.y1); o.x2 = cg(o.x2); o.y2 = cg(o.y2); }
     
                    o.updateObs(); updateObs(); 
                    saveInitialObstacleValue(chosenShape, o);
                    mf.groupBoxObstacleMotion.Visible = true;
                }
                else if (newObCreated)
                {
                    obstacle o = ar[ar.Count - 1];

                    if (boolFixedSize) { o.x2 = o.x1 + 40; o.y2 = o.y1 + 40; }  //dimension of the fixed size obstacle
                    else
                    {
                        o.x2 = eX; o.y2 = eY;

                        if ((o.x2 - o.x1) < 0 && (o.y2 - o.y1) < 0)
                        {
                            o.x2 = o.x1; o.y2 = o.y1; o.x1 = eX; o.y1 = eY;
                        }
                        else if ((o.x2 - o.x1) < 0 && (o.y2 - o.y1) > 0)
                        {
                            o.x2 = o.x1; o.x1 = eX;
                        }
                        if ((o.x2 - o.x1) > 0 && (o.y2 - o.y1) < 0)
                        {
                            o.y2 = o.y1; o.y1 = eY;
                        }
                        if (isSnapToGrid) { o.x1 = cg(o.x1); o.y1 = cg(o.y1); o.x2 = cg(o.x2); o.y2 = cg(o.y2); }
                    }

                    newObCreated = false;
                    updateObs();
                    setInc1(lineLength, o);
                    saveInitialObstacleValue(o);

                    if (f.abs(o.x2-o.x1)<20 || f.abs(o.y2-o.y1)<20)
                    {
                                  if(ar.Count>0) ar.RemoveAt(ar.Count - 1);
                        if(arD.Count>0) arD.RemoveAt(arD.Count - 1);
                    }
                }
            }
        }
        #endregion
        #region getObstaclesForPF
        /// <summary>
        /// Assigns values to the grids for potential field calculations
        /// </summary>
        public void getObstaclesForPF()
        {
            try
            {
                numberOfGridLinesX = Convert.ToInt32(wsWidth / gridSize);
                numberOfGridLinesY = Convert.ToInt32(wsHeight / gridSize);
                ob = new bool[numberOfGridLinesX + 1, numberOfGridLinesY + 1];
                or = new bool[numberOfGridLinesX + 1, numberOfGridLinesY + 1];
                for (int i = 0; i < ar.Count; i++)
                {
                    obstacle o = ar[i];
                    o.updateObs();
                    if (o.stationary)
                    {
                        xGrid = Convert.ToInt32(o.x1 / gridSize);
                        yGrid = Convert.ToInt32(o.y1 / gridSize);
                        xGrid2 = Convert.ToInt32(o.x2 / gridSize);
                        yGrid2 = Convert.ToInt32(o.y2 / gridSize);
                        wGrid = Convert.ToInt32(Math.Abs(o.x2 - o.x1) / gridSize);
                        hGrid = Convert.ToInt32(Math.Abs(o.y2 - o.y1) / gridSize);

                        xGridLim = xGrid + wGrid;
                        yGridLim = yGrid + hGrid;
                        yGridFirst = yGrid;
                        yGridLast = yGridLim;

                        if (xGridLim > numberOfGridLinesX) { xGridLim = numberOfGridLinesX; }
                        if (yGridLim > numberOfGridLinesY) { yGridLim = numberOfGridLinesY; }
                        if (xGrid < 0) xGrid = 0; if (yGrid < 0) yGrid = 0;

                        for (int j = yGridFirst; j <= yGridLast; j++)
                        {
                            for (int ke = xGrid; ke <= xGridLim; ke++)
                            {
                                ob[ke, j] = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                f.m(ex.ToString());
            }
        }
        #endregion
        #region boundryCheck
        /// <summary>
        /// Checks whether obstacle is out of the boundary or not
        /// </summary>
        public void boundryCheck()// boundary check
        {
            if (mf.frameForm1.toolStripButtonStopObstacle.Checked) mf.boolStopObstacle = true;
            else mf.boolStopObstacle = false;
            for (int k = 0; k < ar.Count; k++)
            {
                obstacle o = ar[k];

                    if (o.y2 > wsHeight || o.y2 < o.h || (o.x2 > wsWidth) || o.x2 < o.w)
                    {
                        if (mf.boolStopObstacle)
                        {
                            o.xInc = 0; o.yInc = 0;
                        }
                        else
                        {
                            o.xInc = -o.xInc; o.yInc = -o.yInc;
                            o.directionAngleRad = Math.PI + o.directionAngleRad;
                            o.directionChanged = true;
                        }
                }
                for (int j = 0; j < ar.Count; j++)
                {
                    if (k != j)
                    {
                        obstacle o1 = ar[j];
                        if (o1.stationary)
                        {
                            if (o.xm > o1.x1 && o.xm < o1.x2 && o.ym > o1.y1 && o.ym < o1.y2)
                            {
                                if (mf.boolStopObstacle)
                                {
                                    o.xInc = 0; o.yInc = 0;
                                }
                                else
                                {
                                    o.xInc = -o.xInc; o.yInc = -o.yInc;
                                    o.directionAngleRad = Math.PI + o.directionAngleRad;
                                    o1.xInc = -o1.xInc; o1.yInc = -o1.yInc;
                                    o1.directionAngleRad = Math.PI + o1.directionAngleRad;
                                    o.directionChanged = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region updateObs
        /// <summary>
        /// Updates obstacle values
        /// </summary>
        public void updateObs()
        {
            for (int j = 0; j < ar.Count; j++)
            {
                obstacle o = ar[j];
                o.updateObs();
                #region Obstacle Path Points
                obstacle o2 = new obstacle();//obstacle path
                o2.xm = o.xm;
                o2.ym = o.ym;
                arObsInc.Add(o2);
                #endregion
            }
        }
        #endregion
        #region setInc
        /// <summary>
        /// Increments obstacles coordinates
        /// </summary>
        public void setInc()
        {
            for (int j = 0; j < ar.Count; j++)
            {
                obstacle o = ar[j];
                o.setInc();
            }
        }
        #endregion
        #region drawGrid
        /// <summary>
        /// Draws grid points to the field
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        public void drawGrid(PaintEventArgs e)
        {
            numberOfGridLinesX = Convert.ToInt32(wsWidth / gridSize);
            numberOfGridLinesY = Convert.ToInt32(wsHeight / gridSize);
            Pen p = new Pen(Color.FromArgb(50, 0, 100, 190), 0);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            for (int i = 0; i <= numberOfGridLinesX; i += 1) e.Graphics.DrawLine(p, Convert.ToSingle(i) * f.si(gridSize), 0.0f, Convert.ToSingle(i) * f.si(gridSize), Convert.ToSingle(wsHeight));
            for (int i = 0; i <= numberOfGridLinesY; i += 1) e.Graphics.DrawLine(p, 0.0f, Convert.ToSingle(i) * f.si(gridSize), Convert.ToSingle(wsWidth), Convert.ToSingle(i) * f.si(gridSize));
        }
        #endregion
        #region drawObstaclePointsOnGrid
        /// <summary>
        /// Draws obstacle points on the grid
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        public void drawObstaclePointsOnGrid(PaintEventArgs e)
        {
            try
            {
                Pen p1 = new Pen(Color.DarkTurquoise, 10); float fe = 1f;
                for (int i = 0; i < numberOfGridLinesX; i++)
                {
                    for (int j = 0; j < numberOfGridLinesY; j++)
                    {
                        if (ob[i, j] == true || i == 0 || j == 0)
                        {
                            e.Graphics.DrawEllipse(p1, i * Convert.ToSingle(gridSize) - fe, j * Convert.ToSingle(gridSize) - fe, 2.0f * fe, 2.0f * fe);
                        }
                    }
                }
                for (int i = 0; i <= numberOfGridLinesX; i++) e.Graphics.DrawEllipse(p1, i * Convert.ToSingle(gridSize) - fe, numberOfGridLinesY * Convert.ToSingle(gridSize) - fe, 2.0f * fe, 2.0f * fe);
                for (int j = 0; j <= numberOfGridLinesY; j++) e.Graphics.DrawEllipse(p1, numberOfGridLinesX * Convert.ToSingle(gridSize) - fe, j * Convert.ToSingle(gridSize) - fe, 2.0f * fe, 2.0f * fe);
            }
            catch (Exception)
            {
                
            }
        }
        #endregion
        #region saveInitialObstacleValue
        /// <summary>
        /// Adding values to temporary array to maintain changes on the obstacle
        /// </summary>
        /// <param name="chosenShape">Choosen shape index</param>
        /// <param name="o">Object to reach obstacle values</param>
        public void saveInitialObstacleValue(int chosenShape, obstacle o)
        {
            obstacle oD = arD[chosenShape];
            oD.x1 = o.x1;
            oD.y1 = o.y1;
            oD.x2 = o.x2;
            oD.y2 = o.y2;
            oD.xInc = o.xInc;
            oD.yInc = o.yInc;
            oD.directionAngleDeg = o.directionAngleDeg;
            oD.directionAngleRad = o.directionAngleRad;
            oD.obstacleSpeed = o.obstacleSpeed;

            if (o.obstacleSpeed == 0) o.stationary = true; else o.stationary = false;
            oD.stationary = o.stationary;

            oD.xm = o.xm;
            oD.ym = o.ym;
            oD.wh = o.wh;

            o.xmD = o.xm;
            o.ymD = o.ym;
        }
        #endregion
        #region saveInitialObstacleValue
        /// <summary>
        /// Adding values to temporary array to maintain changes on the obstacle
        /// </summary>
        /// <param name="o">Object to reach obstacle values</param>
        public void saveInitialObstacleValue(obstacle o)
        {
            obstacle oD = new obstacle();
            oD.x1 = o.x1;
            oD.y1 = o.y1;
            oD.x2 = o.x2;
            oD.y2 = o.y2;
            oD.xInc = o.xInc;
            oD.yInc = o.yInc;
            oD.directionAngleDeg = o.directionAngleDeg;
            oD.directionAngleRad = o.directionAngleRad;
            oD.obstacleSpeed = o.obstacleSpeed;

            if (o.obstacleSpeed == 0) o.stationary = true; else o.stationary = false;
            oD.stationary = o.stationary;

            oD.xm = o.xm;
            oD.ym = o.ym;
            oD.wh = o.wh;
            arD.Add(oD);

            o.xmD = o.xm;
            o.ymD = o.ym;
        }
        #endregion
        #region getInitialObstacleValues
        /// <summary>
        /// Adding values to main array to be active
        /// </summary>
        public void getInitialObstacleValues()
        {
            ar.Clear();
            for (int i = 0; i < arD.Count; i++)
            {
                obstacle oD = arD[i];
                obstacle o = new obstacle();
                o.x1 = oD.x1;
                o.y1 = oD.y1;
                o.x2 = oD.x2;
                o.y2 = oD.y2;
                o.xInc = oD.xInc;
                o.yInc = oD.yInc;
                o.directionAngleDeg = oD.directionAngleDeg;
                o.directionAngleRad = oD.directionAngleRad;
                o.obstacleSpeed = oD.obstacleSpeed;
                o.stationary = oD.stationary;
                o.xm = oD.xm;
                o.ym = oD.ym;
                o.wh = oD.wh;
                o.xmD = o.xm;
                o.ymD = o.ym;
                ar.Add(o);
            }
        }
        #endregion
        #region getDirectionAndSpeed
        /// <summary>
        /// Gets directions and speeds of the obstacles drawn on the field
        /// </summary>
        /// <param name="chosenOb">Choosen obstacle index</param>
        /// <param name="directionAngleDeg">Direction angle as degree</param>
        /// <param name="speed">Obstacle speed</param>
        /// <param name="lineLength">Distance between the path points</param>
        public void getDirectionAndSpeed(int chosenOb, double directionAngleDeg, double speed, double lineLength)
        {
            if (chosenOb > -1)
            {
                obstacle o = ar[chosenOb];
                o.directionAngleDeg = directionAngleDeg;
                o.obstacleSpeed = speed;
                if (o.obstacleSpeed == 0) o.stationary = true; else o.stationary = false;
                o.directionAngleRad = directionAngleDeg * (Math.PI / 180d);
                o.xInc = speed * lineLength * Math.Cos(o.directionAngleRad);
                o.yInc = speed * lineLength * Math.Sin(o.directionAngleRad);
            }
        }
        #endregion
        #region setIncForAll
        /// <summary>
        /// Sets increment value for all obstacles
        /// </summary>
        /// <param name="lineLength">Distance between the path points</param>
        public void setIncForAll(double lineLength)
        {
            for (int i = 0; i < ar.Count; i++)
            {
                obstacle o = ar[i];
                o.xInc = o.obstacleSpeed * lineLength * Math.Cos(o.directionAngleRad);
                o.yInc = o.obstacleSpeed * lineLength * Math.Sin(o.directionAngleRad);
                saveInitialObstacleValue(i, ar[i]);
            }
        }
        #endregion
        #region setInc1
        /// <summary>
        /// Sets increment value for specified obstacle
        /// </summary>
        /// <param name="lineLength">Distance between the path points</param>
        /// <param name="o">Object to reach obstacle values</param>
        public void setInc1(double lineLength, obstacle o)
        {
            o.xInc = o.obstacleSpeed * lineLength * Math.Cos(o.directionAngleRad);
            o.yInc = o.obstacleSpeed * lineLength * Math.Sin(o.directionAngleRad);
        }
        #endregion
        #region getIntersectionWithPath
        /// <summary>
        /// Gets the intersection point with the obstacle on the path 
        /// </summary>
        /// <param name="currentP">Current position</param>
        /// <returns>arS</returns>
        public List<obstacle> getIntersectionWithPath(int currentP)
        {
            try
            {
                arM.Clear();
                for (int i = 0; i < ar.Count; i++)//go through obstacles
                {
                    obstacle o = ar[i];
                    if (o.stationary == false && o.boolDone == false) arM.Add(o);
                }
                for (int i = 0; i < arM.Count; i++)//go through obstacles
                    getIndividualIntersectionWithPath(currentP, i);//o.fixedrobotposition=intersectedpoint

                arS = (from fi in arM   //arS=sorted obstacle array
                       where fi.intersected
                       orderby fi.robotPosition ascending
                       select fi).ToList();

                if (arS.Count > 0)
                    setIndividualSmallStep(currentP);
            }
            catch (Exception) 
            { 
            }
            return arS;
        }
        #endregion
        #region getIndividualIntersectionWithPath
        /// <summary>
        /// Gets individual intersection with the obstacle on the path
        /// </summary>
        /// <param name="currentP">Current position</param>
        /// <param name="i">Obstacle index</param>
        private void getIndividualIntersectionWithPath(int currentP, int i)
        {
            arIntersectedPoints.Clear();
            obstacle o = arM[i];
            double vlx = o.xm - o.xm1;
            double vly = o.ym - o.ym1;
            double vl = f.L(vlx, vly, 0);
            for (int k = currentP; k < (mf.pf2D.arPathPoints.Count - smallStep); k += smallStep) 
            {
                boolIntersectedWithObs = false;
                boolIntersectedWithObsLine = false;
                points p = mf.pf2D.arPathPoints[k];
                double mlx = p.xf - o.xm; double mly = p.yf - o.ym;
                double mlvl = vm(vlx, vly, mlx, mly);
                double ml = Math.Sqrt(mlx * mlx + mly * mly);
                double ro = Math.Abs(mlvl) / vl; // shortest distance from an obstacleBase point to the link

                // IntersectionOnObsParametric
                for (int c = 0; c < ar.Count; c++)
                {
                    obstacle o2 = ar[c];
                    if (o2.stationary)
                    {               
                        intersectionOfTwoLines(o2.x2, 0, o2.y2, -o2.h, o.xm, p.xf - o.xm, o.ym, p.yf - o.ym);
                        if (boolIntersectedWithObs) break;
                        intersectionOfTwoLines(o2.x1, 0, o2.y2, -o2.h, o.xm, p.xf - o.xm, o.ym, p.yf - o.ym);
                        if (boolIntersectedWithObs) break;
                        intersectionOfTwoLines(o2.x1, o2.w, o2.y1, 0, o.xm, p.xf - o.xm, o.ym, p.yf - o.ym);
                        if (boolIntersectedWithObs) break;
                        intersectionOfTwoLines(o2.x1, o2.w, o2.y2, 0, o.xm, p.xf - o.xm, o.ym, p.yf - o.ym);
                    }
                }

                if (ro < 1 && !boolIntersectedWithObs)
                {
                    arIntersectedPoints.Add(k);
                }
            }
            try
            {
                points pIntersectedFirst = mf.pf2D.arPathPoints[arIntersectedPoints[0]];
                points pIntersectedLast = mf.pf2D.arPathPoints[arIntersectedPoints[arIntersectedPoints.Count - 1]];
                double beamLengthFirst = Math.Abs(o.xm - pIntersectedFirst.xf);
                double beamLengthLast = Math.Abs(o.xm - pIntersectedLast.xf);
                if (beamLengthLast < beamLengthFirst)
                {
                    points p = pIntersectedLast;
                    int k = arIntersectedPoints[arIntersectedPoints.Count - 1];
                    GetIntersectionPoint(o, p, k, i);
                }
                else
                {
                    points p = pIntersectedFirst;
                    int k = arIntersectedPoints[0];
                    GetIntersectionPoint(o, p, k, i);
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion
        #region intersectionOfTwoLines
        double t1 = 1, t2 = 1;

        /// <summary>
        /// Checks whether two lines are intersected or not
        /// </summary>
        /// <param name="xa">x coordinate of the starting point on the first line</param>
        /// <param name="xb">difference between x coordinates of the first line</param>
        /// <param name="ya">y coordinate of the starting point on the first line</param>
        /// <param name="yb">difference between y coordinates of the first line</param>
        /// <param name="xc">x coordinate of the starting point on the second line</param>
        /// <param name="xd">difference between x coordinates of the second line</param>
        /// <param name="yc">y coordinate of the starting point on the second line</param>
        /// <param name="yd">difference between y coordinates of the second line</param>
        private void intersectionOfTwoLines(double xa, double xb, double ya, double yb, double xc, double xd, double yc, double yd)
        {
            // x1 = xa + t1 * xb
            //y1 = ya + t1 * yb

            //x2 = xc + t2 * xd
            //y2 = yc + t2 * yd


            //if ((xb * yd - yb * xd) == 0.0 || (xb * yd - yb * xd) == 0.0)
            //    mf.Text = "Lines are parallel!";

            t1 = -(-ya * xd + yc * xd + yd * xa - yd * xc) / (xb * yd - yb * xd);
            t2 = (-xa * yb + xb * ya - xb * yc + xc * yb) / (xb * yd - yb * xd);

            if (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1)
            {
                boolIntersectedWithObsLine = true;
                boolIntersectedWithObs = true;
            }
            else
            {
                boolIntersectedWithObsLine = false;
            }
        } 
        #endregion
        #region getIntersectionWithPathForInitialCalculations
        /// <summary>
        /// Gets intersection point with the obstacle on the path 
        /// </summary>
        /// <returns>arS</returns>
        public List<obstacle> getIntersectionWithPathForInitialCalculations() 
        {//getIntersectionWithPath
            try
            {
                arIntersectedPoints.Clear();
                arM.Clear();
                for (int i = 0; i < ar.Count; i++)//go through obstacles
                {
                    obstacle o = ar[i];
                    if (o.stationary == false && o.boolDone == false) arM.Add(o);
                }
                for (int i = 0; i < arM.Count; i++)//go through obstacles
                {

                    obstacle o = arM[i];
                    double vlx = o.xm - o.xm1;
                    double vly = o.ym - o.ym1;
                    double vl = f.L(vlx, vly, 0);

                    for (int k = 0; k < (mf.pf2D.arPathPoints.Count - smallStep); k += smallStep) 
                    {
                        boolIntersectedWithObs = false;
                        points p = mf.pf2D.arPathPoints[k];
                        double mlx = p.xf - o.xm; double mly = p.yf - o.ym;
                        double mlvl = vm(vlx, vly, mlx, mly);
                        double ml = Math.Sqrt(mlx * mlx + mly * mly);
                        double ro = Math.Abs(mlvl) / vl; //shortest distance from an obstacleBase point to the path

                        // IntersectionOnObs
                        for (int c = 0; c < ar.Count; c++)
                        {
                            obstacle o2 = ar[c];
                            double t = 0;
                            for (int j = 0; j < 50; j++)
                            {
                                t += 0.02;
                                double BeamX = o.xm + t * (p.xf - o.xm);
                                double BeamY = o.ym + t * (p.yf - o.ym);
                                if (o2.stationary)
                                {
                                    if (BeamX >= o2.x1 && BeamX <= o2.x2 && BeamY >= o2.y1 && BeamY <= o2.y2)
                                    {
                                        boolIntersectedWithObs = true;
                                    }
                                }
                            }
                        }
                        //

                        if (ro < 1 && !boolIntersectedWithObs)
                        {
                            arIntersectedPoints.Add(k);
                        }
                    }

                    try
                    {
                        points pIntersectedFirst = mf.pf2D.arPathPoints[arIntersectedPoints[0]];
                        points pIntersectedLast = mf.pf2D.arPathPoints[arIntersectedPoints[arIntersectedPoints.Count - 1]];
                        double beamLengthFirst = Math.Abs(o.xm - pIntersectedFirst.xf);
                        double beamLengthLast = Math.Abs(o.xm - pIntersectedLast.xf);
                        if (beamLengthLast < beamLengthFirst)
                        {
                            points p = pIntersectedLast;
                            int k = arIntersectedPoints[arIntersectedPoints.Count - 1];
                            GetIntersectionPoint(o, p, k, i);
                        }

                        else
                        {
                            points p = pIntersectedFirst;
                            int k = arIntersectedPoints[0];
                            GetIntersectionPoint(o, p, k, i);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                arS = (from fi in arM   //arS=sorted obstacle array
                       where fi.intersected
                       orderby fi.fixedRobotPosition ascending
                       select fi).ToList();
            }
            catch (Exception ex) { f.m("esc====\n" + ex.ToString()); }
            return arS;
        }

        public void GetIntersectionPoint(obstacle o, points p, int k, int i)
        {
            o.xf = p.xf; o.yf = p.yf; //intersection point coor
            //o.fixedRobotPosition = k;
            o.robotPosition = k;

            double alfa = Math.Atan2(o.yf - o.ym, o.xf - o.xm);
            if (alfa < 0) alfa = 2d * Math.PI + alfa;

            double diff = o.directionAngleRad - alfa;

            if (Math.Abs(diff) < 0.2)
            {
                o.intersected = true;
                double len = f.L(o.xm - o.xf, o.ym - o.yf);
                o.obsPosition = f.ig(len / mf.lineLength);

                points p1 = mf.pf2D.arPathPoints[k + 1];
                double len1 = f.L(o.xm - p1.xf, o.ym - p1.yf);
                double len2 = f.L(o.xf - p1.xf, o.yf - p1.yf);

                double tang = Math.Acos((len * len + len2 * len2 - len1 * len1) / (2d * len * len2));

                if (tang < 0.3)
                    ar[i].stationary = true;
                o.robotSpeed = mf.refSpeed; // f.ig(Math.Round(vall));
                if (o.robotSpeed == 0) o.robotSpeed = 1;
            }
            else
                o.intersected = false;
            //    string s11 = string.Format("ro={0:f3} dif={1:f3}  k={2:f3}", ro, diff, k);	//    f.m(s11);
            //break;
        }
        #endregion
        #region setIndividualSmallStep
        /// <summary>
        /// Sets individual small step according to current position
        /// </summary>
        /// <param name="currentRobotPosition">Current position</param>
        private void setIndividualSmallStep(int currentRobotPosition)
        {
            int robotPosition = 0;
            obstacle o = arS[0];
            double len = f.L(o.xm - o.xf, o.ym - o.yf);
            o.obsPosition = f.ig(len / mf.lineLength);
            robotPosition = o.robotPosition - currentRobotPosition;
            double vallCritical = (f.db(robotPosition) / (o.obsPosition)) * (o.obstacleSpeed);
            double vall = (f.db(robotPosition + mf.safetyMargin) / (o.obsPosition)) * (o.obstacleSpeed);
            o.robotSpeed = f.ig(Math.Round(vall));
            mf.criticalRobotSpeed = f.ig(Math.Round(vallCritical));
            if (mf.criticalRobotSpeed <= 0) mf.criticalRobotSpeed = 1;
            o.CriticalRobotSpeed = mf.criticalRobotSpeed;
            if (o.robotSpeed <= 0) o.robotSpeed = 1;
        }
        #endregion
        #region getObstacleNormalToPath
        /// <summary>
        /// Gets obstacles normal to the path
        /// </summary>
        public void getObstacleNormalToPath() //s normal
        {
            for (int i = 0; i < ar.Count; i++)
            {
                obstacle o = ar[i];
                double L = 0, L1 = 0;
                if (!o.stationary)
                {
                    for (int k = 0; k < mf.pf2D.arPathPoints.Count - 10; k += 10)
                    {
                        points p = mf.pf2D.arPathPoints[k];
                        points p1 = mf.pf2D.arPathPoints[k + 10];
                        L = f.L(p.xf - o.xm, p.yf - o.ym);
                        L1 = f.L(p1.xf - o.xm, p1.yf - o.ym);
                        if (L1 > L)
                        {
                            o.shortestPointToObs = k;
                            break;
                        }
                    }
                }
            }
            arS = (from fi in ar
                   orderby fi.robotPosition ascending
                   select fi).ToList();
        }
        #endregion
        #region chooseRec
        /// <summary>
        /// Chooses rectangle shaped obstacle
        /// </summary>
        /// <returns>Obstacle type</returns>
        public obstacleType chooseRec()
        {
            ot = obstacleType.rec;
            return ot;
        }
        #endregion
        #region chooseMouse
        /// <summary>
        /// Enables choosing obstacle
        /// </summary>
        /// <returns>Obstacle type</returns>
        public obstacleType chooseMouse()
        {
            ot = obstacleType.mouse;
            return ot;
        }
        #endregion
        #region resetBoolDone
        /// <summary>
        /// Reset boolDone values for all obstacles
        /// </summary>
        public void resetBoolDone()
        {
            for (int i = 0; i < ar.Count; i++)//go through obstacles
            {
                obstacle o = ar[i];
                o.boolDone = false;
            }
        }
        #endregion
    }
    #region enums
    /// <summary>
    /// Obstacle type
    /// </summary>
    public enum obstacleType
    {
        rec,
        mouse,
    }
    /// <summary>
    /// Placeholder position
    /// </summary>
    public enum placeHolder
    {
        topLeft,
        bottomLeft,
        leftMiddle, rightMiddle,
        topRight,
        bottomRight,
        empty
    }
    #endregion
}