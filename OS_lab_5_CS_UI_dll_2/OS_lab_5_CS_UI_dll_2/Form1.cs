using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OS_lab_3_CS_UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] files = Directory.GetFiles(@"C:\");
        private string[] folders = Directory.GetDirectories(@"C:\");

        // critical section variables
        private Object fileCS = new Object();
        private Mutex folderCS = new Mutex();

        delegate void SetTextCallback(string text);
        delegate void SetUpdateProcess(int value);
        delegate void SetValueProcess(int min, int max, int val);
        delegate void SetButton1EnabledDelegate(bool isEnabled);

        List<Thread> T = new List<Thread>();

        /*
         Functions
        */

        public void SetText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.Text += text;
            }
        }

        public void IncrementProgressBar(int value)
        {
            if (this.progressBar1.InvokeRequired)
            {
                SetUpdateProcess d = new SetUpdateProcess(IncrementProgressBar);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.progressBar1.Increment(value);
                this.progressBar1.Update();
            }
        }

        public void UpdateProgressBar(int min, int max, int val)
        {
            if (this.progressBar1.InvokeRequired)
            {
                SetValueProcess d = new SetValueProcess(UpdateProgressBar);
                this.Invoke(d, new object[] { min, max, val });
            }
            else
            {
                this.progressBar1.Minimum = min;
                this.progressBar1.Maximum = max;
                this.progressBar1.Value = val;
                this.progressBar1.Update();
            }
        }

        public void SetButton1Enabled(bool isEnabled)
        {
            if (this.button1.InvokeRequired)
            {
                SetButton1EnabledDelegate d = new SetButton1EnabledDelegate(SetButton1Enabled);
                this.Invoke(d, new object[] { isEnabled });
            }
            else
            {
                this.button1.Enabled = isEnabled;
            }
        }

        public void PrintResults()
        {
            SetButton1Enabled(false);
            var folderslineCount = File.ReadLines(@"folders.txt").Count();
            var fileslineCount = File.ReadLines(@"files.txt").Count();
            SetText("Folders:\n");
            bool isExist = false;
            string sFilesName = "folders.txt";
            UpdateProgressBar(0, folderslineCount + fileslineCount, 0);
            lock (fileCS)
            {
                folderCS.WaitOne();
                if (File.Exists(sFilesName))
                {
                    if (!isExist)
                        isExist = true;
                    StreamReader sr = new StreamReader(sFilesName);
                    string line = sr.ReadLine();

                    //Continue to read until you reach end of file
                    while (line != null)
                    {
                        SetText(line + '\n');
                        IncrementProgressBar(1);
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }

                SetText("\nFiles:\n");
                sFilesName = "files.txt";
                if (File.Exists(sFilesName))
                {
                    if (!isExist)
                        isExist = true;
                    StreamReader sr = new StreamReader(sFilesName);
                    string line = sr.ReadLine();

                    //Continue to read until you reach end of file
                    while (line != null)
                    {
                        SetText(line + '\n');
                        IncrementProgressBar(1);
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
                folderCS.ReleaseMutex();
            }
            if (!isExist)
                MessageBox.Show("Please, firstly start searching by");
            SetButton1Enabled(true);
        }

        // Function that searching files 
        public List<string> SearchFiles(string[] files, string sFileName)
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

            return SearchedFiles;
        }

        // Function that searching folders 
        public List<string> SearchFolders(string[] folders, string sFileName)
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
            return SearchedFolders;
        }


        public void Button1_Click(object sender, System.EventArgs e)
        {
            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Directory path you've written isn't exist");
                return;
            }
            SetButton1Enabled(false);
            progressBar1.Value = 0;
            progressBar1.Update();
            files = Directory.GetFiles(textBox2.Text);
            folders = Directory.GetDirectories(textBox2.Text);
            lock (fileCS)
            {
                folderCS.WaitOne();
                System.IO.StreamWriter fileT = new System.IO.StreamWriter("files.txt", false);
                fileT.Close();
                System.IO.StreamWriter fileF = new System.IO.StreamWriter("folders.txt", false);
                fileF.Close();
                folderCS.ReleaseMutex();
            }


            // Getting template file name
            string sFileName = textBox1.Text;
            if (sFileName == "")
                sFileName = "*";

            // Clearing filepath from files strings
            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = files[i].Remove(0, textBox2.Text.Length);
                for (int j = 0; files[i][j] == '\\'; ++j)
                    files[i] = files[i].Remove(0, 1);
            }
            // Clearing filepath from folders strings
            for (int i = 0; i < folders.Length; ++i)
            {
                folders[i] = folders[i].Remove(0, textBox2.Text.Length);
                for (int j = 0; folders[i][j] == '\\'; ++j)
                    folders[i] = folders[i].Remove(0, 1);
            }

            // Threads count
            int iCountOfFilesThreads = int.Parse(comboBox1.SelectedItem.ToString()) / 2;
            int iCountOfFoldersThreads = int.Parse(comboBox1.SelectedItem.ToString()) / 2;
            while (folders.Length < iCountOfFoldersThreads)
                --iCountOfFoldersThreads;
            while (files.Length < iCountOfFilesThreads)
                --iCountOfFilesThreads;

            // progressBar Max Value

            progressBar1.Maximum = files.Length + folders.Length;
            progressBar1.Update();

            T.Clear();
            // Creating files and threads for folders
            for (int i = 0; i < iCountOfFoldersThreads; ++i)
            {
                // Creating files
                string sFilesWrite = "Folders";
                sFilesWrite += comboBox1.SelectedItem.ToString() + "-";
                sFilesWrite += i.ToString() + ".txt";
                string[] folderstemp;
                int iDec = folders.Length / iCountOfFoldersThreads;
                if (folders.Length % iCountOfFoldersThreads != 0 && i == iCountOfFoldersThreads - 1)
                {
                    folderstemp = new string[iDec + folders.Length % iCountOfFoldersThreads];
                    Array.Copy(folders, i * iDec, folderstemp, 0, iDec + folders.Length % iCountOfFoldersThreads);
                }
                else
                {
                    folderstemp = new string[iDec];
                    Array.Copy(folders, i * iDec, folderstemp, 0, iDec);
                }

                // Starting threads
                T.Add(new Thread(() => SearchFolders(folderstemp, sFileName)));
                T[T.Count - 1].Start();
            }

            // Creating files and threads for files
            for (int i = 0; i < iCountOfFilesThreads; ++i)
            {
                // Creating files
                string sFilesWrite = "Files";
                sFilesWrite += comboBox1.SelectedItem.ToString() + "-";
                sFilesWrite += i.ToString() + ".txt";
                string[] filestemp;
                int iDec = files.Length / iCountOfFilesThreads;
                if (files.Length % iCountOfFilesThreads != 0 && i == iCountOfFilesThreads - 1)
                {
                    filestemp = new string[iDec + files.Length % iCountOfFilesThreads];
                    Array.Copy(files, i * iDec, filestemp, 0, iDec + files.Length % iCountOfFilesThreads);
                }
                else
                {
                    filestemp = new string[iDec];
                    Array.Copy(files, i * iDec, filestemp, 0, iDec);
                }

                // Starting threads
                T.Add(new Thread(() => SearchFiles(filestemp, sFileName)));
                T[T.Count - 1].Start();
            }

            foreach (var thread in T)
            {
                thread.Join();
            }

            richTextBox1.Clear();
            Thread PRT = new Thread(PrintResults);
            PRT.Start();
        }




        // Protection from incorrect template
        public void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                if (textBox1.Text[0] == '.')
                {
                    textBox1.Text = textBox1.Text.Remove(0, 1);
                }
                int iStarPos = 0;
                bool isStar = false;
                for (int i = 0; i < textBox1.Text.Length; ++i)
                {
                    if (textBox1.Text[i] == '*')
                    {
                        iStarPos = i;
                        isStar = true;
                        break;
                    }
                }
                if (isStar && textBox1.Text.Length - 1 > iStarPos)
                {
                    textBox1.Text = textBox1.Text.Remove(iStarPos + 1);
                }
            }
        }
        public void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '/' &&
                e.KeyChar != '\\' &&
                e.KeyChar != ':' &&
                e.KeyChar != '?' &&
                e.KeyChar != '"' &&
                e.KeyChar != '<' &&
                e.KeyChar != '>' &&
                e.KeyChar != '|')
                return;
            else
                e.Handled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
        }
    }
}