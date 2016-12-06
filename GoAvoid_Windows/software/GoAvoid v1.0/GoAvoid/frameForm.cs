
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
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Permissions;
using Microsoft.Win32;
[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify = "HKEY_CURRENT_USER")]

namespace GoAvoid
{
    public partial class frameFormBitmap : Form
    {
        #region var
        public bool boolFirstTime = true;
        public bool boolShowBalloonTips = false;
        int je = 0, i = 0;
        static int docCount;
        RegistryKey GoAvoidFiles;
        String[] docList = new String[6];
        pField2D pf2D = new pField2D();
        #endregion
        #region InitializeComponent
        public frameFormBitmap()
        {
            InitializeComponent();
            toolStrip1.Renderer = new MyRenderer();
        }
        #endregion
        #region ToolStripProfessionalRenderer
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                var btn = e.Item as ToolStripButton;
                if (btn != null && btn.CheckOnClick && btn.Checked)
                {
                    Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
                    e.Graphics.FillRectangle(Brushes.MediumTurquoise, bounds);
                    #region string check
                    string gridOff = "grid off";
                    string gridOn = "grid on";
                    string snapToGridOff = "snap to grid off";
                    string snapToGridOn = "snap to grid on";
                    string bitmapOff = "bitmap off";
                    string bitmapOn = "bitmap on";
                    if (btn.Text == gridOff) btn.Text = gridOn;
                    if (btn.Text == snapToGridOff) btn.Text = snapToGridOn;
                    if (btn.Text == bitmapOff) btn.Text = bitmapOn;
                    #endregion
                }
                else
                {
                    #region string check
                    string gridOff = "grid off";
                    string gridOn = "grid on";
                    string snapToGridOff = "snap to grid off";
                    string snapToGridOn = "snap to grid on";
                    string bitmapOff = "bitmap off";
                    string bitmapOn = "bitmap on";
                    if (btn.Text == gridOn) btn.Text = gridOff;
                    if (btn.Text == snapToGridOn) btn.Text = snapToGridOff;
                    if (btn.Text == bitmapOn) btn.Text = bitmapOff;
                    #endregion
                    base.OnRenderButtonBackground(e);
                }
            }
        }
        #endregion
        #region load
        public void Form1_Load(object sender, EventArgs e)
        {

            for (int j = 0; j < 6; j++) docList[j] = "";
            timerOpenDoc.Enabled = true;

            Screen[] screens = Screen.AllScreens;
            int numberOfScreen = screens.GetUpperBound(0);

            if (numberOfScreen > 0)
            {
                this.Width = screens[1].WorkingArea.Width;
                this.Height = screens[1].WorkingArea.Height;
                this.Location = new Point(screens[1].WorkingArea.Location.X, screens[1].WorkingArea.Location.Y); 
            }
            else
            {
                this.Location = Screen.PrimaryScreen.WorkingArea.Location;
                this.Width = Screen.PrimaryScreen.WorkingArea.Width;
                this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            }
            pf2D.frameform1 = this;
        }
        #endregion
        #region newToolStripMenuItem_Click
        private void newToolStripMenuItem_Click(object sender, EventArgs e) { createDoc(); }
        #endregion
        #region openToolStripMenuItem_Click
        private void openToolStripMenuItem_Click(object sender, EventArgs e) { open(); }
        #endregion
        #region saveToolStripMenuItem_Click
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) { save(); }
        #endregion
        #region saveAsToolStripMenuItem_Click
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) { mainForm sv = (mainForm)(this.ActiveMdiChild); if (sv != null) { sv.saved = false; save(); } }
        #endregion
        #region tileHorizontalToolStripMenuItem_Click
        private void tileHorizontalToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileHorizontal); }
        #endregion
        #region tileVerticalToolStripMenuItem_Click
        private void tileVerticalToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.TileVertical); }
        #endregion
        #region cascadeToolStripMenuItem_Click
        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e) { this.LayoutMdi(MdiLayout.Cascade); }
        #endregion
        #region closeToolStripMenuItem_Click
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) { mainForm sv = (mainForm)(this.ActiveMdiChild); if (sv != null) sv.Close(); }
        #endregion
        #region file0ToolStripMenuItem_Click
        private void file0ToolStripMenuItem_Click(object sender, EventArgs e) {

            if (File.Exists(docList[0]))
            {
                mainForm ce = createDoc(docList[0]); readRegKeys();
                ce.openDoc(docList[0]); ce.saved = true;

                writeRegKeys(docList[0]);
            }
            else MessageBox.Show("File Couldn't Be Opened", "File Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion
        #region file1ToolStripMenuItem_Click
        private void file1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(docList[0]))
            { mainForm ce = createDoc(docList[1]); readRegKeys(); ce.openDoc(docList[1]); ce.saved = true; writeRegKeys(docList[1]); }
        }
        #endregion
        #region file2ToolStripMenuItem_Click
        private void file2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(docList[0]))
            { mainForm ce = createDoc(docList[2]); readRegKeys(); ce.openDoc(docList[2]); ce.saved = true; writeRegKeys(docList[2]); }
        }
        #endregion
        #region file3ToolStripMenuItem_Click
        private void file3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(docList[0]))
            { mainForm ce = createDoc(docList[3]); readRegKeys(); ce.openDoc(docList[3]); ce.saved = true; writeRegKeys(docList[3]); }
        }
        #endregion
        #region file4ToolStripMenuItem_Click
        private void file4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(docList[0]))
            { mainForm ce = createDoc(docList[4]); readRegKeys(); ce.openDoc(docList[4]); ce.saved = true; writeRegKeys(docList[4]); }
        }
        #endregion
        #region toolStripButtonSetGoal_Click
        private void toolStripButtonSetGoal_Click(object sender, EventArgs e) { mainForm sv = (mainForm)(this.ActiveMdiChild); if (sv != null) sv.setGoal1(); }
        #endregion
        #region createDoc
        /// <summary>
        /// Creates new document
        /// </summary>
        /// <returns>New document</returns>
        public mainForm createDoc()
        {
            je++;
            mainForm f = new mainForm();
            f.MdiParent = this;
            f.Text = String.Concat("GoAvoid-", je.ToString());
            f.WindowState = FormWindowState.Maximized;
            docCount++;
            f.frameForm1 = this;
            f.Show();
            if (toolStripButtonFixedSize.Checked) { f.boolFixedSize = true; f.fixedSizeObstacleToolStripMenuItem.Checked = true; }
            else { { f.boolFixedSize = false; f.fixedSizeObstacleToolStripMenuItem.Checked = false; } }
            if (toolStripButtonStopObstacle.Checked) { f.boolStopObstacle = true; f.stoppedObstacleToolStripMenuItem.Checked = true; }
            else { f.boolStopObstacle = false; f.stoppedObstacleToolStripMenuItem.Checked = false; }
            if (toolStripButtonDrawGrid.Checked) { f.drawGrid = true; f.gridOnToolStripMenuItem.Checked = true; }
            else { f.drawGrid = false; f.gridOnToolStripMenuItem.Checked = false; }
            if (toolStripButtonSnapToGrid.Checked) { f.ob.isSnapToGrid = true; f.snapToGridToolStripMenuItem.Checked = true; }
            else { f.ob.isSnapToGrid = false; f.snapToGridToolStripMenuItem.Checked = false; }
            if (toolStripButtonDrawBitmapField.Checked) { f.boolDrawBitmapField = true; f.BitmapFieldtoolStripMenuItem.Checked = true; }
            else { f.boolDrawBitmapField = false; f.BitmapFieldtoolStripMenuItem.Checked = false; }
            return f;         
        }
        public mainForm createDoc(String s)
        {
            mainForm f = new mainForm();
            f.MdiParent = this;
            f.Text = s;
            f.fileNameN = s;
            f.WindowState = FormWindowState.Maximized;
            docCount++;
            f.frameForm1 = this;
            f.Show();
            return f;
        }
        #endregion
        #region open
        /// <summary>
        /// Opens document
        /// </summary>
        private void open()
        {
            i++; //to register files for lates file list
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "GoAvoid Files (*.goav)|*.goav|All Files (*.*)|*.*"; openDlg.FileName = "";
            openDlg.DefaultExt = ".goav"; openDlg.CheckFileExists = true; openDlg.CheckPathExists = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                if (!(openDlg.FileName).EndsWith(".goav"))
                    MessageBox.Show("Unexpected file format", "GoAvoid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    writeRegKeys(openDlg.FileName);	
                    mainForm ce = createDoc(openDlg.FileName);
                    ce.saved = true;
                    ce.openDoc(openDlg.FileName);
                }
            }
        }
        #endregion
        #region save
        /// <summary>
        /// Saves document
        /// </summary>
        private void save()
        {
            try
            {
                mainForm sv = (mainForm)(this.ActiveMdiChild);
                if (!sv.saved)
                {
                    SaveFileDialog sd = new SaveFileDialog();
                    sd.Filter = "GoAvoid Files (*.goav)|*.goav|All Files (*.*)|*.*"; sd.DefaultExt = ".goav"; sd.FileName = "GoAv01.goav";
                    sd.SupportMultiDottedExtensions = true;
                    if (sd.ShowDialog() == DialogResult.OK)
                    {
                        if (sv != null)
                        {
                            sv.saveDoc(sd.FileName); sv.Text = sd.FileName; sv.fileNameN = sd.FileName;
                            docList[i] = sd.FileName; writeRegKeys(sd.FileName);
                        }
                    }
                    sv.saved = true;
                }
                else { if (sv != null) {
                    sv.saveDoc(sv.fileNameN); docList[i] = sv.fileNameN; writeRegKeys(sv.fileNameN); } }
            }
            catch (Exception) { }
        }
        #endregion
        #region writeRegKeys
        /// <summary>
        /// Writes key values to register
        /// </summary>
        /// <param name="s">Key value</param>
        public void writeRegKeys(String s)
        {
            GoAvoidFiles = Registry.CurrentUser.CreateSubKey("GoAvoidFiles");
            String[] se = new String[6];
            for (int j = 0; j < 6; j++) se[j] = ""; int k = 5;
            for (int j = 0; j < 5; j++)
            {
                if (String.Compare(docList[j], s) == 0)
                    k = j;
            }
            for (int j = k; j < 5; j++) docList[j] = docList[j + 1];
            for (int j = 0; j < 5; j++) se[j + 1] = docList[j];
            se[0] = s;
            GoAvoidFiles.SetValue("file0", se[0]);
            GoAvoidFiles.SetValue("file1", se[1]);
            GoAvoidFiles.SetValue("file2", se[2]);
            GoAvoidFiles.SetValue("file3", se[3]);
            GoAvoidFiles.SetValue("file4", se[4]);
            GoAvoidFiles.Close();
            readRegKeys();
        }
        #endregion
        #region readRegKeys
        /// <summary>
        /// Reads key values to register
        /// </summary>
        public void readRegKeys()
        {
            for (int j = 0; j < 5; j++) docList[j] = "";
            RegistryKey GoAvoidFiles = Registry.CurrentUser;
            String[] subKeyNames = new String[GoAvoidFiles.GetSubKeyNames().Length];
            subKeyNames = GoAvoidFiles.GetSubKeyNames();
            for (int i = 0; i < subKeyNames.Length; i++)
            {
                RegistryKey tempKey = GoAvoidFiles.OpenSubKey(subKeyNames[i]);
                if (String.Compare(subKeyNames[i], "GoAvoidFiles") == 0)
                {
                    String[] valueNames = new String[GoAvoidFiles.GetValueNames().Length];
                    valueNames = tempKey.GetValueNames();
                    if (tempKey.GetValue("file0") != null) { file0ToolStripMenuItem.Text = tempKey.GetValue("file0").ToString(); file0ToolStripMenuItem.Visible = true; docList[0] = file0ToolStripMenuItem.Text; }
                    if (tempKey.GetValue("file1") != null) { file1ToolStripMenuItem.Text = tempKey.GetValue("file1").ToString(); file1ToolStripMenuItem.Visible = true; docList[1] = file1ToolStripMenuItem.Text; }
                    if (tempKey.GetValue("file2") != null) { file2ToolStripMenuItem.Text = tempKey.GetValue("file2").ToString(); file2ToolStripMenuItem.Visible = true; docList[2] = file2ToolStripMenuItem.Text; }
                    if (tempKey.GetValue("file3") != null) { file3ToolStripMenuItem.Text = tempKey.GetValue("file3").ToString(); file3ToolStripMenuItem.Visible = true; docList[3] = file3ToolStripMenuItem.Text; }
                    if (tempKey.GetValue("file4") != null) { file4ToolStripMenuItem.Text = tempKey.GetValue("file4").ToString(); file4ToolStripMenuItem.Visible = true; docList[4] = file4ToolStripMenuItem.Text; }
                }
            }
        }
        #endregion
        #region timerOpenDoc_Tick
        private void timerOpenDoc_Tick(object sender, EventArgs e)
        {
            timerOpenDoc.Stop();
            latestDocOpen();
        }
        #endregion
        #region latestDocOpen
        /// <summary>
        /// Opens the latest document
        /// </summary>
        private void latestDocOpen()
        {
            readRegKeys();
            mainForm ce = createDoc(docList[0]);
            ce.saved = true;
            ce.openDoc(docList[0]);
            writeRegKeys(docList[0]);
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            toolStripButtonFixedSize.Checked = sv.boolFixedSize;
            toolStripButtonStopObstacle.Checked = sv.boolStopObstacle;
            sv.fixedSizeObstacleToolStripMenuItem.Checked = sv.boolFixedSize;
            sv.stoppedObstacleToolStripMenuItem.Checked = sv.boolStopObstacle;
        }
        #endregion
        #region latestDocOpenWithName
        /// <summary>
        /// Opens the latest document by using its name
        /// </summary>
        /// <param name="name">Name of the document</param>
        public void latestDocOpen(string name)
        {
            readRegKeys();
            mainForm ce = createDoc(name);
            ce.saved = true;
            ce.openDoc(name);
            writeRegKeys(name);
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            toolStripButtonFixedSize.Checked = sv.boolFixedSize;
            toolStripButtonStopObstacle.Checked = sv.boolStopObstacle;
            sv.fixedSizeObstacleToolStripMenuItem.Checked = sv.boolFixedSize;
            sv.stoppedObstacleToolStripMenuItem.Checked = sv.boolStopObstacle;
        }
        #endregion
        #region coorInframeForm
        /// <summary>
        /// Shows coordinates on the toolstriplabel at the right bottom of the screen
        /// </summary>
        /// <param name="x">x coordinate of the mouse with respect to the new coordinate system</param>
        /// <param name="y">y coordinate of the mouse with respect to the new coordinate system</param>
        public void coorInframeForm(double x, double y) { toolStripStatusLabelCoor.Text = String.Format("x={0:f0} y={1:f0}", x, y); }
        #endregion
        #region exitToolStripMenuItem_Click
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) { this.Close(); }
        #endregion
        #region toolStripButtonFixedSize_Click
        private void toolStripButtonFixedSize_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if (sv != null)
            {
                sv.boolFixedSize = !sv.boolFixedSize;
                if (sv.boolFixedSize)
                {
                    toolStripButtonFixedSize.Checked = true;
                    sv.fixedSizeObstacleToolStripMenuItem.Checked = true;
                    string ToolTipTitleAsHint = "Fixed Size Obstacle Mode Is ON";
                    string ToolTipHint = "Draws a fixed size rectangle shaped obstacle by clicking the mouse left button";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonFixedSize, 180, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                else
                {
                    toolStripButtonFixedSize.Checked = false;
                    sv.fixedSizeObstacleToolStripMenuItem.Checked = false;
                    string ToolTipTitleAsHint = "Fixed Size Obstacle Mode Is OFF";
                    string ToolTipHint = "Draws a rectangle shaped obstacle with mouse movement";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonFixedSize, 180, 66, ToolTipTitleAsHint, ToolTipHint);
                }
            }
        }
        #endregion
        #region toolStripButtonStopObstacle_Click
        private void toolStripButtonStopObstacle_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if (sv!=null)
            {
                sv.boolStopObstacle = !sv.boolStopObstacle;
                if (sv.boolStopObstacle)
                {
                    toolStripButtonStopObstacle.Checked = true;
                    sv.stoppedObstacleToolStripMenuItem.Checked = true;
                    string ToolTipTitleAsHint = "Stopped Obstacle Mode Is ON";
                    string ToolTipHint = "Stops obstacle when it reaches to the boundaries";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 240, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                else
                {
                    toolStripButtonStopObstacle.Checked = false;
                    sv.stoppedObstacleToolStripMenuItem.Checked = false;
                    string ToolTipTitleAsHint = "Stopped Obstacle Mode Is OFF";
                    string ToolTipHint = "Returns obstacle when it reaches to the boundaries";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 240, 66, ToolTipTitleAsHint, ToolTipHint);
                } 
            }
        }
        #endregion
        #region toolStripButtonDrawGrid_Click
        private void toolStripButtonDrawGrid_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if (sv!=null)
            {
                sv.drawGrid = !sv.drawGrid;
                if (sv.drawGrid)
                {
                    toolStripButtonDrawGrid.Checked = true;
                    toolStripButtonDrawGrid.Text = "grid on";
                    sv.gridOnToolStripMenuItem.Checked = true;
                    sv.ob.getObstaclesForPF();
                    string ToolTipTitleAsHint = "Draw Grid Mode Is ON";
                    string ToolTipHint = "Draws grid lines in the workspace";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawGrid, 300, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                else
                {
                    toolStripButtonDrawGrid.Checked = false;
                    toolStripButtonDrawGrid.Text = "grid off";
                    sv.gridOnToolStripMenuItem.Checked = false;
                    string ToolTipTitleAsHint = "Draw Grid Mode Is OFF";
                    string ToolTipHint = "Grid lines are not being drawn";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawGrid, 300, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                sv.Invalidate(); 
            }
        }
        #endregion
        #region toolStripButtonChooseMouse_Click
        private void toolStripButtonChooseMouse_Click(object sender, EventArgs e)
        {
            toolStripButtonDrawObstacle.Checked = false;
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if(sv!=null) sv.mouse();
        }
        #endregion
        #region toolStripButtonDrawObstacle_Click
        private void toolStripButtonDrawObstacle_Click(object sender, EventArgs e)
        {
            toolStripButtonChooseMouse.Checked = false;
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if(sv!=null) sv.rect();
        }
        #endregion
        #region toolStripButtonSetStartPointOfPath_Click
        private void toolStripButtonSetStartPointOfPath_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild); if (sv != null) sv.setStartPointOfPath1();
        }
        #endregion
        #region toolStripButtonSnapToGrid_Click
        private void toolStripButtonSnapToGrid_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if (sv!=null)
            {
                sv.ob.isSnapToGrid = !sv.ob.isSnapToGrid;

                if (sv.ob.isSnapToGrid)
                {
                    toolStripButtonSnapToGrid.Checked = true;
                    toolStripButtonSnapToGrid.Text = "snap to grid on";
                    sv.snapToGridToolStripMenuItem.Checked = true;
                    sv.ob.getObstaclesForPF();
                    string ToolTipTitleAsHint = "Snap to Grid Mode Is ON";
                    string ToolTipHint = "Snaps the drawn items to the grid";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonSnapToGrid, 380, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                else
                {
                    toolStripButtonSnapToGrid.Checked = false;
                    toolStripButtonSnapToGrid.Text = "snap to grid off";
                    sv.snapToGridToolStripMenuItem.Checked = false;
                    string ToolTipTitleAsHint = "Snap to Grid Mode Is OFF";
                    string ToolTipHint = "Drawn items are not being snapped to the grid";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonSnapToGrid, 380, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                sv.Invalidate(); 
            }
        }
        #endregion
        #region toolStripButtonDrawBitmapField_Click
        private void toolStripButtonDrawBitmapField_Click(object sender, EventArgs e)
        {
            mainForm sv = (mainForm)(this.ActiveMdiChild);
            if (sv!=null)
            {
                sv.boolDrawBitmapField = !sv.boolDrawBitmapField;

                if (sv.boolDrawBitmapField)
                {
                    toolStripButtonDrawBitmapField.Checked = true;
                    toolStripButtonDrawBitmapField.Text = "bitmap on";
                    sv.BitmapFieldtoolStripMenuItem.Checked = true;
                    string ToolTipTitleAsHint = "Draw Bitmap Field Mode Is ON";
                    string ToolTipHint = "Draws bitmap field according to the potential field calculations";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawBitmapField, 450, 66, ToolTipTitleAsHint, ToolTipHint);
                    sv.calculateFieldToDrawBitmapField();
                }
                else
                {
                    toolStripButtonDrawBitmapField.Checked = false;
                    toolStripButtonDrawBitmapField.Text = "bitmap off";
                    sv.BitmapFieldtoolStripMenuItem.Checked = false;
                    string ToolTipTitleAsHint = "Draw Bitmap Field Mode Is OFF";
                    string ToolTipHint = "Bitmap field is not being drawn";
                    ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawBitmapField, 450, 66, ToolTipTitleAsHint, ToolTipHint);
                }
                sv.Invalidate(); 
            }
        } 
        #endregion
        #region ShowToolTipAsBalloonForToolStripButton
        private void ShowToolTipAsBalloonForToolStripButton(ToolStripButton tsb, int BalloonTipStartX, int BalloonTipStartY, string ToolTipTitleAsHint, string ToolTipHint)
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
            if (boolShowBalloonTips) toolTipAsHint.Show(ToolTipHint, this, BalloonTipStartX, BalloonTipStartY, 2000);
            else toolTipAsHint.RemoveAll();
        }
        #endregion
        #region toolStripButtonNew_MouseHover
        private void toolStripButtonNew_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "New File";
            string ToolTipHint = "Creates a new file";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonNew, 0, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonOpen_MouseHover
        private void toolStripButtonOpen_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Open File";
            string ToolTipHint = "Opens a saved file";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonOpen, 15, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonSave_MouseHover
        private void toolStripButtonSave_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Save File";
            string ToolTipHint = "Saves a file";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonSave, 40, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonSetStartPointOfPath_MouseHover
        private void toolStripButtonSetStartPointOfPath_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Set Start Point";
            string ToolTipHint = "Sets start point of path";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonSetStartPointOfPath, 65, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonSetGoal_MouseHover
        private void toolStripButtonSetGoal_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Set Goal Point";
            string ToolTipHint = "Sets goal";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonSetGoal, 85, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonChooseMouse_MouseHover
        private void toolStripButtonChooseMouse_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Choose Mouse";
            string ToolTipHint = "Selects arrow";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonSetGoal, 110, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonDrawObstacle_MouseHover
        private void toolStripButtonDrawObstacle_MouseHover(object sender, EventArgs e)
        {
            string ToolTipTitleAsHint = "Draw Obstacle";
            string ToolTipHint = "Draws a rectangle shaped obstacle";
            ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawObstacle, 130, 66, ToolTipTitleAsHint, ToolTipHint);
        }
        #endregion
        #region toolStripButtonFixedSize_MouseHover
        private void toolStripButtonFixedSize_MouseHover(object sender, EventArgs e)
        {
            if (toolStripButtonFixedSize.Checked)
            {
                string ToolTipTitleAsHint = "Fixed Size Obstacle Mode Is ON";
                string ToolTipHint = "Draws a fixed size rectangle shaped obstacle by clicking the mouse left button";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonFixedSize, 180, 66, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Fixed Size Obstacle Mode Is OFF";
                string ToolTipHint = "Draws a rectangle shaped obstacle with mouse movement";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonFixedSize, 180, 66, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region toolStripButtonStopObstacle_MouseHover
        private void toolStripButtonStopObstacle_MouseHover(object sender, EventArgs e)
        {
            if (toolStripButtonStopObstacle.Checked)
            {
                string ToolTipTitleAsHint = "Stopped Obstacle Mode Is ON";
                string ToolTipHint = "Stops obstacle when it reaches to the boundaries";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 240, 66, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Stopped Obstacle Mode Is OFF";
                string ToolTipHint = "Returns obstacle when it reaches to the boundaries";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 240, 66, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region toolStripButtonDrawGrid_MouseHover
        private void toolStripButtonDrawGrid_MouseHover(object sender, EventArgs e)
        {
            if (toolStripButtonDrawGrid.Checked)
            {
                string ToolTipTitleAsHint = "Draw Grid Mode Is ON";
                string ToolTipHint = "Draws grid lines in the workspace";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 300, 66, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Draw Grid Mode Is OFF";
                string ToolTipHint = "Grid lines are not being drawn";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonStopObstacle, 280, 66, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region toolStripButtonDrawBitmapField_MouseHover
        private void toolStripButtonDrawBitmapField_MouseHover(object sender, EventArgs e)
        {
            if (toolStripButtonDrawBitmapField.Checked)
            {
                string ToolTipTitleAsHint = "Draw Bitmap Field Mode Is ON";
                string ToolTipHint = "Draws bitmap field according to the potential field calculations";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawBitmapField, 450, 66, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Draw Bitmap Field Mode Is OFF";
                string ToolTipHint = "Bitmap field is not being drawn ";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonDrawBitmapField, 450, 66, ToolTipTitleAsHint, ToolTipHint);
            }
        } 
        #endregion
        #region toolStripButtonSnapToGrid_MouseHover
        private void toolStripButtonSnapToGrid_MouseHover(object sender, EventArgs e)
        {
            if (toolStripButtonSnapToGrid.Checked)
            {
                string ToolTipTitleAsHint = "Snap to Grid Mode Is ON";
                string ToolTipHint = "Snaps the drawn items to the grid";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonSnapToGrid, 380, 66, ToolTipTitleAsHint, ToolTipHint);
            }
            else
            {
                string ToolTipTitleAsHint = "Snap to Grid Mode Is OFF";
                string ToolTipHint = "Drawn items are not being snapped to the grid";
                ShowToolTipAsBalloonForToolStripButton(toolStripButtonSnapToGrid, 380, 66, ToolTipTitleAsHint, ToolTipHint);
            }
        }
        #endregion
        #region menuStrip1_ItemAdded
        private void menuStrip1_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            if (e.Item.Text == "") e.Item.Visible = false;//this code is for making the menustrip icon unvisible 
        }
        #endregion
        #region goAv01ToolStripMenuItem_Click
        private void goAv01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//GoAv01.goav");
        } 
        #endregion
        #region goAv02ToolStripMenuItem_Click
        private void goAv02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//GoAv02.goav");
        } 
        #endregion
        #region goAv03ToolStripMenuItem_Click
        private void goAv03ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//GoAv03.goav");
        } 
        #endregion
        #region goAv04ToolStripMenuItem_Click
        private void goAv04ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//GoAv04.goav");
        } 
        #endregion
        #region goAv05ToolStripMenuItem_Click
        private void goAv05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//GoAv05.goav");
        } 
        #endregion
        #region Implement01ToolStripMenuItem_Click
        private void Implement01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//Implement01.goav");
        }
        #endregion
        #region Implement02ToolStripMenuItem_Click
        private void Implement02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            latestDocOpen("samples//Implement02.goav");
        } 
        #endregion
    }
}
