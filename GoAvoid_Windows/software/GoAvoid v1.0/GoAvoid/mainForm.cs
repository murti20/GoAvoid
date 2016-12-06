
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using usefulFunctions;
using System.Linq;


namespace GoAvoid
{
    public partial class mainForm : Form
    {
        #region var
        int r1 = 10, kat1 = 7, b1 = 230, exp1 = 25;
        int delay = 10, robotSpeed = 40, majorTick = 1000, ne = 0, numberOfIterations = 200;
        public int refSpeed = 50, safetyMargin = 500;
        public int robotPosition = 0, refSpeedM = 0, obsPathPointInc = 1, criticalRobotSpeed = 0;
        double logbase = 2.1;
        public double lineLength = 0.1d, filterV = 80d;
        public double eX, eY, coorX, coorY, newX, newY;
        public double startX = 200d, startY = 200, goalX = 1400d, goalY = 600d;
        public double mw = 1.0f, ofx = 500.0f, ofy = -1020.0f, offsetOldX, offsetOldY, sf = 0.76f, offsetXBase = 500f, offsetYBase = -1020f;
        double mwDefault = 1, ofxDefault = 140, ofyDefault = -1020;
        double wsWidthTemp, wsHeightTemp;
        public string s1 = "", sRobotSpeed = "", sRelTimeRobot = "";
        public String fileNameN;
        public bool boolFixedSize = true, boolStopObstacle = true, boolBreakWhile = false, boolSelectRef=true, boolShowBalloonTips = false;
        public bool boolDrawBitmapField, boolDrawBitmapField1 = false, boolActivateDelay = false;
        public bool mouseWheelSwitch = true;
        public bool setGoal, setStartPointOfPath, drawObstacles, pan;
        public bool saved = false, drawGrid;
        public bool boolDrawEnclosingCircle = false;
        bool[,] or; bool[,] or1;
        public bool stopMobileRobot = false;
        public bool boolSeekForPointCoor = false;
        public bool boolUpdateInf = true;
        public bool boolChangeColor = false;
        public bool boolChangeSafetyMarginColor = false;
        public bool boolChangeObsPathColor = false;
        public bool boolIsRunning = false;
        bool boolChangeWorkspaceDimension = true;
        bool boolDrawTemporaryWorkspace = false;
        bool boolChangeWorkspaceSetColor = false;
        bool boolIsOutsideOfTheWorkspace = false;
        List<obstacle> arS = null;
        List<obstacle> arS1 = null;
        List<int> arTimePassed = new List<int>();
        public Stopwatch swCalculationTime = new Stopwatch();
        public Stopwatch swRunTime = new Stopwatch();
        public mobileRobot mr = new mobileRobot();
        public frameFormBitmap frameForm1;
        public obstacleBase ob;
        public pField2D pf2D = new pField2D();
        public long elapsedTimeToGoal = 0, elapsedTimeToCalculate = 0;
        Bitmap b = new Bitmap(1600, 900, PixelFormat.Format24bppRgb);
        drawbitmap db;
        Color workspaceColor = Color.Azure;
        Graphics g;
        Font fo = new System.Drawing.Font("Arial", 10), fo1 = new System.Drawing.Font("Arial", 15);
        Pen pc = new Pen(Color.Brown, 3);
        public string currentWorkspace = "", currentCalculationParameters = "";
        public bool boolDirty=false;
        #endregion
        #region initialize component
        public mainForm()  { InitializeComponent(); }
        #endregion 
        #region load
        private void mainForm_Load(object sender, EventArgs e)
        {
            ob = new obstacleBase(this);
            pf2D.mf = this;
            if (drawGrid) gridOnToolStripMenuItem.Checked = true; else gridOnToolStripMenuItem.Checked = false;
            if (ob.isSnapToGrid) snapToGridToolStripMenuItem.Checked = true; else snapToGridToolStripMenuItem.Checked = false;
            g = CreateGraphics(); g.SmoothingMode = SmoothingMode.AntiAlias; scaleT(g);
            or = new bool[ob.numberOfGridLinesX + 1, ob.numberOfGridLinesY + 1];
            or1 = new bool[ob.numberOfGridLinesX + 1, ob.numberOfGridLinesY + 1];
            ob.getObstaclesForPF();
            pc.EndCap = LineCap.ArrowAnchor;
            db = new drawbitmap(b);
            chooseMouse();
            drawObstacles = true;
            hScrollBarLineLength.Value = (int)(100d * lineLength);
            labelLineLength.Text = string.Format("{0:f2}", lineLength);
            labelDelay.Text = delay.ToString();
            if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
            hScrollBarMajorTick.Value = majorTick;
            labelMajorTick.Text = majorTick.ToString();
            hScrollBarRefSpeed.Value = refSpeed;
            labelRefSpeed.Text = string.Format("{0}", refSpeed);
            labelFilter.Text = string.Format("{0}", filterV);
            hScrollBarFilter.Value =Convert.ToInt32( filterV);
            textBoxSafetyMargin.Text = safetyMargin.ToString();
            ob.gridSize =f.ig(textBoxGridSize.Text);
            numberOfIterations = f.ig(textBoxNumberOfIterations.Text);
            currentWorkspace = string.Format("Current={0}x{1}", ob.wsWidth, ob.wsHeight);
            currentCalculationParameters = string.Format("Current={0}x{1}", ob.gridSize, numberOfIterations);
            labelCurrentWorkspace.Text = currentWorkspace;
            labelCurrentCalculationParameters.Text = currentCalculationParameters;
        }
        #endregion
        #region paint
        private void mainForm_Paint(object sender, PaintEventArgs e)// paint
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            scaleT(e.Graphics);
            if (boolDrawBitmapField) e.Graphics.DrawImage(b, 0, 0);
            e.Graphics.DrawLine(f.pGr(0), 0, 0, f.c(ob.wsWidth), 0);
            e.Graphics.DrawLine(f.pGr(0), 0, 0, 0, f.c(ob.wsHeight));
            e.Graphics.DrawLine(f.pGr(0), 0, f.c(ob.wsHeight), f.c(ob.wsWidth), f.c(ob.wsHeight));  
            e.Graphics.DrawLine(f.pGr(0), f.c(ob.wsWidth), 0, f.c(ob.wsWidth), f.c(ob.wsHeight));  

            if (boolDrawTemporaryWorkspace)
            {
                e.Graphics.DrawLine(f.pR(0), 0, 0, f.c(wsWidthTemp), 0);
                e.Graphics.DrawLine(f.pR(0), 0, 0, 0, f.c(wsHeightTemp));
                e.Graphics.DrawLine(f.pR(0), 0, f.c(wsHeightTemp), f.c(wsWidthTemp), f.c(wsHeightTemp));  
                e.Graphics.DrawLine(f.pR(0), f.c(wsWidthTemp), 0, f.c(wsWidthTemp), f.c(wsHeightTemp));  
            }

            Pen p1 = new Pen(Color.Green, 6);
            p1.EndCap = LineCap.Custom;
            p1.CustomEndCap = new AdjustableArrowCap(3.5f, 3.7f, true);
            e.Graphics.DrawLine(p1, 0, 0, f.c(ob.wsWidth / 8d), 0); e.Graphics.DrawLine(p1, 0, 0, 0, f.c(ob.wsHeight / 8d));      
            drawGoal(e); drawStartPointOfPath(e);
            if (drawGrid) { ob.drawGrid(e); ob.drawObstaclePointsOnGrid(e); }
            frameForm1.coorInframeForm(eX, eY);

            pf2D.drawPFTrace(e, robotSpeed, false);
            pf2D.drawPFLargeTrace(e, majorTick);
            ob.paint(e);

            mr.drawMobileRobot(e, Color.Black, 4, ar.Count);

            if (boolDrawObsPathPoint) ob.obsPathDraw(e);

