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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Import thự viện cung cấp các hàm,thuộc tính mã hóa bảo mật
using System.Security.Cryptography;
//Import thự viện cung cấp các hàm,thuộc tính xử lý, đọc file (input,output)
using System.IO;

//Import thự viện cung cấp các hàm,thuộc tính Dùng để truy cập các hộp thoại trong hệ thống của Windows (hộp thoại mở file và lưu file)
using Microsoft.Win32;

//Import thự viện cung cấp các hàm,thuộc tính Dùng để xử lý màu sắc, hình ảnh, âm thanh và các đối tượng đồ họa.
using System.Windows.Media;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void ClickMoveToPage1(object sender, RoutedEventArgs e)
        {
            //Truyền và ghi đè tất cả nội dung XAML Trong Thẻ Grid của Class AntivirusPage vào Thẻ Grid của MainWindow() này
            Main.Content = new AntivirusPage();
        }

        private void ClickMoveToPage2(object sender, RoutedEventArgs e)
        {
            //Truyền và ghi đè tất cả nội dung XAML Trong Thẻ Grid của Class AntivirusPage vào Thẻ Grid của MainWindow() này
            Main.Content = new StorageAnalyzierPage();
        }

        private void ClickMoveToPage3(object sender, RoutedEventArgs e)
        {
            //Truyền và ghi đè tất cả nội dung XAML Trong Thẻ Grid của Class AntivirusPage vào Thẻ Grid của MainWindow() này
            Main.Content = new TimerPage();
        }



    }
}
