//licHeader
//===============================================================================================================
// System  : Nistec.Queue - Nistec.Queue Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using Nistec.IO;
//using Nistec.Channels.RemoteCache;
using Nistec.Generic;
using Nistec.Serialization;
using System.Diagnostics;
using System.Linq;


namespace Nistec.Caching.Demo
{

      class Program
    {

          [STAThread]
          static void Main(string[] args)
          {
              Console.OutputEncoding = System.Text.Encoding.UTF8;
              Console.InputEncoding = System.Text.Encoding.UTF8;
              //Console.BackgroundColor = ConsoleColor.White;
              Console.ForegroundColor = ConsoleColor.Yellow;
              Console.WindowHeight =(int) (Console.LargestWindowHeight*0.70);
              Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.70);
              Console.Title = "Nistec cache console";
              
              

              Console.WriteLine("Welcome to: Nistec Cache commander...");
              Console.WriteLine("=====================================");
              Controller.Run(args);
              Console.WriteLine("Finished...");
              Console.ReadLine();

          }

         
    }
}