            for (int i = 0; i < arP.Count; i++) //paint
            {
                points p = arP[i];
                f.dC(e.Graphics, f.pBr(30), p.x, p.y, 30);
            }
            drawString(e);
        }
        #endregion
        #region draw start goal
        /// <summary>
        /// Draws goal point 
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        private void drawGoal(PaintEventArgs e)
        {
            double r = 12d;//dimension of the goal
            Pen pG = new Pen(Color.Black, 2);
            Rectangle drawArea1 = new Rectangle(f.ig(goalX - r), f.ig(goalY - r), f.ig(2d * r), f.ig(2d * r));
            e.Graphics.DrawEllipse(pG, f.ig(goalX - r), f.ig(goalY - r), f.ig(2d * r), f.ig(2d * r));
            LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 34, 200, 34), Color.FromArgb(255, 0,200,0), LinearGradientMode.Vertical);
            e.Graphics.FillEllipse(lb, f.ig(goalX - r), f.ig(goalY - r), f.ig(2d * r), f.ig(2d * r));
        }
        /// <summary>
        /// Draws start point of path
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        private void drawStartPointOfPath(PaintEventArgs e)
        {
            double r = 12d;//dimension of the start
            Pen pG = new Pen(Color.Black, 2);
            Rectangle drawArea1 = new Rectangle(f.ig(startX - r), f.ig(startY - r), f.ig(2d * r), f.ig(2d * r));
            LinearGradientBrush lb = new LinearGradientBrush(drawArea1, Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 255, 255,255), LinearGradientMode.Vertical);
            e.Graphics.FillEllipse(lb, Convert.ToSingle(startX - r), Convert.ToSingle(startY - r), f.si(2d * r), f.si(2d * r));
            e.Graphics.DrawEllipse(pG, Convert.ToSingle(startX - r), Convert.ToSingle(startY - r), f.si(2d * r), f.si(2d * r));
        }
        #endregion
        #region drawBitmapField
        /// <summary>
        /// Draws bitmap field
        /// </summary>
        private Bitmap drawBitmapField()
        {
            try
            {
                db.b1 = new Bitmap(f.ig(ob.wsWidth), f.ig(ob.wsHeight), PixelFormat.Format24bppRgb);
                db.bytes = db.b1.Width * db.b1.Height * 3;  // Declare an array to hold the bytes of the bitmap. This code is specific to a bitmap with 24 bits per pixels.
                db.rgbValues = new byte[db.bytes];

                int logar = 0;
                double cor1;
                double correct = -Math.Exp(Convert.ToDouble(exp1)) / Math.Pow(2.0, 124.0);
                db.lockbits(false);
                int xlim = db.b1.Width, ylim = db.b1.Height;
                unsafe
                {
                    for (int j = 0; j < ylim; j++)
                    {
                        for (int i = 0; i < xlim; i++)
                        {
                            cor1 = Math.Log(correct * pf2D.field(Convert.ToDouble(i), Convert.ToDouble(j)), logbase);
                            if (cor1 >= 0.0) logar = Convert.ToInt32(cor1);
                            if (logar < 0) logar = Math.Abs(logar);
                            if (logar > 255) logar = 255;
                            int g = kat1 * logar;
                            if (g > 254) g = 255;
                            db.setpixel(r1, g, b1);
                            db.ptr += 3;
                        }
                        db.ptr += db.remain;
                    }
                }
                db.unlockbits();       
            }
            catch (Exception)
            {
                MessageBoxDetailed("Out of memory, please increase the grid size or reduce workspace dimensions then try again", "Draw Bitmap Field Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                boolDrawBitmapField = false;
                frameForm1.toolStripButtonDrawBitmapField.Checked = false;
                frameForm1.toolStripButtonDrawBitmapField.Text = "bitmap off";
                BitmapFieldtoolStripMenuItem.Checked = false;
            }
            return db.b1;
        }
        #endregion
        #region scaleTransform
        /// <summary>
        /// Enables zooming in and out
        /// </summary>
        /// <param name="g">Graphics g</param>
        private void scaleT(Graphics g)
        {
            g.ResetTransform();
            g.ScaleTransform(f.si(mw * sf), -f.si(mw * sf)); g.TranslateTransform(f.si(ofx), f.si(ofy));
        }

        #endregion
        #region save document
        /// <summary>
        /// Saves document as specified filename
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public void saveDoc(String fileName)// save
        {
            try
            {
                Stream s = File.Open(fileName, FileMode.Create);
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(s, ob.ar);
                b.Serialize(s, ob.arD);
                b.Serialize(s, ob.wsWidth);
                b.Serialize(s, ob.wsHeight);
                b.Serialize(s, drawGrid);
                b.Serialize(s, ob.gridSize);
                b.Serialize(s, ob.isSnapToGrid);
                b.Serialize(s, goalX); b.Serialize(s, goalY);
                b.Serialize(s, ofx); b.Serialize(s, ofy);
                b.Serialize(s, mw);
                b.Serialize(s, ofxDefault); b.Serialize(s, ofyDefault);
                b.Serialize(s, mwDefault);
                b.Serialize(s, or);
                b.Serialize(s, or1);
                b.Serialize(s, startX); b.Serialize(s, startY);
                b.Serialize(s, boolDrawBitmapField);
                b.Serialize(s, filterV);
                b.Serialize(s, majorTick);
                b.Serialize(s, boolDrawEnclosingCircle);
                b.Serialize(s, delay);
                b.Serialize(s, lineLength);
                b.Serialize(s, refSpeed);
                b.Serialize(s, safetyMargin);
                b.Serialize(s, boolShowBalloonTips);
                b.Serialize(s, boolFixedSize);
                b.Serialize(s, boolStopObstacle);
                b.Serialize(s, numberOfIterations);
                b.Serialize(s, boolSeekForPointCoor);
                b.Serialize(s, boolSelectRef);
                b.Serialize(s, boolDrawObsPathPoint);
                b.Serialize(s, obsPathPointInc);
                s.Close();
                boolDirty = false;
            }
            catch (Exception ex)
            {
                MessageBoxDetailed(ex.ToString(), "File Couldn't Be Saved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //MessageBoxDetailed("File couldn't be saved because of the serialization problem please check the savedoc() function","Save File Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region open document
        /// <summary>
        /// Opens document
        /// </summary>
        /// <param name="fileName">Name of the filename</param>
        public void openDoc(String fileName)// open
        {
            try
            {
                Stream s = File.Open(fileName, FileMode.Open);
                BinaryFormatter b = new BinaryFormatter();
                ob.ar = (List<obstacle>)(b.Deserialize(s));
                ob.arD = (List<obstacle>)(b.Deserialize(s));
                ob.wsWidth = Convert.ToInt32(b.Deserialize(s));
                ob.wsHeight = Convert.ToInt32(b.Deserialize(s));
                drawGrid = Convert.ToBoolean(b.Deserialize(s));
                ob.gridSize = Convert.ToSingle(b.Deserialize(s));
                ob.isSnapToGrid = Convert.ToBoolean(b.Deserialize(s));
                goalX = Convert.ToInt32(b.Deserialize(s));
                goalY = Convert.ToInt32(b.Deserialize(s));
                ofx = Convert.ToSingle(b.Deserialize(s));
                ofy = Convert.ToSingle(b.Deserialize(s));
                mw = Convert.ToSingle(b.Deserialize(s));
                ofxDefault = Convert.ToSingle(b.Deserialize(s));
                ofyDefault = Convert.ToSingle(b.Deserialize(s));
                mwDefault = Convert.ToSingle(b.Deserialize(s));
                or = (bool[,])(b.Deserialize(s));
                or1 = (bool[,])(b.Deserialize(s));
                startX = Convert.ToInt32(b.Deserialize(s));  
                startY = Convert.ToInt32(b.Deserialize(s));  
                boolDrawBitmapField = Convert.ToBoolean(b.Deserialize(s));
                filterV = Convert.ToDouble(b.Deserialize(s));
                majorTick = Convert.ToInt32(b.Deserialize(s));
                boolDrawEnclosingCircle = Convert.ToBoolean(b.Deserialize(s));
                delay = Convert.ToInt32(b.Deserialize(s));
                lineLength = Convert.ToDouble(b.Deserialize(s));
                refSpeed = Convert.ToInt32(b.Deserialize(s));
                safetyMargin = Convert.ToInt32(b.Deserialize(s));
                boolShowBalloonTips = Convert.ToBoolean(b.Deserialize(s));
                boolFixedSize = Convert.ToBoolean(b.Deserialize(s));
                boolStopObstacle = Convert.ToBoolean(b.Deserialize(s));
                numberOfIterations = Convert.ToInt32(b.Deserialize(s));
                boolSeekForPointCoor = Convert.ToBoolean(b.Deserialize(s));
                boolSelectRef = Convert.ToBoolean(b.Deserialize(s));
                boolDrawObsPathPoint = Convert.ToBoolean(b.Deserialize(s));
                obsPathPointInc = Convert.ToInt32(b.Deserialize(s));
                
                updateRefSpeed();

                if (boolSeekForPointCoor) checkBoxSeekingForPathPoint.Checked = true; else checkBoxSeekingForPathPoint.Checked = false;
                if (boolSelectRef) checkBoxRefSpeed.Checked = true; else checkBoxRefSpeed.Checked = false;
                if (boolDrawEnclosingCircle) checkBoxDrawEnclosingCircle.Checked = true; else checkBoxDrawEnclosingCircle.Checked = false;
                if (boolDrawObsPathPoint) checkBoxDrawObsPathPoint.Checked = true; else checkBoxDrawObsPathPoint.Checked = false;
                if (boolShowBalloonTips) showBalloonTipsToolStripMenuItem.Checked = true; else showBalloonTipsToolStripMenuItem.Checked = false;
                frameForm1.boolShowBalloonTips = boolShowBalloonTips;

                chooseMouse();

                if (drawGrid)
                {
                    gridOnToolStripMenuItem.Checked = true;
                    frameForm1.toolStripButtonDrawGrid.Checked = true;
                    frameForm1.toolStripButtonDrawGrid.Text = "grid on";
                }
                else
                {
                    gridOnToolStripMenuItem.Checked = false;
                    frameForm1.toolStripButtonDrawGrid.Checked = false;
                    frameForm1.toolStripButtonDrawGrid.Text = "grid off";
                }
                if (ob.isSnapToGrid)
                {
                    snapToGridToolStripMenuItem.Checked = true;
                    frameForm1.toolStripButtonSnapToGrid.Checked = true;
                    frameForm1.toolStripButtonSnapToGrid.Text = "snap to grid on";
                }

                else
                {
                    snapToGridToolStripMenuItem.Checked = false;
                    frameForm1.toolStripButtonSnapToGrid.Checked = false;
                    frameForm1.toolStripButtonSnapToGrid.Text = "snap to grid off";
                }

                offsetXBase = ofx * mw; offsetYBase = ofy * mw; 
                hScrollBarFilter.Value = f.ig(filterV);
                labelFilter.Text = filterV.ToString();
                hScrollBarMajorTick.Value = majorTick;
                labelMajorTick.Text = majorTick.ToString();
                hScrollBarDelay.Value = delay; labelDelay.Text = delay.ToString();
                hScrollBarLineLength.Value = f.ig(lineLength * 100d); labelLineLength.Text = lineLength.ToString("f3");
                hScrollBarRefSpeed.Value = refSpeed; labelRefSpeed.Text = refSpeed.ToString();
                textBoxSafetyMargin.Text = safetyMargin.ToString();
                textBoxObsPath.Text = obsPathPointInc.ToString();

                textBoxWorkspaceWidth.Text = ob.wsWidth.ToString();
                textBoxWorkspaceHeight.Text = ob.wsHeight.ToString();
                textBoxGridSize.Text = ob.gridSize.ToString();
                textBoxNumberOfIterations.Text = numberOfIterations.ToString();
                currentWorkspace = string.Format("Current={0}x{1}", ob.wsWidth, ob.wsHeight);
                labelCurrentWorkspace.Text = currentWorkspace;
                currentCalculationParameters = string.Format("Current={0}x{1}", ob.gridSize, numberOfIterations);
                labelCurrentCalculationParameters.Text = currentCalculationParameters;

                if (boolDrawBitmapField)
                {
                    frameForm1.toolStripButtonDrawBitmapField.Checked = true;
                    frameForm1.toolStripButtonDrawBitmapField.Text = "bitmap on";
                    BitmapFieldtoolStripMenuItem.Checked = true;    
               
                }
                else
                {
                    frameForm1.toolStripButtonDrawBitmapField.Checked = false;
                    frameForm1.toolStripButtonDrawBitmapField.Text = "bitmap off";
                    BitmapFieldtoolStripMenuItem.Checked = false;
                }
                calculateFieldToDrawBitmapField();

                if (boolFixedSize)
                {
                    frameForm1.toolStripButtonFixedSize.Checked = true;
                    fixedSizeObstacleToolStripMenuItem.Checked = true;
                }
                else
                {
                    frameForm1.toolStripButtonFixedSize.Checked = false;
                    fixedSizeObstacleToolStripMenuItem.Checked = false;
                }
                if (boolStopObstacle)
                {
                    frameForm1.toolStripButtonStopObstacle.Checked = true;
                    stoppedObstacleToolStripMenuItem.Checked = true;
                }
                else
                {
                    frameForm1.toolStripButtonStopObstacle.Checked = false;
                    stoppedObstacleToolStripMenuItem.Checked = false;
                }

                Invalidate();

                s.Close();
                boolDirty = false;
            }
            catch (Exception)
            {
                if (frameForm1.boolFirstTime)
                {
                    saved = false;
                    frameForm1.boolFirstTime = false;
                    fileName = "";
                    fileNameN = "";
                    this.Text = "";
                }
                else
                {
                    MessageBoxDetailed("File Couldn't Be Opened", "File Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //MessageBoxDetailed("File couldn't be opened because of the deserialization problem please check the opendoc() function", "Open File Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }            
            }
        }
        #endregion
        #region mousedown
        private void mainForm_MouseDown(object sender, MouseEventArgs e)
        {  
            getTransCoor(e);
            offsetOldX = eX; offsetOldY = eY;    // for "pan"
            if (e.Button == MouseButtons.Middle)
            {
                panToolStripMenuItem.PerformClick();
            }

            if (drawObstacles&&!pan)
            {
                ob.mouseDown(e, eX, eY);
                if (e.Button == MouseButtons.Left && ob.chosenShape > -1 && !boolIsRunning)
                {
                    groupBoxObstacleMotion.Visible = true;
                    groupBoxObstacleMotion.Location = new Point(e.X + 40, e.Y);
                }
                if (e.Button == MouseButtons.Left && ob.chosenShape == -1)
                {
                    groupBoxObstacleMotion.Visible = false;
                    groupBoxObstacleMotion.Location = new Point(557, -4);
                }
            }
        }
        #endregion
        #region mousemove
        private void mainForm_MouseMove(object sender, MouseEventArgs e)
        {
            getTransCoor(e);

            if (boolSeekForPointCoor && e.Button == MouseButtons.Middle)
            {
                for (int k = 0; k < (pf2D.arPathPoints.Count); k += 10) 
                {
                    points p = pf2D.arPathPoints[k];
                    if (f.L(p.xf - eX, p.yf - eY) < 5) labelPointCoor.Text = string.Format("{0}", k);
                }
            }
            if (drawObstacles&&!pan)
            {
                ob.mouseMove(e, eX, eY);
            }

            coorX = eX; coorY = eY; newX = eX; newY = eY;

            // start goal outside

            if (e.Button == MouseButtons.Left && setGoal && !boolIsRunning)
            {
                ob.CheckCursorIsOutside(ref eX, ref eY);

                if (eX >= ob.gridSize && eX <= ob.wsWidth - ob.gridSize) goalX = f.ig(f.ig(eX / ob.gridSize) * ob.gridSize);
                else if (eX > ob.wsWidth - ob.gridSize) goalX = ob.wsWidth - ob.gridSize;
                else if (eX < ob.gridSize) goalX = ob.gridSize;

                if (eY >= ob.gridSize && eY <= ob.wsHeight - ob.gridSize) goalY = f.ig(f.ig(eY / ob.gridSize) * ob.gridSize);
                else if (eY > ob.wsHeight - ob.gridSize) goalY = ob.wsHeight - ob.gridSize;
                else if (eY < ob.gridSize) goalY = ob.gridSize;
            }

            if (e.Button == MouseButtons.Left && setStartPointOfPath && !boolIsRunning)
            {
                ob.CheckCursorIsOutside(ref eX, ref eY);

                if (eX >= ob.gridSize && eX<=ob.wsWidth-ob.gridSize) startX = f.ig(f.ig(eX / ob.gridSize) * ob.gridSize);
                else if(eX>ob.wsWidth-ob.gridSize) startX = ob.wsWidth - ob.gridSize;
                else if(eX<ob.gridSize) startX = ob.gridSize;

                if (eY >= ob.gridSize && eY <= ob.wsHeight - ob.gridSize) startY = f.ig(f.ig(eY / ob.gridSize) * ob.gridSize);
                else if (eY > ob.wsHeight - ob.gridSize) startY = ob.wsHeight - ob.gridSize;
                else if (eY < ob.gridSize) startY = ob.gridSize;
            }

            if ((e.Button == MouseButtons.Left && pan))
            {
                ofx = ((double)e.X) / (mw * sf); ofy = -((double)e.Y) / (mw * sf);
                ofx -= offsetOldX; ofy -= offsetOldY;
                offsetXBase = (ofx) * mw; offsetYBase = ofy * mw;
            }
            Invalidate();
        }
        #endregion
        #region mouseup
        private void mainForm_MouseUp(object sender, MouseEventArgs e)
        {
            eX = (int)((float)e.X * (1.0 / (mw * sf)) - ofx); eY = -(int)((float)e.Y * (1.0 / (mw * sf)) + ofy);

            if (drawObstacles&&!pan)
            {
                boolDirty = true;
                if (frameForm1.toolStripButtonFixedSize.Checked) boolFixedSize = true;
                else boolFixedSize = false;
                ob.mouseUp(e, eX, eY, boolFixedSize, lineLength);
            }

            if (e.Button == MouseButtons.Left && ob.chosenShape > -1&&!pan) {
                boolDirty = true;
                groupBoxObstacleMotion.Location = new Point(e.X + 80, e.Y); 
                groupBoxObstacleMotion.Update(); 
            }
            setGoal = false; setStartPointOfPath = false; drawObstacles = true;
        }
        #endregion
        #region mousewheel
        double whX = 0, whY = 0; float vall = 0.2f;
        private void mainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            whX = e.X;
            whY = e.Y;
            getTransCoor(e);

            if (mouseWheelSwitch)
            {
                if (e.Delta > 0.0f) mw += vall;
                else mw -= vall;
                if (mw <= 0.1f) mw = 0.1f;
                ofx = offsetXBase / mw;
                ofy = offsetYBase / mw;
                Invalidate();
            }
        }
        #endregion
        #region mouse doubleclick
        private void mainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)// defaultview
            {
                ofx = ofxDefault;
                ofy = ofyDefault;
                mw = mwDefault;
                offsetXBase = (ofx) * mw; offsetYBase = ofy * mw; 
                Invalidate();
            }
        }
        #endregion
        #region key down
        private void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (ob.ar.Count > 0 && ob.chosenShape > -1 && ob.chosenShape <= (ob.ar.Count - 1))
                {
                    ob.ar.RemoveAt(ob.chosenShape);
                    ob.arD.RemoveAt(ob.chosenShape);
                    ob.getObstaclesForPF();
                    Invalidate();
                    groupBoxObstacleMotion.Visible = false;
                }
            }
        }
        #endregion
        #region hScrollBarObstacleDirection_Scroll
        private void hScrollBarObstacleDirection_Scroll(object sender, ScrollEventArgs e)
        {
            if (ob.ar.Count > 0)
            {
                textBoxObstacleDirection.Text = hScrollBarObstacleDirection.Value.ToString();
                textBoxObstacleSpeed.Text = hScrollBarObstacleSpeed.Value.ToString();
                ob.getDirectionAndSpeed(ob.chosenShape, f.db(hScrollBarObstacleDirection.Value), f.db(hScrollBarObstacleSpeed.Value), lineLength);
                ob.saveInitialObstacleValue(ob.chosenShape, ob.ar[ob.chosenShape]);
                Invalidate();
            }
        }
        #endregion
        #region hScrollBarObstacleSpeed_Scroll
        private void hScrollBarObstacleSpeed_Scroll(object sender, ScrollEventArgs e)
        {
            if (ob.ar.Count > 0)
            {
                textBoxObstacleDirection.Text = hScrollBarObstacleDirection.Value.ToString();
                textBoxObstacleSpeed.Text = hScrollBarObstacleSpeed.Value.ToString();
                ob.getDirectionAndSpeed(ob.chosenShape, f.db(hScrollBarObstacleDirection.Value), f.db(hScrollBarObstacleSpeed.Value), lineLength);
                ob.saveInitialObstacleValue(ob.chosenShape, ob.ar[ob.chosenShape]);
                Invalidate();
            }
        }
        #endregion
        #region hScrollBarDelay_Scroll
        private void hScrollBarDelay_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                delay = hScrollBarDelay.Value;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region hScrollBarRobotInc_Scroll
        private void hScrollBarRobotInc_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                if (!boolIsRunning)
                {
                    lineLength = (double)(hScrollBarLineLength.Value) / 100d;
                    labelLineLength.Text = string.Format("{0:f3}", lineLength);
                    initialize();
                    ob.setIncForAll(lineLength);
                    Invalidate();
                }
                //else MessageBoxDetailed("Animation is running", "Set Line Length Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception) { }
        }
        #endregion
        #region hScrollBarRefSpeed_Scroll
        private void hScrollBarRefSpeed_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                refSpeed = hScrollBarRefSpeed.Value;
                labelRefSpeed.Text = string.Format("{0}", refSpeed);
            }
            catch (Exception) { }
        }
        #endregion
        #region hScrollBarMajorTick_Scroll
        private void hScrollBarMajorTick_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                majorTick = e.NewValue;
                labelMajorTick.Text = majorTick.ToString();
                Invalidate();
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region hScrollBarFilter_Scroll
        private void hScrollBarFilter_Scroll(object sender, ScrollEventArgs e)
        {
            labelFilter.Text = hScrollBarFilter.Value.ToString();
            filterV = Convert.ToDouble(hScrollBarFilter.Value);
            boolDirty = true;
            //else MessageBoxDetailed("Animation is running", "Set Filter Value Problem",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        #endregion
        #region button1ms_Click
        private void button1ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 1;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button10ms_Click
        private void button10ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 10;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button20ms_Click
        private void button20ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 20;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button40ms_Click
        private void button40ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 40;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button80ms_Click
        private void button80ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 80;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button150ms_Click
        private void button150ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 150;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region button1000ms_Click
        private void button1000ms_Click(object sender, EventArgs e)
        {
            try
            {
                delay = 1000;
                hScrollBarDelay.Value = delay;
                labelDelay.Text = delay.ToString();
                if (delay != 0) boolActivateDelay = true; else boolActivateDelay = false;
                boolDirty = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region buttonStopMobileRobot_Click
        private void buttonStopMobileRobot_Click(object sender, EventArgs e)
        {
            stopMobileRobot = !stopMobileRobot;
            stopMobileRobotM();
        }
        #endregion
        #region buttonInitial_Click
        private void buttonInitial_Click(object sender, EventArgs e)
        {
            initialize();
        }
        #endregion
        #region buttonMobileRobotInitialPos_Click
        private void buttonMobileRobotInitialPos_Click(object sender, EventArgs e)
        {
            arP.Clear();
            swRunTime.Start();
            stopMobileRobot = false;
            groupBoxObstacleMotion.Visible = false;
            ob.getInitialObstacleValues();
            ob.updateObs();
            Invalidate();
        }
        #endregion
        #region buttonSafetyMarginDec_Click
        private void buttonSafetyMarginDec_Click(object sender, EventArgs e)
        {
            try
            {
                safetyMargin = f.ig(textBoxSafetyMargin.Text);
                textBoxSafetyMargin.Text = (safetyMargin -= 500).ToString();
                if (safetyMargin < 0) textBoxSafetyMargin.Text = "0";
                buttonSetSafetyMargin.BackColor = Color.Red;
            }
            catch (Exception) { }
        }
        #endregion
        #region buttonSafetyMarginInc_Click
        private void buttonSafetyMarginInc_Click(object sender, EventArgs e)
        {
            try
            {
                safetyMargin = f.ig(textBoxSafetyMargin.Text);
                textBoxSafetyMargin.Text = (safetyMargin += 500).ToString();
                buttonSetSafetyMargin.BackColor = Color.Red;
            }
            catch (Exception) { }
        }
        #endregion
        #region buttonSetSafetyMargin_Click
        int SafetyMarginBefore;
        private void buttonSetSafetyMargin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!boolIsRunning)
                {
                    if (safetyMargin >= 0) SafetyMarginBefore = safetyMargin;

                    safetyMargin = f.ig(textBoxSafetyMargin.Text);
                    if (safetyMargin < 0)
                    {
                        safetyMargin = SafetyMarginBefore;
                        textBoxSafetyMargin.Text = SafetyMarginBefore.ToString();
                        buttonSetSafetyMargin.BackColor = Color.Lavender;
                    }
                    else
                    {
                        buttonSetSafetyMargin.BackColor = Color.Lavender;
                    }
                    boolDirty = true;
                }
                else
                {
                    MessageBoxDetailed("Animation is running", "Set Overall Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    safetyMargin = SafetyMarginBefore;
                    textBoxSafetyMargin.Text = SafetyMarginBefore.ToString();
                    buttonSetSafetyMargin.BackColor = Color.Lavender;
                }

            }
            catch (Exception)
            {
                safetyMargin = SafetyMarginBefore;
                textBoxSafetyMargin.Text = SafetyMarginBefore.ToString();
                buttonSetSafetyMargin.BackColor = Color.Lavender;
            }
        }
        #endregion
        #region buttonSetObsPath_Click
        int obsPathPointIncBefore;
        private void buttonSetObsPath_Click(object sender, EventArgs e)
        {
            try
            {
                if (obsPathPointInc > 0) obsPathPointIncBefore = obsPathPointInc;

                obsPathPointInc = f.ig(textBoxObsPath.Text);
                if (obsPathPointInc < 1)
                {
                    obsPathPointInc = obsPathPointIncBefore;
                    textBoxObsPath.Text = obsPathPointIncBefore.ToString();
                    buttonSetObsPath.BackColor = Color.Lavender;
                }
                else
                {
                    buttonSetObsPath.BackColor = Color.Lavender;
                }
                boolDirty = true;
            }
            catch (Exception)
            {
                textBoxObsPath.Text = obsPathPointIncBefore.ToString();
                buttonSetObsPath.BackColor = Color.Lavender;
            }
        }
        #endregion
        #region buttonObsPathDec_Click
        private void buttonObsPathDec_Click(object sender, EventArgs e)
        {
            try
            {
                obsPathPointInc = f.ig(textBoxObsPath.Text);
                textBoxObsPath.Text = (obsPathPointInc -= 10).ToString();
                if (obsPathPointInc <= 10) textBoxObsPath.Text = "10";
                buttonSetObsPath.BackColor = Color.Red;
            }
            catch (Exception) { }
        }
        #endregion
        #region buttonObsPathInc_Click
        private void buttonObsPathInc_Click(object sender, EventArgs e)
        {
            try
            {
                obsPathPointInc = f.ig(textBoxObsPath.Text);
                textBoxObsPath.Text = (obsPathPointInc += 10).ToString();
                buttonSetObsPath.BackColor = Color.Red;
            }
            catch (Exception) { }
        }
        #endregion
        #region buttonWorkspaceSet_Click
        double gridSizeBefore = 20; 
        int numberOfIterationsBefore = 200;
        private void buttonWorkspaceSet_Click(object sender, EventArgs e)
        {
            try
            {
                #region setWorkspaceDimension
                if (!boolIsRunning)
                {
                    boolChangeWorkspaceDimension = true;
                    boolDrawTemporaryWorkspace = false;
                    try
                    {
                        wsWidthTemp = Convert.ToDouble(textBoxWorkspaceWidth.Text);
                        wsHeightTemp = Convert.ToDouble(textBoxWorkspaceHeight.Text);
                    }
                    catch (Exception)
                    {
                        textBoxWorkspaceWidth.Text = ob.wsWidth.ToString();
                        textBoxWorkspaceHeight.Text = ob.wsHeight.ToString();
                        buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                        return;
                    }
                    for (int i = 0; i < ob.arD.Count; i++)
                    {
                        obstacle o = ob.arD[i];
                        if (o.x1 < 0 || o.x1 > wsWidthTemp - Math.Abs(o.x2 - o.x1) || o.y1 < 0 || o.y1 > wsHeightTemp - Math.Abs(o.y2 - o.y1))
                        {
                            //   f.m("Obstacle Is Outside Please Move The Obstacle To The Red Region And Try Again");
                            boolChangeWorkspaceDimension = false;
                        }
                    }
                    if (startX < 0 || startX > wsWidthTemp || startY < 0 || startY > wsHeightTemp)
                    {
                        //f.m("Start Point Is Outside Please Move The Obstacle To The Red Region And Try Again");
                        boolChangeWorkspaceDimension = false;
                    }
                    if (goalX < 0 || goalX > wsWidthTemp || goalY < 0 || goalY > wsHeightTemp)
                    {
                        //f.m("Goal Point Is Outside Please Move The Obstacle To The Red Region And Try Again");
                        boolChangeWorkspaceDimension = false;
                    }
                    if (boolChangeWorkspaceDimension)
                    {
                        ob.wsWidth = wsWidthTemp;
                        ob.wsHeight = wsHeightTemp;
                        buttonWorkspaceSet.BackColor = Color.Lavender;
                        Invalidate();
                    }
                    else
                    {
                        MessageBoxDetailed("Please move all the items inside the red region then try again", "Change Workspace Dimension Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        boolDrawTemporaryWorkspace = true;
                        buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                    }
                    currentWorkspace = string.Format("Current={0}x{1}", ob.wsWidth, ob.wsHeight);
                    labelCurrentWorkspace.Text = currentWorkspace;
                }
                else
                {
                    MessageBoxDetailed("Animation is running", "Set Workspace Dimension Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxWorkspaceWidth.Text = ob.wsWidth.ToString();
                    textBoxWorkspaceHeight.Text = ob.wsHeight.ToString();
                    textBoxGridSize.Text = ob.gridSize.ToString();
                    textBoxNumberOfIterations.Text = numberOfIterations.ToString();
                    buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                    boolIsRunning = false;
                } 
                #endregion
                #region DrawBitmapField
                if (boolDrawBitmapField)
                {
                    frameForm1.toolStripButtonDrawBitmapField.Checked = true;
                    BitmapFieldtoolStripMenuItem.Checked = true;
                    calculateFieldToDrawBitmapField();
                }
                else
                {
                    frameForm1.toolStripButtonDrawBitmapField.Checked = false;
                    BitmapFieldtoolStripMenuItem.Checked = false;
                } 
                #endregion
                #region Grid Size and Iteration Number 
                if (!boolIsRunning)
                {
                    gridSizeBefore = ob.gridSize;
                    ob.gridSize = Convert.ToDouble(textBoxGridSize.Text);
                    if (ob.gridSize <= 0 || ob.gridSize >= 0.125 * ob.wsWidth)
                    {
                        ob.gridSize = gridSizeBefore;
                        textBoxGridSize.Text = ob.gridSize.ToString();
                        buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                        Invalidate();
                    }
                    numberOfIterationsBefore = numberOfIterations;
                    numberOfIterations = f.ig(textBoxNumberOfIterations.Text);
                    if (numberOfIterations <= 0)
                    {
                        numberOfIterations = numberOfIterationsBefore;
                        textBoxNumberOfIterations.Text = numberOfIterations.ToString();
                        buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                        Invalidate();
                    }

                    ob.getObstaclesForPF();
                    currentCalculationParameters = string.Format("Current={0}x{1}", ob.gridSize, numberOfIterations);
                    labelCurrentCalculationParameters.Text = currentCalculationParameters;
                } 
                #endregion
                boolDirty = true;
            }
            catch (Exception)
            {
                textBoxWorkspaceWidth.Text = ob.wsWidth.ToString();
                textBoxWorkspaceHeight.Text = ob.wsHeight.ToString();
                buttonWorkspaceSet.BackColor = Color.MediumTurquoise;
                textBoxGridSize.Text = ob.gridSize.ToString();
                textBoxNumberOfIterations.Text = numberOfIterations.ToString();
            }
        }
        #endregion
        #region buttonStopMobileRobot_MouseEnter
        private void buttonStopMobileRobot_MouseEnter(object sender, EventArgs e)
        {
            buttonStopMobileRobot.Focus();
        }
        #endregion
        #region textBoxObstacleDirection_TextChanged
        private void textBoxObstacleDirection_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (ob.ar.Count > 0)
                {
                    hScrollBarObstacleDirection.Value = Convert.ToInt32(textBoxObstacleDirection.Text);
                    textBoxObstacleSpeed.Text = hScrollBarObstacleSpeed.Value.ToString();
                    ob.getDirectionAndSpeed(ob.chosenShape, f.db(hScrollBarObstacleDirection.Value), f.db(hScrollBarObstacleSpeed.Value), lineLength);
                    ob.saveInitialObstacleValue(ob.chosenShape, ob.ar[ob.chosenShape]);
                    Invalidate();
                }
            }
            catch (Exception) { textBoxObstacleDirection.Text = ""; }
        }
        #endregion
        #region textBoxObstacleSpeed_TextChanged
        private void textBoxObstacleSpeed_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (ob.ar.Count > 0)
                {
                    hScrollBarObstacleSpeed.Value = Convert.ToInt32(textBoxObstacleSpeed.Text);
                    textBoxObstacleDirection.Text = hScrollBarObstacleDirection.Value.ToString();
                    ob.getDirectionAndSpeed(ob.chosenShape, f.db(hScrollBarObstacleDirection.Value), f.db(hScrollBarObstacleSpeed.Value), lineLength);
                    ob.saveInitialObstacleValue(ob.chosenShape, ob.ar[ob.chosenShape]);
                    Invalidate();
                }
            }
            catch (Exception) { textBoxObstacleSpeed.Text = ""; }
        }
        #endregion
        #region textBoxSafetyMargin_TextChanged
        private void textBoxSafetyMargin_TextChanged(object sender, EventArgs e)
        {
            CheckOverflow(textBoxSafetyMargin, 10000);
            try
            {
                if (boolChangeSafetyMarginColor)
                {
                    boolChangeSafetyMarginColor = false;
                    buttonSetSafetyMargin.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxSafetyMargin_MouseDown
        private void textBoxSafetyMargin_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeSafetyMarginColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxObsPath_TextChanged
        private void textBoxObsPath_TextChanged(object sender, EventArgs e)
        {
            CheckOverflow(textBoxObsPath, 10000);
            try
            {
                if (boolChangeObsPathColor)
                {
                    boolChangeObsPathColor = false;
                    buttonSetObsPath.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxObsPath_MouseDown
        private void textBoxObsPath_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeObsPathColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxSpeedMax_MouseDown
        private void textBoxSpeedMax_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxWorkspaceWidth_TextChanged
        private void textBoxWorkspaceWidth_TextChanged(object sender, EventArgs e)
        {
            CheckOverflow(textBoxWorkspaceWidth, 20000);
            try
            {
                if (boolChangeWorkspaceSetColor)
                {
                    boolChangeWorkspaceSetColor = false;
                    buttonWorkspaceSet.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxWorkspaceHeight_TextChanged
        private void textBoxWorkspaceHeight_TextChanged(object sender, EventArgs e)
        {
            CheckOverflow(textBoxWorkspaceHeight, 10000);
            try
            {
                if (boolChangeWorkspaceSetColor)
                {
                    boolChangeWorkspaceSetColor = false;
                    buttonWorkspaceSet.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxWorkspaceWidth_MouseDown
        private void textBoxWorkspaceWidth_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeWorkspaceSetColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region textBoxWorkspaceHeight_MouseDown
        private void textBoxWorkspaceHeight_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeWorkspaceSetColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region defaultToolStripMenuItem_Click
        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofx = ofxDefault;
            ofy = ofyDefault;
            mw = mwDefault;
            offsetXBase = (ofx) * mw; offsetYBase = ofy * mw; 
            Invalidate();
        }
        #endregion
        #region randomObstaclesToolStripMenuItem_Click
        private void randomObstaclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!boolIsRunning) getRandomObstacles();
            else MessageBoxDetailed("Animation is running", "Add Random Obstacle Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion
        #region RandomObsToolStripMenuItem_Click
        private void RandomObsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!boolIsRunning) getRandomObstacles();
            else MessageBoxDetailed("Animation is running", "Add Random Obstacle Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Invalidate();
        }
        #endregion
        #region BitmapFieldtoolStripMenuItem_Click
        private void BitmapFieldtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            boolDrawBitmapField = !boolDrawBitmapField;

            if (boolDrawBitmapField)
            {
                frameForm1.toolStripButtonDrawBitmapField.Checked = true;
                frameForm1.toolStripButtonDrawBitmapField.Text = "bitmap on";
                BitmapFieldtoolStripMenuItem.Checked = true; 
                calculateFieldToDrawBitmapField();
            }
            else
            {
                frameForm1.toolStripButtonDrawBitmapField.Checked = false;
                frameForm1.toolStripButtonDrawBitmapField.Text = "bitmap off";
                BitmapFieldtoolStripMenuItem.Checked = false; 
            }
        }
        #endregion
        #region setScaleTo1ToolStripMenuItem_Click
        private void setScaleTo1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mw = 1; sf = 0.7f;
            Invalidate();
            ofx = offsetXBase / mw;
            ofy = offsetYBase / mw;
        } 
        #endregion
        #region drawObstaclesToolStripMenuItem_Click
        private void drawObstaclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawObstacles = true;
        }
        #endregion
        #region rectangleToolStripMenuItem_Click
        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rect();
        }
        #endregion
        #region gridOnToolStripMenuItem_Click
        private void gridOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawGrid = !drawGrid;
            ob.getObstaclesForPF();
            if (drawGrid)
            {
                gridOnToolStripMenuItem.Checked = true;
                frameForm1.toolStripButtonDrawGrid.Checked = true;
                frameForm1.toolStripButtonDrawGrid.Text = "grid on";
            }
            else
            {
                gridOnToolStripMenuItem.Checked = false;
                frameForm1.toolStripButtonDrawGrid.Checked = false;
                frameForm1.toolStripButtonDrawGrid.Text = "grid off";
            }
            Invalidate();
        }
        #endregion
        #region snapToGridToolStripMenuItem_Click
        private void snapToGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ob.isSnapToGrid = !ob.isSnapToGrid;
            if (ob.isSnapToGrid)
            {
                snapToGridToolStripMenuItem.Checked = true;
                frameForm1.toolStripButtonSnapToGrid.Checked = true;
                frameForm1.toolStripButtonSnapToGrid.Text = "snap to grid on";
            }
            else
            {
                snapToGridToolStripMenuItem.Checked = false;
                frameForm1.toolStripButtonSnapToGrid.Checked = false;
                frameForm1.toolStripButtonSnapToGrid.Text = "snap to grid off";
            }
            Invalidate();
            boolDirty = true;
        }
        #endregion
        #region mouseToolStripMenuItem_Click
        private void mouseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mouse();
        }
        #endregion
        #region workspaceColorToolStripMenuItem_Click
        private void workspaceColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK) BackColor = colorDialog1.Color; workspaceColor = colorDialog1.Color;
        }
        #endregion
        #region setStartPointToolStripMenuItem_Click
        private void setStartPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setStartPointOfPath1();
        }
        #endregion
        #region setGoalToolStripMenuItem_Click
        private void setGoalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setGoal1();
        }
        #endregion
        #region setStartPointOfPathToolStripMenuItem_Click
        private void setStartPointOfPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setStartPointOfPath1();
        }
        #endregion
        #region panToolStripMenuItem_Click
        private void panToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pan = !pan;
            chooseMouse();
            if (pan) 
            { 
                panToolStripMenuItem.BackColor = Color.MediumTurquoise; 
                panToolStripMenuItem1.Checked = true; 
                this.Cursor = Cursors.Hand;
                for (int i = 0; i < ob.ar.Count; i++) { obstacle o = ob.ar[i]; o.drawPlaceHolder = false; o.ph = placeHolder.empty; }
                inv(groupBoxObstacleMotion, false);
            }
            else
            { 
                panToolStripMenuItem.BackColor = Color.FromName("Control"); 
                panToolStripMenuItem1.Checked = false; 
                this.Cursor = Cursors.Default;
            }      
        }
        #endregion
        #region fixedSizeObstacleToolStripMenuItem_Click
        private void fixedSizeObstacleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            boolFixedSize = !boolFixedSize;
            if (boolFixedSize)
            {
                frameForm1.toolStripButtonFixedSize.Checked = true;
                fixedSizeObstacleToolStripMenuItem.Checked = true;
            }
            else
            {
                frameForm1.toolStripButtonFixedSize.Checked = false;
                fixedSizeObstacleToolStripMenuItem.Checked = false;
            }
        }
        #endregion
        #region stoppedObstacleToolStripMenuItem_Click
        private void stoppedObstacleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            boolStopObstacle = !boolStopObstacle;
            if (boolStopObstacle)
            {
                frameForm1.toolStripButtonStopObstacle.Checked = true;
                stoppedObstacleToolStripMenuItem.Checked = true;
            }
            else
            {
                frameForm1.toolStripButtonStopObstacle.Checked = false;
                stoppedObstacleToolStripMenuItem.Checked = false;
            }
        }
        #endregion
        #region setAsADefaultViewToolStripMenuItem_Click
        private void setAsADefaultViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofxDefault = ofx;
            ofyDefault = ofy;
            mwDefault = mw;
        }
        #endregion
        #region showBalloonTipsToolStripMenuItem_Click
        private void showBalloonTipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            boolShowBalloonTips = !boolShowBalloonTips;
            frameForm1.boolShowBalloonTips = boolShowBalloonTips;
            if (boolShowBalloonTips)
            {
                showBalloonTipsToolStripMenuItem.Checked = true;
            }
            else
            {
                showBalloonTipsToolStripMenuItem.Checked = false;
            }
        }
        #endregion
        #region deleteSelectedObstacleToolStripMenuItem_Click
        private void deleteSelectedObstacleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ob.ar.Count > 0 && ob.chosenShape > -1 && ob.chosenShape <= (ob.ar.Count - 1))
            {
                ob.ar.RemoveAt(ob.chosenShape);
                ob.arD.RemoveAt(ob.chosenShape);
                ob.getObstaclesForPF();
                Invalidate();
                groupBoxObstacleMotion.Visible = false;
            }
        }
        #endregion
        #region checkBoxRefSpeed_CheckedChanged
        private void checkBoxRefSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRefSpeed.Checked)
            {
                boolSelectRef = true;
                string ToolTipTitleAsHint = "Reference Speed Mode Is ON";
                string ToolTipHint = "Uses the reference speed value instead of the calculated speed considering the safety margin parameter value";
                ShowToolTipAsBalloonForCheckBox(checkBoxRefSpeed, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolSelectRef = false;
                string ToolTipTitleAsHint = "Reference Speed Mode Is OFF";
                string ToolTipHint = "Uses the calculated speed considering the safety margin parameter value";
                ShowToolTipAsBalloonForCheckBox(checkBoxRefSpeed, ToolTipTitleAsHint, ToolTipHint);
            }
            boolDirty = true;
        } 
        #endregion
        #region checkBoxDrawObsPathPoint_CheckedChanged
        public bool boolDrawObsPathPoint = false;
        private void checkBoxDrawObsPathPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawObsPathPoint.Checked)
            {
                boolDrawObsPathPoint = true;
                string ToolTipTitleAsHint = "Draw Obstacle Path Mode Is ON";
                string ToolTipHint = "Draws the obstacle path";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawObsPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolDrawObsPathPoint = false;
                string ToolTipTitleAsHint = "Draw Obstacle Path Mode Is OFF";
                string ToolTipHint = "Obstacles' paths are not drawn";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawObsPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            boolDirty = true;
        }
        #endregion
        #region checkBoxDrawEnclosingCircle_CheckedChanged
        private void checkBoxDrawEnclosingCircle_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDrawEnclosingCircle.Checked)
            {
                boolDrawEnclosingCircle = true;
                string ToolTipTitleAsHint = "Draw Enclosing Circle Mode Is ON";
                string ToolTipHint = "Draws the enclosing circles to all moving obstacles";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawEnclosingCircle, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolDrawEnclosingCircle = false;
                string ToolTipTitleAsHint = "Draw Enclosing Circle Mode Is OFF";
                string ToolTipHint = "Enclosing circles are not drawn";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawEnclosingCircle, ToolTipTitleAsHint, ToolTipHint);
            }
            Invalidate();
            boolDirty = true;
        }
        #endregion
        #region CheckOverflow
        /// <summary>
        /// Checks overflow exception for textbox control
        /// </summary>
        /// <param name="textBox">Textbox that will be checked for exception</param>
        /// <param name="maxValue">Maximum value that texbox can take as an input</param>
        private void CheckOverflow(TextBox textBox, int maxValue)
        {
            try
            {
                int n = f.ig(textBox.Text);
                if (n > maxValue) textBox.Text = maxValue.ToString();
            }
            catch (Exception)
            {
                textBox.Text = "";
            }
        } 
        #endregion
        #region rect
        /// <summary>
        /// Enables drawing rectangle obstacle
        /// </summary>
        public void rect()
        {
            if (ob.chooseRec() == obstacleType.rec)
            {
                frameForm1.toolStripButtonDrawObstacle.Checked = true;
                rectangleToolStripMenuItem.Checked = true;
                mouseToolStripMenuItem.Checked = false;
                frameForm1.toolStripButtonChooseMouse.Checked = false;
                pan = false;
                if (pan) { panToolStripMenuItem.BackColor = Color.MediumTurquoise; panToolStripMenuItem1.Checked = true; this.Cursor = Cursors.Hand; }
                else { panToolStripMenuItem.BackColor = Color.FromName("Control"); panToolStripMenuItem1.Checked = false; this.Cursor = Cursors.Default; }
            }
            else rectangleToolStripMenuItem.Checked = false;
        }
        #endregion
        #region mouse
        /// <summary>
        /// Enables choosing rectangle obstacle
        /// </summary>
        public void mouse()
        {
            drawObstacles = true;
            chooseMouse();
        }
        #endregion
        #region chooseMouse
        /// <summary>
        /// Enables choosing rectangle obstacle
        /// </summary>
        private void chooseMouse()
        {
            if (ob.chooseMouse() == obstacleType.mouse)
            {
                frameForm1.toolStripButtonChooseMouse.Checked = true;
                mouseToolStripMenuItem.Checked = true;
                rectangleToolStripMenuItem.Checked = false;
                frameForm1.toolStripButtonDrawObstacle.Checked = false;
            }
            else
            {
                mouseToolStripMenuItem.Checked = false;
            }
        }
        #endregion
        #region ar vital code
        /// <summary>
        /// returns pf2D.arPathPoints
        /// </summary>
        public List<points> ar { get { return pf2D.arPathPoints; } }// important code 

        #endregion
        #region filterPath2D
        /// <summary>
        /// Filters the evaluated path
        /// </summary>
        /// <param name="ar">List that keeps evaluated path points coordinates</param>
        /// <param name="r">First coordinate index in ar that will be filtered</param>
        /// <param name="ep">Filtering coefficient</param>
        private bool filterPath2D(List<points> ar, int r, int ep)
        { 
            if (ar.Count < (ep * 3)) return false; 

            points p, p1;
            int z = 0;
            int k = ar.Count - 1;
            double toplamX = 0, toplamY = 0;
            p = ar[r]; p1 = ar[r + 1];
            p.xf = (p.x + p1.x) / 2.0; p1.xf = p.xf; p.yf = (p.y + p1.y) / 2.0; p1.yf = p.yf;
            for (int i = r + 1; i < ar.Count; i++)
            { //filtering
                if (i <= ep)
                {//first part
                    for (int j = 0; j < i * 2 + 1; j++) { p = (points)ar[j]; toplamX += p.x; toplamY += p.y; }
                    p = ar[i];
                    p.xf = toplamX / Convert.ToDouble(i * 2 + 1);
                    p.yf = toplamY / Convert.ToDouble(i * 2 + 1);
                }
                if (i > ep && i < (ar.Count - ep))
                { //middle part
                    for (int j = i - ep; j < i + ep + 1; j++) { p = ar[j]; toplamX += p.x; toplamY += p.y; }
                    p = ar[i];
                    p.xf = toplamX / Convert.ToDouble(2 * ep + 1);
                    p.yf = toplamY / Convert.ToDouble(2 * ep + 1);
                }
                if (i >= (ar.Count - ep))
                {  // last part
                    for (int j = i - (ar.Count - i - 1); j < ar.Count; j++) { p = ar[j]; toplamX += p.x; toplamY += p.y; }
                    p = ar[i]; p.xf = toplamX / Convert.ToDouble(2 * ar.Count - 2 * i - 1);
                    p.yf = toplamY / Convert.ToDouble(2 * ar.Count - 2 * i - 1);
                    z += 2;
                }
                toplamX = 0.0; toplamY = 0.0;
            }

            return false;
        }
        #endregion
        #region get2DReady
        /// <summary>
        /// Re-evaluates the path
        /// </summary>
        private void get2DReady()
        {
            ar.Clear();
            pf2D.calculateField(ob.numberOfGridLinesX, ob.numberOfGridLinesY, startX, startY, goalX, goalY, ob.gridSize, numberOfIterations);
            pf2D.getArPathPointsArray(lineLength, false);

            if (boolDrawBitmapField) b=drawBitmapField();
            try
            {
                points po1 = ar[0];
                startX = po1.xf; startY = po1.yf;
            }
            catch (Exception)
            {              

            }
        }
        #endregion
        #region updateRefSpeed
        /// <summary>
        /// Updates reference speed according to hScrollBarRefSpeed value
        /// </summary>
        private void updateRefSpeed()
        {
            hScrollBarRefSpeed.Value = refSpeed;
            labelRefSpeed.Text = refSpeed.ToString();
        }
        #endregion
        #region stopMobileRobotM
        /// <summary>
        /// Updates buttonStopMobileRobot status
        /// </summary>
        private void stopMobileRobotM()
        {
            if (stopMobileRobot)
            {
                inv(buttonStopMobileRobot, "continue");
                inv(buttonStopMobileRobot, Color.Red);
                boolIsRunning = false;
            }
            else
            {
                inv(buttonStopMobileRobot, "pause");
                inv(buttonStopMobileRobot, Color.LightBlue);
                boolIsRunning = true;
            }
            Invalidate();
        }
        #endregion
        #region getRandomObstacles
        /// <summary>
        /// Adds random obstacles
        /// </summary>
        int iLim = 0;
        int numberOfRandomObstacles = 1;
        private void getRandomObstacles()
        {
            Random r = new Random();
            for (int i = 0; i < numberOfRandomObstacles; i++)
            {
                iLim++;
                if (iLim > 1000000) break;
                obstacle o = new obstacle();
                o.directionAngleDeg = f.db(r.Next(0, 360));
                int num = r.Next(0, 2);
                if (num == 0) o.obstacleSpeed = 0;
                else o.obstacleSpeed = f.db(r.Next(1, 50));
                o.x1 = f.db(r.Next(0, 1500));
                o.y1 = f.db(r.Next(0, 800));
                if (num == 0)
                {
                    o.x2 = o.x1 + f.db(r.Next(30, 200));
                    o.y2 = o.y1 + f.db(r.Next(30, 200));
                }
                else
                {
                    o.x2 = o.x1 + f.db(r.Next(30, 50));
                    o.y2 = o.y1 + f.db(r.Next(30, 50));
                }
                int n = 0;
                for (int k = 0; k < ob.ar.Count; k++)
                {
                    obstacle o1 = ob.ar[k];
                    if (((o.x1 > o1.x1 && o.y1 > o1.y1) && (o.x1 < o1.x2 && o.y1 < o1.y2))
                        || ((o.x1 > o1.x1 && o.y2 > o1.y1) && (o.x1 < o1.x2 && o.y2 < o1.y2))
                        || ((o.x2 > o1.x1 && o.y2 > o1.y1) && (o.x2 < o1.x2 && o.y2 < o1.y2))
                    || ((o.x2 > o1.x1 && o.y1 > o1.y1) && (o.x2 < o1.x2 && o.y1 < o1.y2))
                    || ((goalX > o1.x1 && goalY > o1.y1) && (goalX < o1.x2 && goalY < o1.y2)))
                    {
                        n++;
                    }
                }
                if (n == 0)
                {
                    ob.ar.Add(o);
                    ob.getDirectionAndSpeed(ob.ar.Count - 1, o.directionAngleDeg, o.obstacleSpeed, lineLength);
                    ob.updateObs();
                    ob.setInc1(lineLength, o);
                    ob.saveInitialObstacleValue(o);
                }
                else i--;
            }
            iLim = 0;
        }
        #endregion
        #region initialize
        /// <summary>
        /// Gets initial position
        /// </summary>
        public void initialize()
        {
            inv(buttonGo, true);
            arP.Clear();
            ob.arObsInc.Clear();
            swRunTime.Reset();
            stopMobileRobot = false;
            ob.getInitialObstacleValues();
            stopMobileRobotM();
            boolBreakWhile = true;
            boolIsRunning = false;
            if (ar.Count > 0)
            {
                mr.setRobotPosition(ar[0], ar[1]);
                mr.arTrace.Clear();
            }
            elapsedTimeToGoal = 0;
            elapsedTimeToCalculate = 0;
            h = 0;
            robotSpeed = 0;
            criticalRobotSpeed = 0;
            Invalidate();
        }
        #endregion
        #region drawString
        /// <summary>
        /// Draws strings
        /// </summary>
        /// <param name="e">PaintEventArgs e</param>
        private void drawString(PaintEventArgs e)// drawstring
        {
            e.Graphics.ResetTransform();
            int baseV = 440;
            e.Graphics.DrawString(string.Format("Information"), new Font("Microsoft Sans Serif", 12.25f, FontStyle.Underline), Brushes.Black, f.si(5), f.si(baseV));
            e.Graphics.DrawString(string.Format("robotSpeed = {0}", robotSpeed), new Font("Microsoft Sans Serif", 10.25f, FontStyle.Bold), Brushes.Black, f.si(5), f.si(baseV + 20));
            e.Graphics.DrawString(string.Format("CriticalSpeed = {0}", criticalRobotSpeed), new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold), Brushes.Black, f.si(5), f.si(baseV + 45));
            e.Graphics.DrawString(string.Format("allPoints h={0} / {1}", h, ar.Count), new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold), Brushes.Black, f.si(5), f.si(baseV + 65));
            e.Graphics.DrawString(string.Format("elapsedTimeToCalculate={0} ms", elapsedTimeToCalculate), new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold), Brushes.Black, f.si(5), f.si(baseV + 85));
            e.Graphics.DrawString(string.Format("elapsedTimeToGoal={0} ms", elapsedTimeToGoal), new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold), Brushes.Black, f.si(5), f.si(baseV + 105));
            scaleT(e.Graphics);
            e.Graphics.DrawString("X", new Font("Microsoft Sans Serif", 20.25f, FontStyle.Bold), Brushes.Green, f.si(ob.wsWidth / 8d), -15f);
            e.Graphics.DrawString("O", new Font("Microsoft Sans Serif", 20.25f, FontStyle.Bold), Brushes.Green, -20, -35);
            e.Graphics.RotateTransform(180);
            e.Graphics.DrawString("Y", new Font("Microsoft Sans Serif", 20.25f, FontStyle.Bold), Brushes.Green, -13.5f, f.si(-ob.wsHeight / 8d - 30));
            scaleT(e.Graphics);
        }
        #endregion
        #region invoke
        /// <summary>
        /// Invokes actions for label's text property
        /// </summary>
        /// <param name="l">Label whose action will be invoked</param>
        /// <param name="si">Desired string value</param>
        public void inv(Label l, string si) { try { this.Invoke(new Action(() => l.Text = si)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for button's text property
        /// </summary>
        /// <param name="l">Button whose action will be invoked</param>
        /// <param name="si">Desired string value</param>
        private void inv(Button l, string si) { try { this.Invoke(new Action(() => l.Text = si)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for button's backcolor property
        /// </summary>
        /// <param name="l">Button whose action will be invoked</param>
        /// <param name="si">Desired color</param>
        public void inv(Button l, Color si) { try { this.Invoke(new Action(() => l.BackColor = si)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for groupbox's visibility property
        /// </summary>
        /// <param name="l">Groupbox whose action will be invoked</param>
        /// <param name="visibility">Desired visibility value</param>
        private void inv(GroupBox l, bool visibility) { try { this.Invoke(new Action(() => l.Visible = visibility)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for button's enabled property
        /// </summary>
        /// <param name="l">Button whose action will be invoked</param>
        /// <param name="enabled">Desired enabled value</param>
        public void inv(Button l, bool enabled) { try { this.Invoke(new Action(() => l.Enabled = enabled)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for progressbar's value property
        /// </summary>
        /// <param name="p">Progressbar whose action will be invoked</param>
        /// <param name="value">Desired value</param>
        public void inv(ProgressBar p, int value) { try { this.Invoke(new Action(() => p.Value = value)); } catch (Exception) { } }

        /// <summary>
        /// Invokes actions for progressbar's maximum property
        /// </summary>
        /// <param name="p">Progressbar whose action will be invoked</param>
        /// <param name="maximum">Desired maximum value</param>
        public void inv2(ProgressBar p, int maximum) { try { this.Invoke(new Action(() => p.Maximum = maximum)); } catch (Exception) { } }
        #endregion
        #region setGoal1
        /// <summary>
        /// Enables setting the goal position
        /// </summary>
        public void setGoal1()
        {

            if (!boolIsRunning)
            {
                setGoal = true;
                drawObstacles = false;
            }
            else MessageBoxDetailed("Animation is running", "Set Goal Point Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion
        #region setStartPointOfPath1
        /// <summary>
        /// Enables setting the start position
        /// </summary>
        public void setStartPointOfPath1()
        {
            if (!boolIsRunning)
            {
                setStartPointOfPath = true;
                drawObstacles = false;
            }
            else MessageBoxDetailed("Animation is running", "Set Start Point Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion
        #region getTransCoor
        /// <summary>
        /// Transforms mouse coordinates 
        /// </summary>
        /// <param name="e">MouseEventArgs e</param>
        public void getTransCoor(MouseEventArgs e)
        {
            eX = ((f.db(e.X) / (mw * sf)) - ofx);
            eY = -((f.db(e.Y) / (mw * sf)) + ofy);
        }
        #endregion
        #region calculateFieldToDrawBitmapField
        /// <summary>
        /// Performs potential field calculations on the field to draw bitmap field
        /// </summary>
        public void calculateFieldToDrawBitmapField()
        {
            ob.arObsInc.Clear(); arP.Clear(); ar.Clear(); pf2D.arPathPointsAll.Clear();mr.arTrace.Clear();	
            inv(groupBoxObstacleMotion, false);
            ob.getInitialObstacleValues();
            ob.getObstaclesForPF();        
            pf2D.calculateField(ob.numberOfGridLinesX, ob.numberOfGridLinesY, startX, startY, goalX, goalY, ob.gridSize, numberOfIterations);
            pf2D.getArPathPointsArray(lineLength, false);
            filterPath2D(ar, 0, Convert.ToInt32(filterV));
            if (boolDrawBitmapField) this.Invoke(new MethodInvoker(delegate() { b = drawBitmapField(); Invalidate(); }));
        }
        #endregion
        #region ShowToolTipAsBalloonForTextBox
        /// <summary>
        /// Shows tooltip as balloon when mouse hover the textbox 
        /// </summary>
        /// <param name="tb">Textbox that is shown tooltip</param>
        /// <param name="ToolTipTitleAsHint">Title of the tooltip</param>
        /// <param name="ToolTipHint">Instruction of the tooltip</param>
        private void ShowToolTipAsBalloonForTextBox(TextBox tb, string ToolTipTitleAsHint, string ToolTipHint)
        {
            toolTipAsHint.ToolTipTitle = ToolTipTitleAsHint;
            toolTipAsHint.UseFading = true;
            toolTipAsHint.UseAnimation = true;
            toolTipAsHint.IsBalloon = true;
            toolTipAsHint.ToolTipIcon = ToolTipIcon.Info;
            toolTipAsHint.ShowAlways = true;
            toolTipAsHint.AutoPopDelay = 5000;
            toolTipAsHint.InitialDelay = 1000;
            toolTipAsHint.ReshowDelay = 500;
            if (boolShowBalloonTips) toolTipAsHint.SetToolTip(tb, ToolTipHint);
            else toolTipAsHint.RemoveAll();
        }
        #endregion
        #region ShowToolTipAsBalloonForButton
        /// <summary>
        /// Shows tooltip as balloon when mouse hover the button 
        /// </summary>
        /// <param name="tb">Button that is shown tooltip</param>
        /// <param name="ToolTipTitleAsHint">Title of the tooltip</param>
        /// <param name="ToolTipHint">Instruction of the tooltip</param>
        private void ShowToolTipAsBalloonForButton(Button b, string ToolTipTitleAsHint, string ToolTipHint)
        {
            toolTipAsHint.ToolTipTitle = ToolTipTitleAsHint;
            toolTipAsHint.UseFading = true;
            toolTipAsHint.UseAnimation = true;
            toolTipAsHint.IsBalloon = true;
            toolTipAsHint.ToolTipIcon = ToolTipIcon.Info;
            toolTipAsHint.ShowAlways = true;
            toolTipAsHint.AutoPopDelay = 5000;
            toolTipAsHint.InitialDelay = 1000;
            toolTipAsHint.ReshowDelay = 500;
            if (boolShowBalloonTips) toolTipAsHint.SetToolTip(b, ToolTipHint);
            else toolTipAsHint.RemoveAll();
        }
        #endregion
        #region ShowToolTipAsBalloonForButtonAsWarning
        /// <summary>
        /// Shows tooltip as balloon to warn the user 
        /// </summary>
        /// <param name="tb">Button that is shown tooltip</param>
        /// <param name="ToolTipTitleAsHint">Title of the tooltip</param>
        /// <param name="ToolTipHint">Instruction of the tooltip</param>
        private void ShowToolTipAsBalloonForButtonAsWarning(Button b, string ToolTipTitleAsHint, string ToolTipHint)
        {
            ToolTip ToolTipAsHint = new ToolTip();
            ToolTipAsHint.ToolTipTitle = ToolTipTitleAsHint;
            ToolTipAsHint.UseFading = true;
            ToolTipAsHint.UseAnimation = true;
            ToolTipAsHint.IsBalloon = true;
            ToolTipAsHint.ToolTipIcon = ToolTipIcon.Warning;
            ToolTipAsHint.ShowAlways = true;
            ToolTipAsHint.AutoPopDelay = 5000;
            ToolTipAsHint.InitialDelay = 1000;
            ToolTipAsHint.ReshowDelay = 500;
            ToolTipAsHint.SetToolTip(b, ToolTipHint);
        }
        #endregion
        #region ShowToolTipAsBalloonForCheckBox
        /// <summary>
        /// Shows tooltip as balloon when mouse hover the checkbox
        /// </summary>
        /// <param name="tb">Checkbox that is shown tooltip</param>
        /// <param name="ToolTipTitleAsHint">Title of the tooltip</param>
        /// <param name="ToolTipHint">Instruction of the tooltip</param>
        private void ShowToolTipAsBalloonForCheckBox(CheckBox cb, string ToolTipTitleAsHint, string ToolTipHint)
        {
            toolTipAsHint.ToolTipTitle = ToolTipTitleAsHint;
            toolTipAsHint.UseFading = true;
            toolTipAsHint.UseAnimation = true;
            toolTipAsHint.IsBalloon = true;
            toolTipAsHint.ToolTipIcon = ToolTipIcon.Info;
            toolTipAsHint.ShowAlways = true;
            toolTipAsHint.AutoPopDelay = 5000;
            toolTipAsHint.InitialDelay = 1000;
            toolTipAsHint.ReshowDelay = 500;
            if (boolShowBalloonTips) toolTipAsHint.SetToolTip(cb, ToolTipHint);
            else toolTipAsHint.RemoveAll();
        }
        #endregion
        #region ShowToolTipAsBalloonForHScrollBar
        /// <summary>
        /// Shows tooltip as balloon when mouse hover the hscrollbar 
        /// </summary>
        /// <param name="tb">Hscrollbar that is shown tooltip</param>
        /// <param name="ToolTipTitleAsHint">Title of the tooltip</param>
        /// <param name="ToolTipHint">Instruction of the tooltip</param>
        private void ShowToolTipAsBalloonForHScrollBar(HScrollBar hsb, string ToolTipTitleAsHint, string ToolTipHint)
        {
            toolTipAsHint.ToolTipTitle = ToolTipTitleAsHint;
            toolTipAsHint.UseFading = true;
            toolTipAsHint.UseAnimation = true;
            toolTipAsHint.IsBalloon = true;
            toolTipAsHint.ToolTipIcon = ToolTipIcon.Info;
            toolTipAsHint.ShowAlways = true;
            toolTipAsHint.AutoPopDelay = 5000;
            toolTipAsHint.InitialDelay = 1000;
            toolTipAsHint.ReshowDelay = 500;
            if (boolShowBalloonTips) toolTipAsHint.SetToolTip(hsb, ToolTipHint);
            else toolTipAsHint.RemoveAll();
        }
        #endregion
        #region button1ms_MouseHover
        private void button1ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 1 milisecond";
            ShowToolTipAsBalloonForButton(button1ms, ToolTipTitleAsHint, ToolTipHint);
        } 
        #endregion
        #region button10ms_MouseHover
        private void button10ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 10 miliseconds";
            ShowToolTipAsBalloonForButton(button10ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region button20ms_MouseHover
        private void button20ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 20 miliseconds";
            ShowToolTipAsBalloonForButton(button20ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region button40ms_MouseHover
        private void button40ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 40 miliseconds";
            ShowToolTipAsBalloonForButton(button40ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region button80ms_MouseHover
        private void button80ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 80 miliseconds";
            ShowToolTipAsBalloonForButton(button80ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region button150ms_MouseHover
        private void button150ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 150 miliseconds";
            ShowToolTipAsBalloonForButton(button150ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region button1000ms_MouseHover
        private void button1000ms_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation 1 second";
            ShowToolTipAsBalloonForButton(button1000ms, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region textBoxSafetyMargin_MouseHover
        private void textBoxSafetyMargin_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Safety Margin Of The Robot Motion";
            string ToolTipHint = "If value is zero robot moves with criticalSpeed and coincides with the obstacles during its traverse ";
            ShowToolTipAsBalloonForTextBox(textBoxSafetyMargin, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonSafetyMarginDec_MouseHover
        private void buttonSafetyMarginDec_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Safety Margin Of The Robot Motion";
            string ToolTipHint = "Decreases the value by 500 per each click";
            ShowToolTipAsBalloonForButton(buttonSafetyMarginDec, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonSafetyMarginInc_MouseHover
        private void buttonSafetyMarginInc_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Safety Margin Of The Robot Motion";
            string ToolTipHint = "Increases the value by 500 per each click";
            ShowToolTipAsBalloonForButton(buttonSafetyMarginInc, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonSetSafetyMargin_MouseHover
        private void buttonSetSafetyMargin_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Safety Margin Of The Robot Motion";
            string ToolTipHint = "Sets the safety margin of the robot motion";
            ShowToolTipAsBalloonForButton(buttonSetSafetyMargin, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonGo_MouseHover
        private void buttonGo_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Go method";
            string ToolTipHint = "Runs the go method";
            ShowToolTipAsBalloonForButton(buttonGo, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonStopMobileRobot_MouseHover
        private void buttonStopMobileRobot_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Pause / Continue";
            string ToolTipHint = "Pauses / Continues the animation";
            ShowToolTipAsBalloonForButton(buttonStopMobileRobot, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonInitial_MouseHover
        private void buttonInitial_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Initial position";
            string ToolTipHint = "Gets the robot to initial position";
            ShowToolTipAsBalloonForButton(buttonInitial, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region textBoxObsPath_MouseHover
        private void textBoxObsPath_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Obstacle Path Drawing Increment Value";
            string ToolTipHint = "If value is one and drawobspath checkbox is checked, it draws the obstacle path per each motion";
            ShowToolTipAsBalloonForTextBox(textBoxObsPath, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonObsPathDec_MouseHover
        private void buttonObsPathDec_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Obstacle Path Drawing Increment Value";
            string ToolTipHint = "Decreases the value by 10 per each click";
            ShowToolTipAsBalloonForButton(buttonObsPathDec, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonObsPathInc_MouseHover
        private void buttonObsPathInc_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Obstacle Path Drawing Increment Value";
            string ToolTipHint = "Increases the value by 10 per each click";
            ShowToolTipAsBalloonForButton(buttonObsPathInc, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonSetObsPath_MouseHover
        private void buttonSetObsPath_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Obstacle Path Drawing Increment Value";
            string ToolTipHint = "Sets the increment value of the obstacles path";
            ShowToolTipAsBalloonForButton(buttonSetObsPath, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region checkBoxRefSpeed_MouseHover
        private void checkBoxRefSpeed_MouseHover(object sender, EventArgs e)
        {
            if (checkBoxRefSpeed.Checked)
            {
                boolSelectRef = true;
                string ToolTipTitleAsHint = "Reference Speed Mode Is ON";
                string ToolTipHint = "Uses the reference speed value instead of the calculated speed considering the safety margin parameter value";
                ShowToolTipAsBalloonForCheckBox(checkBoxRefSpeed, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolSelectRef = false;
                string ToolTipTitleAsHint = "Reference Speed Mode Is OFF";
                string ToolTipHint = "Uses the calculated speed considering the safety margin parameter value";
                ShowToolTipAsBalloonForCheckBox(checkBoxRefSpeed, ToolTipTitleAsHint, ToolTipHint);
            }
        } 
        #endregion
        #region checkBoxDrawEnclosingCircle_MouseHover
        private void checkBoxDrawEnclosingCircle_MouseHover(object sender, EventArgs e)
        {
            if (checkBoxDrawEnclosingCircle.Checked)
            {
                string ToolTipTitleAsHint = "Draw Enclosing Circle Mode Is ON";
                string ToolTipHint = "Draws the enclosing circles to all moving obstacles";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawEnclosingCircle, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Draw Enclosing Circle Mode Is OFF";
                string ToolTipHint = "Enclosing circles are not drawn";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawEnclosingCircle, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region checkBoxDrawObsPathPoint_MouseHover
        private void checkBoxDrawObsPathPoint_MouseHover(object sender, EventArgs e)
        {
            if (checkBoxDrawObsPathPoint.Checked)
            {
                string ToolTipTitleAsHint = "Draw Obstacle Path Mode Is ON";
                string ToolTipHint = "Draws the obstacle path";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawObsPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Draw Obstacle Path Mode Is OFF";
                string ToolTipHint = "Obstacles' paths are not drawn";
                ShowToolTipAsBalloonForCheckBox(checkBoxDrawObsPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region checkBoxSeekingForPathPoint_CheckedChanged
        private void checkBoxSeekingForPathPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSeekingForPathPoint.Checked)
            {
                boolSeekForPointCoor = true;
                string ToolTipTitleAsHint = "Seeking Path Point Mode Is ON";
                string ToolTipHint = "Seeks for the path point number by moving the left button pressed mouse on the path";
                ShowToolTipAsBalloonForCheckBox(checkBoxSeekingForPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolSeekForPointCoor = false;
                string ToolTipTitleAsHint = "Seeking Path Point Mode Is OFF";
                string ToolTipHint = "Activate this mode to seeks for the path point number";
                ShowToolTipAsBalloonForCheckBox(checkBoxSeekingForPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            boolDirty = true;
        }
        #endregion
        #region checkBoxSeekingForPathPoint
        private void checkBoxSeekingForPathPoint_MouseHover(object sender, EventArgs e)
        {
            if (checkBoxSeekingForPathPoint.Checked)
            {
                boolSeekForPointCoor = true;
                string ToolTipTitleAsHint = "Seeking Path Point Mode Is ON";
                string ToolTipHint = "Seeks for the path point number by moving the left button pressed mouse on the path";
                ShowToolTipAsBalloonForCheckBox(checkBoxSeekingForPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                boolSeekForPointCoor = false;
                string ToolTipTitleAsHint = "Seeking Path Point Mode Is OFF";
                string ToolTipHint = "Activate this mode to seek for the path point number";
                ShowToolTipAsBalloonForCheckBox(checkBoxSeekingForPathPoint, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region hScrollBarFilter_MouseHover
        private void hScrollBarFilter_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Filtering The Path";
            string ToolTipHint = "Sets the filtering coefficient";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarFilter, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarMajorTick_MouseHover
        private void hScrollBarMajorTick_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Major Tick";
            string ToolTipHint = "Sets the major tick value drawn on the path to specify the distance";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarMajorTick, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarLineLength_MouseHover
        private void hScrollBarLineLength_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Line Length";
            string ToolTipHint = "Sets the line length value between the two path points";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarLineLength, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarRefSpeed_MouseHover
        private void hScrollBarRefSpeed_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Reference Speed Value Of The Robot";
            string ToolTipHint = "Sets the reference speed value";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarRefSpeed, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarDelay_MouseHover
        private void hScrollBarDelay_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Delay Value Of The Animation";
            string ToolTipHint = "Delays the animation";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarDelay, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarObstacleDirection_MouseHover
        private void hScrollBarObstacleDirection_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Motion Direction Of The Obstacle";
            string ToolTipHint = "Sets the direction of the selected obstacle motion";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarObstacleDirection, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region hScrollBarObstacleSpeed_MouseHover
        private void hScrollBarObstacleSpeed_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Obstacle Speed";
            string ToolTipHint = "Sets the speed of the selected obstacle";
            ShowToolTipAsBalloonForHScrollBar(hScrollBarObstacleSpeed, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region textBoxWorkspaceWidth_MouseHover
        private void textBoxWorkspaceWidth_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Width Value Of The Workspace";
            string ToolTipHint = "Please enter the width of the workspace then click the set button";
            ShowToolTipAsBalloonForTextBox(textBoxWorkspaceWidth, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region textBoxWorkspaceHeight_MouseHover
        private void textBoxWorkspaceHeight_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Height Value Of The Workspace";
            string ToolTipHint = "Please enter the height of the workspace then click the set button";
            ShowToolTipAsBalloonForTextBox(textBoxWorkspaceHeight, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region buttonWorkspaceSet_MouseHover
        private void buttonWorkspaceSet_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Dimensions Of The Workspace";
            string ToolTipHint = "Sets dimensions of the workspace";
            ShowToolTipAsBalloonForButton(buttonWorkspaceSet, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region MessageBoxDetailed
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info">Instruction of the messagebox</param>
        /// <param name="title">Title of the messagebox</param>
        /// <param name="mbButtons">Buttons of the messagebox</param>
        /// <param name="mbIcon">Icon of the messagebox</param>
        public void MessageBoxDetailed(string info, string title, MessageBoxButtons mbButtons, MessageBoxIcon mbIcon)
        {
            MessageBox.Show(info, title, mbButtons, mbIcon);
        }
        #endregion
        #region algorithm
        #region variables
        int h = 0;
        public List<points> arP = new List<points>();
        #endregion
        #region buttonGo_Click
        Thread t;
        private void buttonGo_Click(object sender, EventArgs e)
        {
            t = new Thread(new ThreadStart(go));
            t.Priority = ThreadPriority.Highest;
            t.Start();
        }
        #endregion
        #region go
        /// <summary>
        /// Runs go method
        /// </summary>
        private void go()
        {
            try// go
            {
                swCalculationTime.Start();
                swRunTime.Start();
                #region Control Path Obstruction For Single Obstacle
                for (int i = 0; i < ob.arD.Count; i++)// Single Obstacle Obstruction
                {
                    obstacle o = ob.arD[i];

                    if (goalX > o.xm && startX < o.xm && Math.Abs(o.y2 - o.y1) >= ob.wsHeight - 40)
                    {
                        MessageBoxDetailed("Path is vertically obstructed by a stationary obstacle", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (startX > o.xm && goalX < o.xm && Math.Abs(o.y2 - o.y1) >= ob.wsHeight - 40)
                    {
                        MessageBoxDetailed("Path is vertically obstructed by a stationary obstacle", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (goalY > o.ym && startY < o.ym && Math.Abs(o.x2 - o.x1) >= ob.wsWidth - 40)
                    {
                        MessageBoxDetailed("Path is horizontally obstructed by a stationary obstacle", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (startY > o.ym && goalY < o.ym && Math.Abs(o.x2 - o.x1) >= ob.wsWidth - 40)
                    {
                        MessageBoxDetailed("Path is horizontally obstructed by a stationary obstacle", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                #endregion
                #region Control If Start Goal In the Obstacles

                for (int i = 0; i < ob.arD.Count; i++)// Single Obstacle Obstruction
                {
                    obstacle o = ob.arD[i];

                    if (startX >= o.x1 && startX <= o.x2 && startY >= o.y1 && startY <= o.y2)
                    {
                        MessageBoxDetailed("Start point is in the obstacle", "Start Point Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (goalX >= o.x1 && goalX <= o.x2 && goalY >= o.y1 && goalY <= o.y2)
                    {
                        MessageBoxDetailed("Goal point is in the obstacle", "Goal Point Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                #endregion
                #region Control If Any Item Is Outside Of The Workspace
                boolIsOutsideOfTheWorkspace = false;
                for (int i = 0; i < ob.arD.Count; i++)
                {
                    obstacle o = ob.arD[i];
                    if (o.x1 < 0 || o.x1 > ob.wsWidth - Math.Abs(o.x2 - o.x1) || o.y1 < 0 || o.y1 > ob.wsHeight - Math.Abs(o.y2 - o.y1))
                    {
                        boolIsOutsideOfTheWorkspace = true;
                    }
                }

                if (startX < 0 || startX > ob.wsWidth || startY < 0 || startY > ob.wsHeight)
                {
                    boolIsOutsideOfTheWorkspace = true;
                }

                if (goalX < 0 || goalX > ob.wsWidth || goalY < 0 || goalY > ob.wsHeight)
                {
                    boolIsOutsideOfTheWorkspace = true;
                }

                if (boolIsOutsideOfTheWorkspace)
                {
                    MessageBoxDetailed("Please move all the items to the wokspace then try again", "Out Of Workspace Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                #endregion
              
                inv(buttonGo, false);
                boolIsRunning = true;
                elapsedTimeToGoal = 0;
                boolBreakWhile = false;
                ob.arObsInc.Clear();
                arP.Clear();

                stopMobileRobot = false;
                stopMobileRobotM();
                inv(groupBoxObstacleMotion, false);
                ob.getInitialObstacleValues();
                mr.arTrace.Clear();			
                ob.getObstaclesForPF();
                ar.Clear(); pf2D.arPathPointsAll.Clear();
                pf2D.calculateField(ob.numberOfGridLinesX, ob.numberOfGridLinesY, startX, startY, goalX, goalY, ob.gridSize, numberOfIterations);
                pf2D.getArPathPointsArray(lineLength, false);
                filterPath2D(ar, 0, Convert.ToInt32(filterV));

                if (boolDrawBitmapField) this.Invoke(new MethodInvoker(delegate() { b=drawBitmapField(); }));

                #region     Control If Path Point Is In the Stationary Obstacle Control
                for (int i = 0; i < ob.arD.Count; i++)
                {
                    obstacle o = ob.arD[i];
                    if (o.stationary)
                    {
                        for (int j = 0; j < ar.Count; j++)
                        {
                            points p = ar[j];
                            if (p.xf >= o.x1 && p.xf <= o.x2 && p.yf >= o.y1 && p.yf <= o.y2)
                            {
                                ar.Clear();
                                pf2D.arPathPointsAll.Clear();
                                MessageBoxDetailed("Path is obstructed by stationary obstacles", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }
                }
                #endregion

                int robotPosition = 0; h = 0;
                arS = ob.getIntersectionWithPath(0);
                if (arS.Count == 0)robotSpeed = refSpeed;
                else
                {
                    robotSpeed = arS[0].robotSpeed;
                    robotPosition = arS[0].robotPosition;
                    ob.resetBoolDone();

                    if (boolSelectRef)
                    {
                        if (arS[0].robotSpeed < refSpeed)
                        {
                            robotSpeed = refSpeed;
                            robotPosition = arS[0].robotPosition;
                            ob.resetBoolDone();
                        }
                        else
                        {
                            robotSpeed = arS[0].robotSpeed;
                            robotPosition = arS[0].robotPosition;
                            ob.resetBoolDone();
                        } 
                    }             
                }
                elapsedTimeToCalculate = swCalculationTime.ElapsedMilliseconds;
                swCalculationTime.Reset();
                while (true)
                {
                    if (boolBreakWhile) break;
                    ob.boundryCheck();
                    this.Invoke(new MethodInvoker(delegate() { ob.setInc(); }));
                    this.Invoke(new MethodInvoker(delegate() { ob.updateObs();}));

                    if (h > (robotPosition + safetyMargin))
                    {
                        ne = 0;
                        if (arS.Count > 0)
                        {
                            ob.arS[0].boolDone = true; 
                        }
                        arS = ob.getIntersectionWithPath(h);

                        if (arS.Count > 0)
                        {
                            obstacle o1 = arS[0];
                            robotSpeed = o1.robotSpeed;
                            robotPosition = o1.robotPosition;
                            robotSpeed = arS[0].robotSpeed;
                            robotPosition = arS[0].robotPosition;

                            if (checkBoxRefSpeed.Checked)
                            {
                                if (arS[0].robotSpeed < refSpeed)
                                {
                                    robotSpeed = refSpeed;
                                    robotPosition = arS[0].robotPosition;
                                }
                                else
                                {
                                    robotSpeed = arS[0].robotSpeed;
                                    robotPosition = arS[0].robotPosition;
                                } 
                            }
                        }
                        else
                        {
                            robotSpeed = refSpeed;
                            robotPosition = ar.Count;
                        }
                    }
                    delay = hScrollBarDelay.Value;

                    int diff = ar.Count - h - 2;
                    if (diff > 0 && diff < robotSpeed)
                    {
                        points p = ar[h - diff];
                        points p1 = ar[h + diff];
                        mr.setRobotPosition(p, p1);
                    }
                    else if (diff > 0)
                    {
                        points p = ar[h];
                        points p1 = ar[h + robotSpeed];
                        mr.setRobotPosition(p, p1);
                    }
                    else
                    {
                        boolIsRunning = false;
                        break;
                    }
                    while (stopMobileRobot) { }

                    this.Invoke(new MethodInvoker(delegate() { Invalidate(); }));

                    if (boolActivateDelay) f.s(delay);
                    h += robotSpeed;
                    ne++;
                }
            }
            catch (Exception) { t.Abort(); } 

            elapsedTimeToGoal = swRunTime.ElapsedMilliseconds;
            Invalidate();
            inv(buttonGo, true);
            swRunTime.Reset();
        }
        #endregion
        #region initialCalculations
        /// <summary>
        /// Performs necessary calculations before applying method
        /// </summary>
        public void initialCalculations()
        {// initialCalculations
            arTimePassed.Clear();
            arP.Clear();
            stopMobileRobot = false;
            stopMobileRobotM();
            inv(groupBoxObstacleMotion, false);
            ob.getInitialObstacleValues();
            mr.arTrace.Clear();
            ob.getObstaclesForPF();
            pf2D.arPathPoints.Clear(); pf2D.arPathPointsAll.Clear();
            pf2D.calculateField(ob.numberOfGridLinesX, ob.numberOfGridLinesY, startX, startY, goalX, goalY, ob.gridSize, numberOfIterations);
            pf2D.getArPathPointsArray(lineLength, false);
            filterPath2D(pf2D.arPathPoints, 0, Convert.ToInt32(filterV));
            if (boolDrawBitmapField) this.Invoke(new MethodInvoker(delegate() { b=drawBitmapField(); }));
            robotPosition = 0; h = 0; boolBreakWhile = false;
            refSpeedM = refSpeed;
            arS1 = new List<obstacle>();
            arS = ob.getIntersectionWithPathForInitialCalculations();
            // path obstruction
            #region     Control If Path Point Is In the Stationary Obstacle Control
            for (int i = 0; i < ob.arD.Count; i++)
            {
                obstacle o = ob.arD[i];
                if (o.stationary)
                {
                    for (int j = 0; j < pf2D.arPathPoints.Count; j++)
                    {
                        points p = pf2D.arPathPoints[j];
                        if (p.xf >= o.x1 && p.xf <= o.x2 && p.yf >= o.y1 && p.yf <= o.y2)
                        {
                            pf2D.arPathPoints.Clear();
                            pf2D.arPathPointsAll.Clear();
                            MessageBoxDetailed("Path is obstructed by stationary obstacles", "Path Obstruction Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
            }
            #endregion
            //
        }
        #endregion
        #endregion
        #region buttonSetGridSize_Click
        private void buttonSetGridSize_Click(object sender, EventArgs e)
        {
            try
            {
                if (!boolIsRunning)
                {
                    ob.gridSize = Convert.ToDouble(textBoxGridSize.Text);
                    initialCalculations();
                }
            }
            catch (Exception)
            {
                textBoxGridSize.Text = ob.gridSize.ToString();
            }
        } 
        #endregion
        #region buttonCancelProgress_Click
        private void buttonCancelProgress_Click(object sender, EventArgs e)
        {
            boolBreakWhile = true;
            boolIsRunning = false;
            progressBarIteration.Value = 0;
            labelProgress.Text = "Operation Terminated";
        }
        #endregion
        #region textBoxGridSize_TextChanged
        private void textBoxGridSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (boolChangeWorkspaceSetColor)
                {
                    boolChangeWorkspaceSetColor = false;
                    buttonWorkspaceSet.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        } 
        #endregion
        #region textBoxGridSize_MouseDown
        private void textBoxGridSize_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeWorkspaceSetColor = true;
            }
            catch (Exception) { }
        } 
        #endregion
        #region textBoxNumberOfIterations_MouseDown
        private void textBoxNumberOfIterations_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                boolChangeWorkspaceSetColor = true;
            }
            catch (Exception) { }
        }
        #endregion
        #region mainForm_MouseEnter
        private void mainForm_MouseEnter(object sender, EventArgs e)
        {
            if (pan) Cursor = Cursors.Hand;
            else Cursor = Cursors.Arrow;
        } 
        #endregion
        #region textBoxNumberOfIterations_TextChanged
        private void textBoxNumberOfIterations_TextChanged(object sender, EventArgs e)
        {
            CheckOverflow(textBoxNumberOfIterations, 100000);
            try
            {
                if (boolChangeWorkspaceSetColor)
                {
                    boolChangeWorkspaceSetColor = false;
                    buttonWorkspaceSet.BackColor = Color.Red;
                }
            }
            catch (Exception) { }
        } 
        #endregion
        #region UserManualToolStripMenuItem_Click
        private void UserManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                help h = new help();
                h.Show();
                h.Text = "GoAvoid User's Manual";
                h.axAcroPDF1.LoadFile("manual//GoAvoid_User_Manual.pdf");
            }
            catch (Exception)
            {
                MessageBoxDetailed("Adode Acrobat Reader is required", "Open PDF File Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region licenseAgreementToolStripMenuItem_Click
        private void licenseAgreementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                help h = new help();
                h.Show();
                h.Text = "License Agreement";
                h.axAcroPDF1.LoadFile("license//License.pdf");
            }
            catch (Exception)
            {
                MessageBoxDetailed("Adode Acrobat Reader is required", "Open PDF File Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region aboutToolStripMenuItem_Click
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about a = new about();
            a.Show();
        }
        #endregion
        #region Form closing
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (boolDirty)
            {
                DialogResult save = MessageBox.Show("Do you want to save changes ?", "GoAvoid", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (save == DialogResult.Yes)
                {
                    if (!saved)
                    {
                        SaveFileDialog saveDlg = new SaveFileDialog();
                        saveDlg.Filter = "GoAvoid Files (*.goav)|*.goav|All Files (*.*)|*.*";
                        saveDlg.DefaultExt = ".goav"; saveDlg.FileName = "GoAv01.goav";
                        if (saveDlg.ShowDialog() == DialogResult.OK) saveDoc(saveDlg.FileName);
                        else if (saveDlg.ShowDialog() == DialogResult.Cancel) e.Cancel = true;

                       frameForm1.readRegKeys();
                       frameForm1.writeRegKeys(saveDlg.FileName);
                    }
                    else saveDoc(this.fileNameN);
                }
                else if (save == DialogResult.No) return;
                else e.Cancel = true; 
            }
            }
        #endregion 
    }
}