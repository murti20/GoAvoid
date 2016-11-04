
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
using System.Text;

namespace GoAvoid{
    [Serializable] 
    public class points //:  IComparable 
	{
        #region var
        public double x, y, pA, pL, xf, yf; //coordinates of point (x,y), angle of the point pA, length from the center pL, coordinates of intersection path point (xf,yf)
        #endregion
        #region constructor
        public points() { } 
        #endregion
        #region constructor
        public points(double x, double y, double pA, double pL)
        {
            this.x = x;
            this.y = y;
            this.pA = pA;
            this.pL = pL;
        }  
        #endregion
    }
}
