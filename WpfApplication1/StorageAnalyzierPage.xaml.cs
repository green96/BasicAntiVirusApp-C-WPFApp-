using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Threading;

// AZUL CODING ---------------------------------------
// WPF C#/VB - How to Analyse Storage on Your PC
// https://youtu.be/M-V2GKO7fh4

// Looking for an example app? Get Quota Express:
// https://johnjds.co.uk/quota?ref=azul

//package nuget.exe - Windows x86 Commandline
/*
Lưu nuget.exe vào:
C:\Users\david\OneDrive\Documents\Visual Studio 2013\Projects\WpfApplication1\WpfApplication1
Chạy PowerShell: .\nuget.exe install WindowsAPICodePack-Shell -Version 1.1.1 -OutputDirectory packages
Mở Solution Explorer.
Chuột phải vào References → Add Reference... → Browse.
Mở thư mục:
Tìm DLL trong thư mục lib và chọn Microsoft.WindowsAPICodePack.Shell.dll.
Làm tương tự với Core, chọn Microsoft.WindowsAPICodePack.dll.
Nhấn OK rồi chọn Build → Rebuild Solution.
 */

// Plugin: https://www.nuget.org/packages/WindowsAPICodePackShell
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for StorageAnalyzierPage.xaml
    /// </summary>
    public partial class StorageAnalyzierPage : Page
    {
        //This ObservableCollection Will contain the list of files and folders in the UI
        private readonly ObservableCollection<FileItem> FileDisplayList
            = new ObservableCollection<FileItem>();

        private CancellationTokenSource CancelTokenSource
            = new CancellationTokenSource();

        private Task FileSizeTask;
        private string CurrentRoot = "";

        // Browser Folder For Plugin (Khới tạo đối tượng từ 2 thư viện mới)
        private readonly CommonOpenFileDialog FolderBrowserDialog
            = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = false
            };

        public StorageAnalyzierPage()
        {
            InitializeComponent();

            FolderItems.ItemsSource = FileDisplayList;

            //Dùng Linq Select displayName + với DriveType hiện tại
            DriveCombo.ItemsSource = DriveInfo.GetDrives().Select(d =>
            {
                string displayName;

                switch (d.DriveType)
                {
                    case DriveType.Removable:
                        displayName = "USB Drive " + d.Name;
                        break;
                    case DriveType.Network:
                        displayName = "Network Drive " + d.Name;
                        break;
                    case DriveType.CDRom:
                        displayName = "CD Drive " + d.Name;
                        break;
                    default:
                        displayName = "Drive " + d.Name;
                        break;
                }

                return new ComboBoxItem()
                {
                    Content = displayName,
                    Tag = d.Name
                };
            }).ToList();

            if (DriveCombo.Items.Count > 0)
                DriveCombo.SelectedIndex = 0;

            // Load Downloads folder at startup when available
            try
            {
                //Load Thư Mục Downloads Mặc định khi bắt đầu app
                string downloadsPath = KnownFolders.Downloads.Path;
                if (!string.IsNullOrEmpty(downloadsPath) &&
                    Directory.Exists(downloadsPath))
                {
                    GetFiles(downloadsPath);
                }
            }
            catch
            {
                // Nếu không lấy được Downloads, người dùng có thể tự chọn thư mục.
            }
        }

        private void ClickBackToMain(object sender, RoutedEventArgs e)
        {
            //Về MainWindow
            ((MainWindow)Application.Current.MainWindow).Main.Content = null;
        }

        private void ClickMoveToPage1(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Main.Content
                = new AntivirusPage();
        }

        private void ClickMoveToPage2(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Main.Content
                = new StorageAnalyzierPage();
        }

        private void ClickMoveToPage3(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Main.Content
                = new CleanerPage();
        }

        #region Folder Picker

        private void ChooseBtn_Click(object sender, RoutedEventArgs e)
        {
            //Khi click thì show ShowDialog() nếu FileDialogResult là kết quả Ok thì hiện FileName
            if (FolderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GetFiles(FolderBrowserDialog.FileName);
            }
        }

        private void FilePathTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                !string.IsNullOrWhiteSpace(FilePathTxt.Text))
            {
                string path = FilePathTxt.Text;//Hiện path lên Text của TextBlock FilePathTxt

                if (Directory.Exists(path))
                {
                    GetFiles(path);
                }
                else
                {
                    MessageBox.Show("The folder path you entered doesn't exist.");
                    FilePathTxt.Text = CurrentRoot;
                }
            }
        }

        #endregion

        #region File Items

        private void FileItem_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null)
                return;

            FileItem item = clickedButton.Tag as FileItem;
            if (item == null)
                return;

            if (item.IsFolder)
            {
                GetFiles(item.FilePath);
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = item.FilePath,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Can't open file.");
                }
            }
        }

        private async void GetFiles(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("The folder path doesn't exist or cannot be accessed.");
                    return;
                }

                string[] folders = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);

                if (FileSizeTask != null && !FileSizeTask.IsCompleted)
                {
                    CancelTokenSource.Cancel();
                    await FileSizeTask;
                }

                CancelTokenSource.Dispose();
                CancelTokenSource = new CancellationTokenSource();

                FileDisplayList.Clear();

                foreach (string item in folders)
                {
                    FileDisplayList.Add(new FileItem()
                    {
                        FileName = System.IO.Path.GetFileName(item),
                        FilePath = item,
                        IsFolder = true
                    });
                }

                foreach (string item in files)
                {
                    FileDisplayList.Add(new FileItem()
                    {
                        FileName = System.IO.Path.GetFileName(item),
                        FilePath = item,
                        IsFolder = false
                    });
                }

                FilePathTxt.Text = CurrentRoot = path;
                FolderSizeLbl.Text = "Calculating...";
                Scroller.ScrollToTop();

                CancellationToken token = CancelTokenSource.Token;
                FileItem[] items = FileDisplayList.ToArray();

                FileSizeTask = Task.Run(() => GetFileSizes(items, token));
                await FileSizeTask;

                if (!token.IsCancellationRequested)
                {
                    FolderSizeLbl.Text =
                        FormatBytes(FileDisplayList.Sum(x => x.FileSize));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void GetFileSizes(
            FileItem[] items,
            CancellationToken cancellationToken)
        {
            foreach (FileItem item in items)
            {
                //Nếu user Cancel thì return
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    //Nếu IsFolder thì lấy FileSize trong DirectoryInfo của Folder
                    if (item.IsFolder)
                    {
                        item.FileSize = GetDirSize(
                            new DirectoryInfo(item.FilePath),
                            cancellationToken);
                    }
                    else
                    {
                        item.FileSize = new FileInfo(item.FilePath).Length;
                    }
                }
                catch
                {
                    item.FileSize = 0L;
                }
            }
        }

        #endregion

        #region Drive Analysis

        private void DriveCombo_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                ComboBoxItem selected = DriveCombo.SelectedItem as ComboBoxItem;
                if (selected == null || selected.Tag == null)
                    return;

                DriveInfo d = new DriveInfo(selected.Tag.ToString());

                if (!d.IsReady)
                {
                    DriveSpaceTakenLbl.Text = "—";
                    DriveRemainingSpaceLbl.Text = "—";
                    DriveTotalSpaceLbl.Text = "—";
                    return;
                }

                DriveSpaceTakenLbl.Text =
                    FormatBytes(d.TotalSize - d.TotalFreeSpace);
                DriveRemainingSpaceLbl.Text = FormatBytes(d.TotalFreeSpace);
                DriveTotalSpaceLbl.Text = FormatBytes(d.TotalSize);
            }
            catch
            {
                DriveSpaceTakenLbl.Text =
                    DriveRemainingSpaceLbl.Text =
                    DriveTotalSpaceLbl.Text = "—";
            }
        }

        #endregion

        #region Helper Functions

        //Lấy file trong Directory bằng thông qua DirectoryInfo và CancellationToken
        private static long GetDirSize(
            DirectoryInfo d,
            CancellationToken cancellationToken)
        {
            long size = 0L;

            try
            {
                FileInfo[] fis = d.GetFiles();

                foreach (FileInfo fi in fis)
                {
                    //Tương tự giải thích dưới
                    if (cancellationToken.IsCancellationRequested)
                        return 0L;

                    size += fi.Length;
                }

                DirectoryInfo[] dis = d.GetDirectories();

                foreach (DirectoryInfo di in dis)
                {
                    //Tương tự giải thích dưới
                    if (cancellationToken.IsCancellationRequested)
                        return 0L;

                    size += GetDirSize(di, cancellationToken);
                }
            }
            catch { }

            //Nếu user cancel thì return 0L; liền còn không thì hiện size (Do muốn app này xữ lý bất đồng bộ ==> user thoải mái di chuyển app mà không phải đợi Tính xong file size)
            if (cancellationToken.IsCancellationRequested)
                return 0L;
            else
                return size;
        }

        //FormatBytes To Reader String(like 50GB...) ==> Chuyển bytes lấy được trong PC về kểu dữ liệu dễ dọc
        public static string FormatBytes(long bytes)
        {
            double bytesConverted;
            string result = "—";

            try
            {
                if (bytes >= 1125899906842625L)
                {
                    result = "1000+ TB";
                }
                else if (bytes >= 1099511627776L)
                {
                    bytesConverted = bytes / (double)1099511627776L;
                    result = Math.Round(bytesConverted, 2).ToString() + " TB";
                }
                else if (bytes >= 1073741824L)
                {
                    bytesConverted = bytes / (double)1073741824L;
                    result = Math.Round(bytesConverted, 2).ToString() + " GB";
                }
                else if (bytes >= 1048576L)
                {
                    bytesConverted = bytes / (double)1048576L;
                    result = Math.Round(bytesConverted, 2).ToString() + " MB";
                }
                else if (bytes >= 1024L)
                {
                    bytesConverted = bytes / (double)1024L;
                    result = Math.Round(bytesConverted, 2).ToString() + " KB";
                }
                else if (bytes >= 1L)
                {
                    bytesConverted = bytes;
                    result = Math.Round(bytesConverted, 2).ToString() + " b";
                }
                else
                {
                    return result;
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        #endregion


        #region Helper Classes
        // Tạo Class FileItem kế thừa INotifyPropertyChanged ==> giúp update UI FileItem khi PropertyChanged 
        public class FileItem : INotifyPropertyChanged
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }

            public bool IsFolder { get; set; }

            public FileItem()
            {
                FileName = "";
                FilePath = "";
                IsFolder = false;
            }

            private long size = -1L;

            //Do mất 1 số thời gian để Tính Toán FileSize Nên Ta Sẽ Dùng event PropertyChangedEventHandler gọi hàm bất đồng bộ
            public long FileSize
            {
                get { return size; }
                set
                {
                    if (size != value)
                    {
                        size = value;
                        //Update UI FileSize sau khi Tính toán xong
                        OnPropertyChanged("FileSize");
                    }
                }
            }

            // PropertyChangedEventHandler Kiểu delegate xử lý sự kiện PropertyChanged khi một thuộc tính thay đổi.
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(
                        this,
                        new PropertyChangedEventArgs(propertyName)
                    );
                }
            }
        }

        #endregion
    }

    //Chuyển kiểu để có thể hiện trên UI
    public class IconConverter : IValueConverter
    {
        //Hiện icon folder hoặc file 
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return (value is bool && (bool)value) ?
                "https://img.icons8.com/fluency/48/folder-invoices--v1.png" :
                "https://img.icons8.com/fluency/48/file.png";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value;
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        //Chuyển byte thành 1 string có thể đọc được
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (!(value is long))
                return "—";
            //Không có fileSize có Item này thì ra về -1 ==> Ngược lại có fileSize thì hiện text Calculating...
            return (long)value == -1L
                ? "Calculating..."
                : StorageAnalyzierPage.FormatBytes((long)value);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value;
        }
    }
}
