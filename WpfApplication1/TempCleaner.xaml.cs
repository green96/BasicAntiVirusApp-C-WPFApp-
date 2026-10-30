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
using System.IO;//Thư viện chứa hàm,thuộc tính xữ lý Input/Output VD: .GetTempPath(), DirectoryInfo...
namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for TimerPage.xaml
    /// </summary>
    public partial class CleanerPage : Page
    {
        public CleanerPage()
        {
            InitializeComponent();
        }

        //Lưu ý: Chức năng của 2 button này chỉ hoạt động khi build ra App
        private void ClickMoveToClearPrefetch(object sender, RoutedEventArgs e)
        {

            DirectoryInfo prefetch = new DirectoryInfo("C:\\Windows\\Prefetch");//Đường dẫn nơi lưu trữ folder Prefetch trong Window
            try
            {
                if (prefetch.Exists)
                {
                    //lặp quan mỗi temp_1 lấy Files bằng GetFiles() lưu vào biến file dạng FileInfo rồi delete file đó
                    foreach (FileInfo file in prefetch.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                            // Bo qua file khong the xoa
                        }

                    }
                    foreach (DirectoryInfo folder in prefetch.GetDirectories())
                    {
                        try
                        {
                            folder.Delete(true); //giúp xóa thư mục cùng các file và thư mục con bên trong.
                        }
                        catch
                        {
                            // Bo qua folder khong the xoa
                        }

                    }
                }
                else {
                    MessageBox.Show("Prefetch Folder Not Found", "Not Found", MessageBoxButton.OK,  MessageBoxImage.Information);
                }

            }

            catch
            {
                MessageBox.Show("Cant Clear Prefetch", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //Lưu ý: Chức năng của 2 button này chỉ hoạt động khi build ra App
        private void ClickMoveToClearTemp(object sender, RoutedEventArgs e)
        {
            //Lưu thông tin Directory(Thư mục hay Folder) từ Path của hàm GetTempPath() và biến temp_1
            DirectoryInfo temp_1 = new DirectoryInfo(Path.GetTempPath());

            DirectoryInfo temp_2 = new DirectoryInfo("C:\\Windows\\Temp");//Đường dẫn nơi lưu trữ folder Temp trong Window
            try {
                if (temp_1.Exists)
                {
                    //lặp quan mỗi temp_1 lấy Files bằng GetFiles() lưu vào biến file dạng FileInfo rồi delete file đó
                    foreach (FileInfo file in temp_1.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {

                        }

                    }
                    foreach (DirectoryInfo folder in temp_1.GetDirectories())
                    {
                        try
                        {
                            folder.Delete();
                        }
                        catch
                        {

                        }

                    } 
                }
                else {
                    MessageBox.Show("Temp Folder Not Found", "Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (temp_2.Exists)
                {
                    //lặp quan mỗi temp_1 lấy Files bằng GetFiles() lưu vào biến file dạng FileInfo rồi delete file đó
                    foreach (FileInfo file in temp_2.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {

                        }

                    }
                    foreach (DirectoryInfo folder in temp_2.GetDirectories())
                    {
                        try
                        {
                            folder.Delete();
                        }
                        catch
                        {

                        }

                    } 
                }
                else {
                    MessageBox.Show("Temp Folder Not Found", "Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
            catch{
                   //Hiện MessageBox Với text Cant Clear Temp và Chỉ Cho User Bấm Button OK(MessageBoxButton.OK)
                MessageBox.Show("Cant Clear Temp", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        //Xữ lý chuyển trang
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

        private void ClickBackToMain(object sender, RoutedEventArgs e)
        {
            //Về MainWindow
            ((MainWindow)Application.Current.MainWindow).Main.Content = null;
        }
    }
}
