using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace OS_lab5_dll
{
    public static class Class1
    {
        // critical section variables
        private static Object fileCS = new Object();
        private static Mutex folderCS = new Mutex();

        // Function that searching files 
        [DllExport("SearchFiles", CallingConvention = CallingConvention.StdCall)]
        public static void SearchFiles(string[] files, string sFileName)
        {
            List<string> SearchedFiles = new List<string>();

            if (sFileName[sFileName.Length - 1] == '*')
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    bool isIdentity = true;
                    for (int j = 0; j < sFileName.Length; ++j)
                    {
                        if (sFileName[j] == '*')
                            break;
                        if (sFileName[j] != files[i][j])
                        {
                            isIdentity = false;
                            break;
                        }
                    }
                    if (isIdentity)
                        SearchedFiles.Add(files[i]);
                }
            }

            else
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    if (files[i].Length == sFileName.Length)
                    {
                        bool isIdentity = true;
                        for (int j = 0; j < sFileName.Length; ++j)
                        {
                            if (sFileName[j] != files[i][j])
                            {
                                isIdentity = false;
                                break;
                            }
                        }
                        if (isIdentity)
                            SearchedFiles.Add(files[i]);
                    }
                }
            }
            lock (fileCS)
            {
                for (int i = 0; i < SearchedFiles.Count; ++i)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("files.txt", true))
                    {
                        file.WriteLine(SearchedFiles[i].ToString());
                    }
                }
            }
        }

        // Function that searching folders 
        [DllExport("SearchFolders", CallingConvention = CallingConvention.StdCall)]
        public static void SearchFolders(string[] folders, string sFileName)
        {
            List<string> SearchedFolders = new List<string>();

            if (sFileName[sFileName.Length - 1] == '*')
            {
                for (int i = 0; i < folders.Length; ++i)
                {
                    bool isIdentity = true;
                    for (int j = 0; j < sFileName.Length; ++j)
                    {
                        if (sFileName[j] == '*')
                            break;
                        if (sFileName[j] != folders[i][j])
                        {
                            isIdentity = false;
                            break;
                        }
                    }
                    if (isIdentity)
                        SearchedFolders.Add(folders[i]);
                }
            }
            else
            {
                for (int i = 0; i < folders.Length; ++i)
                {
                    if (folders[i].Length == sFileName.Length)
                    {
                        bool isIdentity = true;
                        for (int j = 0; j < sFileName.Length; ++j)
                        {
                            if (sFileName[j] != folders[i][j])
                            {
                                isIdentity = false;
                                break;
                            }
                        }
                        if (isIdentity)
                            SearchedFolders.Add(folders[i]);
                    }
                }
            }
            folderCS.WaitOne();
            {
                for (int i = 0; i < SearchedFolders.Count; ++i)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("folders.txt", true))
                    {
                        file.WriteLine(SearchedFolders[i].ToString());
                    }
                }
            }
            folderCS.ReleaseMutex();
        }
    }
}
