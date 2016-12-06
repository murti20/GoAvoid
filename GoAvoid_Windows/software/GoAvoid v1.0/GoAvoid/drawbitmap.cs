
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


namespace GoAvoid
{
    unsafe class drawbitmap
    {
        #region var
        public Bitmap b1;
        System.Drawing.Imaging.BitmapData bData;
        public int bytes, bytes1;
        public byte[] rgbValues;
        public IntPtr ptr1;
        public int stride;
        public byte* ptr;
        public int remain;
        bool safecode = false;
        #endregion
        #region constructor
        public drawbitmap(Bitmap b1)
        {
            this.b1 = b1;
            bytes = b1.Width * b1.Height * 3;  // Declare an array to hold the bytes of the bitmap. This code is specific to a bitmap with 24 bits per pixels.
            rgbValues = new byte[bytes];
        } 
        #endregion
        #region lockbits
        /// <summary>
        /// Locks bits for safety
        /// </summary>
        /// <param name="safecode">Determines the code is safety or not</param>
        public void lockbits(bool safecode)
        {
            this.safecode = safecode;
            Rectangle rect = new Rectangle(0, 0, b1.Width, b1.Height);      // Lock the bitmap's bits. 
            bData = b1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, b1.PixelFormat);
            ptr1 = bData.Scan0;    // Get the address of the first line.
            ptr = (byte*)ptr1;
            stride = bData.Stride;
            remain = stride - b1.Width * 3;
            bytes1 = stride * b1.Height;
            rgbValues = new byte[bytes1];
            if (safecode) System.Runtime.InteropServices.Marshal.Copy(ptr1, rgbValues, 0, bytes1);   // Copy the RGB values into the array.
        } 
        #endregion
        #region unlockbits
        /// <summary>
        /// Unlocks bits for safety
        /// </summary>
        public void unlockbits()
        {
            if (safecode) System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr1, bytes1);   // Copy the RGB values back to the bitmap   
            b1.UnlockBits(bData);      // Unlock the bits.
        } 
        #endregion
        #region setpixel
        /// <summary>
        /// sets the pixel value according to RGB 
        /// </summary>
        /// <param name="red">Red pixel value 0 to 255</param>
        /// <param name="green">Green pixel value 0 to 255</param>
        /// <param name="blue">Blue pixel value 0 to 255</param>
        unsafe public void setpixel(int red, int green, int blue)
        {
            ptr[0] = Convert.ToByte(blue);
            ptr[1] = Convert.ToByte(green);
            ptr[2] = Convert.ToByte(red);
        } 
        #endregion
    }
}
