﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Nistec.Generic;
using Nistec.Logging;

namespace Nistec.Messaging.Io
{
    public class SysIO
    {

        public static string EnsureQueueFilename(string QueuePath,string identifier)
        {
            string path = Assists.EnsureIdentifierPath(QueuePath, identifier);
            return Path.Combine(path, Assists.FormatQueueFilename(identifier));
        }

        public static void WriteToFile(string QueuePath, IQueueMessage message)
        {
            string filename = EnsureQueueFilename(QueuePath, message.Identifier);

            var stream = message.ToStream();
            if (stream == null)
            {
                throw new Exception("Invalid BodyStream , Can't save body stream to file,");
            }
            stream.Copy().SaveToFile(filename);
        }



        #region static method NormalizeFolder

        /// <summary>
        /// Normalizes folder value. Replaces \ to /, removes duplicate //, removes / from folder start and end.
        /// </summary>
        /// <param name="folder">Folder to normalize.</param>
        /// <returns></returns>
        public static string NormalizeFolder(string folder)
        {
            // API uses only / as path separator.
            folder = folder.Replace("\\", "/");

            // Remove // duplicate continuos path separators, if any.
            while (folder.IndexOf("//") > -1)
            {
                folder = folder.Replace("//", "/");
            }

            // Remove from folder start, if any.
            if (folder.StartsWith("/"))
            {
                folder = folder.Substring(1);
            }

            // Remove from folder end, if any.
            if (folder.EndsWith("/"))
            {
                folder = folder.Substring(0, folder.Length - 1);
            }

            return folder;
        }

        #endregion

        #region static method PathFix

