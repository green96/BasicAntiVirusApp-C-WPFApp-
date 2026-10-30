﻿using System;
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
using System.Text.RegularExpressions;//Thư viện Regex (Regex cung cấp các Hàm,thuộc tính... giúp tìm tất cả chuỗi MD5 có đúng 32 ký tự hexadecimal trong từng file TXT.)
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
    /// Interaction logic for AntivirusPage.xaml
    /// </summary>
    public partial class AntivirusPage : Page
    {
        public AntivirusPage()
        {
            InitializeComponent();
        }


        public string GetMDSFromFile(string filePath)
        {
            // Ưu tiên lấy MD5 có sẵn bên trong file TXT
            try
            {
                FileInfo fi = new FileInfo(filePath);

                if (fi.Length < 10000)
                {
                    string content = File.ReadAllText(filePath);

                    Match match = Regex.Match(
                        content,
                        @"\b[a-fA-F0-9]{32}\b"
                    );

                    if (match.Success)
                    {
                        return match.Value.ToLower();
                    }
                }
            }
            catch
            {
                // Nếu không đọc được nội dung thì chuyển sang tính MD5
            }

            // Nếu không có MD5 bên trong file thì tính MD5 thực tế
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
            //var md5signature = File.ReadAllLines("Malware_sample222_MD5.txt");

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(TbMD5.Text))
            {
                MessageBox.Show("Please Select File To Scan!");
                return;
            }
            // Kiểm tra MD5 có đúng định dạng không
            string targetMD5 = TbMD5.Text.Trim().ToLower();

            if (!Regex.IsMatch(targetMD5, @"\A[a-f0-9]{32}\z"))
            {
                //targetMD5 không đúng quy định string của Regex(kỹ tự từ a đến f, từ 0 đến 9, Tổng là 32 ký tự) ==> Thì chạy thông báo dưới
                MessageBox.Show("Invalid MD5!");
                return;
            }

            // Hiển thị Loading Cursor
            Mouse.OverrideCursor = Cursors.Wait;

            // Vô hiệu hóa nút Scan để tránh click nhiều lần
            HiButton.IsEnabled = false;

            try
            {
                int totalFiles;

                // Chạy quá trình scan trên Background Thread
                // Tìm MD5 trong toàn bộ database (Database này chỉ lưu tất cả nội dung file virusShare)
                bool isInfected = SearchMD5Database(
                    targetMD5,
                    out totalFiles
                );

                if (isInfected)
                {
                    StatusLabel.Content = "Infected!";
                    StatusLabel.Foreground = Brushes.Red;
                }
                else
                {
                    StatusLabel.Content = "Clean!";
                    StatusLabel.Foreground = Brushes.Green;
                }

                MessageBox.Show(
                    "Scan completed!\n" +
                    "MD5: " + targetMD5 + "\n" +
                    "Total TXT files: " + totalFiles + "\n" +
                    "Result: " + (isInfected ? "Infected!" : "Clean!")
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Scan error: " + ex.Message
                );
            }
            finally
            {
                //finally ==>Luôn thực thi để khôi phục Cursor và nút Scan.

                // Khôi phục Cursor bình thường
                Mouse.OverrideCursor = null;

                // Cho phép click Scan trở lại
                HiButton.IsEnabled = true;
            }
            /*
            Mẹo test để thấy kết quả Infected!:
            Copy một đoạn nhỏ mã MD5 mẫu có sẵn trong file Malware_sample222_MD5.txt (ví dụ: 5fa178eb37af1ad39165434d4cf612fa).
            Dán trực tiếp mã đó vào file text bất kỳ ở vị trí tùy ý.
            Bấm nút Scan Kết quả sẽ báo Infected! màu đỏ.
         
             */
        }


        private bool SearchMD5Database(
            //phải truyền targetMD5 khi gọi hàm để lấy path
            string targetMD5,

            //out int totalFiles ==> Đây là tham số đầu ra (out).
            //out int totalFiles ==> Nó cho phép hàm truyền dữ liệu totalFiles ngược trở lại nơi gọi hàm.
            out int totalFiles)
        {
            //Sau khi hàm chạy xong, biến totalFiles sẽ nhận số lượng file database đã kiểm tra ==> truyền dữ liệu totalFiles ngược trở lại nơi gọi hàm (Dùng tham số out int totalFiles)
            totalFiles = 0;

            // Đường dẫn Malware_sample\VirusShare_Hashes
            string folderPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\Malware_sample\VirusShare_Hashes"
                )
            );
            /*
            AppDomain.Domain.CurrentDomain.BaseDirectory: lấy thư mục chạy chương trình, thường là bin\Debug.
            ..\..: quay ngược hai cấp về thư mục project WpfApplication1.
            Malware_sample\VirusShare_Hashes: truy cập thư mục chứa dữ liệu MD5.
            Path.GetFullPath(): chuyển đường dẫn tương đối thành đường dẫn tuyệt đối.
             */

            // Kiểm tra thư mục
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(
                    "Cannot find database folder:\n" + folderPath
                );
            }

            // Lấy tất cả file TXT
            string[] files = Directory.GetFiles(
                folderPath,
                "*.txt",
                SearchOption.AllDirectories
            );

            totalFiles = files.Length;

            if (totalFiles == 0)
            {
                throw new FileNotFoundException(
                    "No TXT files found in database!"
                );
            }

            // Duyệt từng file TXT
            foreach (string file in files)
            {
                try
                {
                    // Đọc từng dòng, không tải toàn bộ file vào RAM
                    foreach (string line in File.ReadLines(file))
                    {
                        // Tìm chính xác MD5 cần kiểm tra
                        if (Regex.IsMatch(
                            line,
                            @"\b" + Regex.Escape(targetMD5) + @"\b",
                            RegexOptions.IgnoreCase))
                        {
                            // Tìm thấy chữ ký virus
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bỏ qua file không đọc được
                    Console.WriteLine(
                        "Cannot read: " + file +
                        "\n" + ex.Message
                    );
                }
            }

            // Không tìm thấy MD5 trong database
            return false;
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