using System;
using EFlocalDB;
using System.ComponentModel;
using Microsoft.Win32;

namespace SimatekExCnahger
{
    public class FileWorks: Entity

    {


        private string _rootdir;
        public string rootdir
        {
            get { return _rootdir; }
            set
            {
                _rootdir = value;
                OnPropertyChanged(new PropertyChangedEventArgs("rootdir"));
            }
        }

 
        public FileWorks()
        {
            rootdir = AppDomain.CurrentDomain.BaseDirectory;
        }


        public string GetSaveFilePath()
        {
            return SaveFileDialogResult();
        }

        public string GetLoadFilePath()
        {
            return OpenFileDialogResult();
        }

        private string SaveFileDialogResult()
        {
            string dir = rootdir;
            string filename = "";

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.InitialDirectory = dir;
            saveDlg.Filter = "Database files (*.mdf)|*.mdf;|All Files (*.*)|*.*";

            // Set filter for file extension and default file extension
            saveDlg.DefaultExt = ".mdf";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = saveDlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string safename = saveDlg.SafeFileName;
                filename = saveDlg.FileName;

                dir = filename.Remove(filename.Length - safename.Length - 1);

            }

            return filename;
        }

        private string OpenFileDialogResult()
        {
            string dir = rootdir;
            string filename = "";

            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.InitialDirectory = dir;
            openDlg.Filter = "Database files (*.mdf)|*.mdf;|All Files (*.*)|*.*";

            // Set filter for file extension and default file extension
            openDlg.DefaultExt = ".mdf";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = openDlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string safename = openDlg.SafeFileName;
                filename = openDlg.FileName;

                dir = filename.Remove(filename.Length - safename.Length - 1);

            }

            return filename;
        }

    }
}
