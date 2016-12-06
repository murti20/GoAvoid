
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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GoAvoid
{
    public partial class help : Form
    {
        public help()
        {
            InitializeComponent();
        }
        #region help_FormClosing
        private void help_FormClosing(object sender, FormClosingEventArgs e)
        {
            axAcroPDF1.Dispose();
        } 
        #endregion
    }
}