        /// <summary>
        /// Fixes path separator, replaces / \ with platform separator char.
        /// </summary>
        /// <param name="path">Path to fix.</param>
        /// <returns></returns>
        public static string PathFix(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        #endregion

        #region static method DirectoryExists

        /// <summary>
        /// Checks if directory exists. If linux, checks with case-insenstively (linux is case-sensitive). 
        /// Returns actual dir (In linux it may differ from requested directory, because of case-sensitivity.)
        /// or null if directory doesn't exist.
        /// </summary>
        /// <param name="dirName">Directory to check.</param>
        /// <returns></returns>
        public static string DirectoryExists(string dirName)
        {
            // Windows we can use Directory.Exists
            if (Environment.OSVersion.Platform.ToString().ToLower().IndexOf("win") > -1)
            {
                if (Directory.Exists(dirName))
                {
                    return dirName;
                }
            }
            // Unix,Linux we can't trust Directory.Exists value because of case-sensitive file system
            else
            {
                if (Directory.Exists(dirName))
                {
                    return dirName;
                }
                else
                {
                    // Remove / if path starts with /.
                    if (dirName.StartsWith("/"))
                    {
                        dirName = dirName.Substring(1);
                    }
                    // Remove / if path ends with /.
                    //if(dirName.EndsWith("/")){
                    //    dirName = dirName.Substring(0,dirName.Length - 1);
                    //}

                    string[] pathParts = dirName.Split('/');
                    string currentPath = "/";

                    // See if dirs path is valid
                    for (int i = 0; i < pathParts.Length; i++)
                    {
                        bool dirExists = false;
                        string[] dirs = Directory.GetDirectories(currentPath);
                        foreach (string dir in dirs)
                        {
                            string[] dirParts = dir.Split('/');
                            if (pathParts[i].ToLower() == dirParts[dirParts.Length - 1].ToLower())
                            {
                                currentPath = dir;
                                dirExists = true;
                                break;
                            }
                        }
                        if (!dirExists)
                        {
                            return null;
                        }
                    }

                    return currentPath;
                }
            }

            return null;
        }

        #endregion

        #region static method EnsureDirectory

        /// <summary>
        /// Ensures that specified folder exists, if not it will be created.
        /// Returns actual dir (In linux it may differ from requested directory, because of case-sensitivity.).
        /// </summary>
        /// <param name="folder">Folder name with path.</param>
        public static string EnsureFolder(string folder)
        {
            string normalizedFolder = DirectoryExists(folder);
            if (normalizedFolder == null)
            {
                Directory.CreateDirectory(folder);

                return folder;
            }
            else
            {
                return normalizedFolder;
            }
        }

        #endregion

        #region static method FileExists

        /// <summary>
        /// Checks if file exists. If linux, checks with case-insenstively (linux is case-sensitive). 
        /// Returns actual file (In linux it may differ from requested file, because of case-sensitivity.)
        /// or null if file doesn't exist.
        /// </summary>
        /// <param name="fileName">File to check.</param>
        /// <returns></returns>
        public static string FileExists(string fileName)
        {
            // Windows we can use File.Exists
            if (Environment.OSVersion.Platform.ToString().ToLower().IndexOf("win") > -1)
            {
                if (File.Exists(fileName))
                {
                    return fileName;
                }
            }
            // Unix,Linux we can't trust File.Exists value because of case-sensitive file system
            else
            {
                if (File.Exists(fileName))
                {
                    return fileName;
                }
                else
                {
                    // Remove / if path starts with /.
                    if (fileName.StartsWith("/"))
                    {
                        fileName = fileName.Substring(1);
                    }

                    string[] pathParts = fileName.Split('/');
                    string currentPath = "/";

                    // See if dirs path is valid
                    for (int i = 0; i < pathParts.Length - 1; i++)
                    {
                        bool dirExists = false;
                        string[] dirs = Directory.GetDirectories(currentPath);
                        foreach (string dir in dirs)
                        {
                            string[] dirParts = dir.Split('/');
                            if (pathParts[i].ToLower() == dirParts[dirParts.Length - 1].ToLower())
                            {
                                currentPath = dir;
                                dirExists = true;
                                break;
                            }
                        }
                        if (!dirExists)
                        {
                            return null;
                        }
                    }

                    // Check that file exists
                    string[] files = Directory.GetFiles(currentPath);
                    foreach (string file in files)
                    {
                        if (pathParts[pathParts.Length - 1].ToLower() == Path.GetFileName(file).ToLower())
                        {
                            return file;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region static method StreamCopy

        ///// <summary>
        ///// Copies all data from source stream to destination stream.
        ///// Copy starts from source stream current position and will be copied to the end of source stream.
        ///// </summary>
        ///// <param name="source">Source stream.</param>
        ///// <param name="destination">Destination stream.</param>
        ///// <returns>Returns number of bytes copied.</returns>
        //public static long StreamCopy(Stream source, Stream destination)
        //{
        //    byte[] buffer = new byte[16000];
        //    long totalReadedCount = 0;
        //    while (true)
        //    {
        //        int readedCount = source.Read(buffer, 0, buffer.Length);
        //        // End of stream reached.
        //        if (readedCount == 0)
        //        {
        //            return totalReadedCount;
        //        }

        //        totalReadedCount += readedCount;
        //        destination.Write(buffer, 0, readedCount);
        //    }

        //    //return totalReadedCount;
        //}


        #endregion
            
        #region Stream 

        /// <summary>
        /// Copies <b>source</b> stream data to <b>target</b> stream.
        /// </summary>
        /// <param name="source">Source stream. Reading starts from stream current position.</param>
        /// <param name="target">Target stream. Writing starts from stream current position.</param>
        /// <param name="blockSize">Specifies transfer block size in bytes.</param>
        /// <returns>Returns number of bytes copied.</returns>
        public static long StreamCopy(Stream source, Stream target, int blockSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (blockSize < 1024)
            {
                throw new ArgumentException("Argument 'blockSize' value must be >= 1024.");
            }

            byte[] buffer = new byte[blockSize];
            long totalReaded = 0;
            while (true)
            {
                int readedCount = source.Read(buffer, 0, buffer.Length);
                // We reached end of stream, we readed all data sucessfully.
                if (readedCount == 0)
                {
                    return totalReaded;
                }
                else
                {
                    target.Write(buffer, 0, readedCount);
                    totalReaded += readedCount;
                }
            }
        }

        /// <summary>
        /// Copies all data from source stream to destination stream.
        /// Copy starts from source stream current position and will be copied to the end of source stream.
        /// </summary>
        /// <param name="source">Source stream.</param>
        /// <param name="destination">Destination stream.</param>
        public static void StreamCopy(Stream source, Stream destination)
        {
            byte[] buffer = new byte[8000];
            int readedCount = source.Read(buffer, 0, buffer.Length);
            while (readedCount > 0)
            {
                destination.Write(buffer, 0, readedCount);

                readedCount = source.Read(buffer, 0, buffer.Length);
            }
        }

        //public static void StreamCopy(Stream source, Stream destination, int size)
        //{
        //    byte[] buffer = new byte[size];
        //    int readedCount = source.Read(buffer, 0, buffer.Length);
        //    while (readedCount > 0)
        //    {
        //        destination.Write(buffer, 0, readedCount);

        //        readedCount = source.Read(buffer, 0, buffer.Length);
        //    }
        //}

        public static void SaveFile(Stream message, string filename)
        {
            // Create relay message.
            using (FileStream fs = File.Create(filename))
            {
                StreamCopy(message, fs);

                // Create message info file for the specified relay message.
                //RelayMessageInfo messageInfo = new RelayMessageInfo(sender, to, date, false, targetHost);
                //File.WriteAllBytes(PathFix(path + "\\" + id + ".info"), messageInfo.ToByte());
            }
        }

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex) 
                {
                    Netlog.ErrorFormat("Error DeleteFile: {0}, {1} ", filename, ex.Message);
                }
            }
        }

        public static void MoveFile(string source, string dest)
        {
            try
            {
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }
                File.Move(source, dest);
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Error MoveFile: {0}, {1} ", dest, ex.Message);
            }
        }

        
        public static void RecursiveFileSearch(DirectoryInfo root, string search, Action<string> OnTake, Action<string> OnFault)
        {
            IEnumerable<FileInfo> files = null;
            IEnumerable <DirectoryInfo> subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.EnumerateFiles(search);
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                if (OnFault != null)
                    OnFault(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                if (OnFault != null)
                    OnFault(e.Message);

            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    OnTake(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.EnumerateDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    RecursiveFileSearch(dirInfo,search, OnTake, OnFault);
                }
            }
        }
        #endregion
    }
}
