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

        public string GetMDSFromFile(string filePath)
        {
            // Nếu chọn file text test chứa chuỗi mã MD5 32 ký tự
            try
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length < 10000)
                {
                    string content = File.ReadAllText(filePath);
                    string[] words = content.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var w in words)
                    {
                        if (w.Trim().Length == 32)
                        {
                            return w.Trim().ToLower();
                        }
                    }
                }
            }
            catch { }

            //using ở đây giúp tự động giải phóng đối tượng sau khi thực hiện xong khối lệnh ==> giúp không cần tự gọi: md5.Dispose();
            //MD5.Create() ==> tạo đối tượng chứa các hàm,thuộc tính để thực hiện thuật toán MD5 lưu vào biến md5 ==> md5 sẽ sử dụng được các hàm của MD5 bằng md5.Attribute/Function
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))//Đọc file từ filePath thông qua thư viện System.IO và lưu vào biến stream
                {
                    //BitConverter: chuyển từng byte thành 2 ký tự hệ 16 VD:10 ==> 0A (diễn các byte thành chuỗi ngắn gọn, dễ đọc và dễ so sánh.)
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();//Trả file path thông qua biến steam và mã hóa bằng hàm ComputeHash của thư viện System.Security.Cryptography đã tạo và lưu vào biến md5 ==> Sau đó ToString vì hàm này chỉ nhận return string(public string)
                    //Replace("-", string.Empty).ToLower();: thay mấy cái gạch ngang của hệ 16 (VD: 0A-12) Thành Empty(VD: 0A12) và chuyển về viết thường ToLower()
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog() ==> tạo đối tượng chứa các hàm,thuộc tính để tạo một hộp thoại cho phép người dùng chọn file từ máy tính và lưu vào biến ofd
            OpenFileDialog ofd = new OpenFileDialog();

            //Dùng thuộc tính Filter của đối tượng OpenFileDialog để Chỉ chọn file txt
            ofd.Filter = "Textfiles |*.txt";

            //Dùng hàm ShowDialog() của đối tượng OpenFileDialog ==> để check nếu true (đã chọn file thì chạy code dưới)
            if (ofd.ShowDialog() == true)
            {
                // lấy từ thuộc tính FileName của biến ofd đỗ data vào biến TbFilePath dạng .Text
                TbFilePath.Text = ofd.FileName;

                // Tính MD5 và hiển thị
                TbMD5.Text = GetMDSFromFile(ofd.FileName);
            }
        }

        private void ButtonScan_Click(object sender, RoutedEventArgs e)
        {

            //Đọc Tất cả dòng trong file Malware_sample222_MD5.txt và lưu vào biến md5signature
            var md5signature = File.ReadAllLines("Malware_sample222_MD5.txt");


            // Kiểm tra MD5 có tồn tại trong danh sách không
            if (md5signature.Contains(TbMD5.Text)) //Nếu md5signature chứa TbMD5.Text ==> Bất kỳ ký tự đã lưu trong biến md5signature mà giống(chứa) TbMD5.Text ==> Infected!
            {
                //Thay đổi Content của Label thành Infected
                StatusLabel.Content = "Infected!";
                //Đổi màu text
                StatusLabel.Foreground = Brushes.Red;
            }
            else 
            {
                StatusLabel.Content = "Clean!";
                //Đổi màu text
                StatusLabel.Foreground = Brushes.Green;
            }
        }

        /*
        Mẹo test để thấy kết quả Infected!:
        Copy một đoạn nhỏ mã MD5 mẫu có sẵn trong file Malware_sample222_MD5.txt (ví dụ: 5fa178eb37af1ad39165434d4cf612fa).
        Dán trực tiếp mã đó vào file text bất kỳ ở vị trí tùy ý.
        Bấm nút Scan Kết quả sẽ báo Infected! màu đỏ.
         
         */

    }
}
