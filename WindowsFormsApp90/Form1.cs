using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp90
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text);
            request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    string[] details = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string permissions = details[0];
                    string fileType = details[0].Substring(0, 1) == "d" ? "Directory" : "File";
                    string name = details[details.Length - 1];

                    TreeNode node = new TreeNode(name);
                    node.Tag = fileType;

                    if (fileType == "Directory")
                    {
                        node.Nodes.Add("*"); 
                    }

                    treeView1.Nodes.Add(node);
                }
            }

            reader.Close();
            response.Close();
        }
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "*") 
            {
                e.Node.Nodes.Clear();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + e.Node.FullPath);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] details = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string permissions = details[0];
                        string fileType = details[0].Substring(0, 1) == "d" ? "Directory" : "File";
                        string name = details[details.Length - 1];

                        TreeNode node = new TreeNode(name);
                        node.Tag = fileType;

                        if (fileType == "Directory")
                        {
                            node.Nodes.Add("*"); 
                        }

                        e.Node.Nodes.Add(node);
                    }
                }

                reader.Close();
                response.Close();
            }
        }
        private void upload_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string targetDirectory = UploadTxt.Text.Trim();

                foreach (string fileName in openFileDialog1.FileNames)
                {
                    string targetFilePath = HostTxt.Text + targetDirectory + Path.GetFileName(fileName);

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(targetFilePath);
                    request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    byte[] file = File.ReadAllBytes(fileName);
                    Stream strz = request.GetRequestStream();
                    strz.Write(file, 0, file.Length);
                    strz.Close();
                    strz.Dispose();

                    MessageBox.Show(Path.GetFileName(fileName) + " завантажено");
                }
            }
        }

        private void create_Click(object sender, EventArgs e)
        {
            string newDirectoryName = NewDirTxt.Text.Trim();
            if (!string.IsNullOrEmpty(newDirectoryName))
            {
                TreeNode selectedNode = treeView1.SelectedNode;
                if (selectedNode != null)
                {
                    string directoryPath = HostTxt.Text + selectedNode.FullPath + "/" + newDirectoryName;

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                    request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    MessageBox.Show("Каталог " + newDirectoryName + " створено");

                    response.Close();

                    selectedNode.Nodes.Add(newDirectoryName); 
                }
            }
        }

        private void appe_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + openFileDialog1.SafeFileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.AppendFile;

                using (Stream fileStream = openFileDialog1.OpenFile())
                using (Stream ftpStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }

                MessageBox.Show(openFileDialog1.SafeFileName + " додано");
            }
        }

        private void dele_Click(object sender, EventArgs e)
        {
            string fileName = List1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(fileName))
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + fileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                MessageBox.Show(fileName + " видалено");

                response.Close();
            }
        }

        private void retr_Click(object sender, EventArgs e)
        {
            string fileName = List1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(fileName))
            {
                saveFileDialog1.Filter = "All files (*.*)|*.*";
                saveFileDialog1.FileName = fileName;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + fileName);
                    request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    using (Stream ftpStream = response.GetResponseStream())
                    using (Stream fileStream = File.Create(saveFileDialog1.FileName))
                    {
                        ftpStream.CopyTo(fileStream);
                    }

                    MessageBox.Show(fileName + " завантажено");
                }
            }
        }

        private void mdt_Click(object sender, EventArgs e)
        {
            string fileName = List1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(fileName))
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + fileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                MessageBox.Show("Час модифікації " + fileName + ": " + response.LastModified);

                response.Close();
            }
        }

        private void size_Click(object sender, EventArgs e)
        {
            string fileName = List1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(fileName))
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + fileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.GetFileSize;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                MessageBox.Show("Розмір " + fileName + ": " + response.ContentLength + " bytes");

                response.Close();
            }
        }

        private void nlist_Click(object sender, EventArgs e)
        {
            List1.Items.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text);
            request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    List1.Items.Add(line);
                }
            }

            MessageBox.Show("NLIST виконано");
        }

        private void list_Click(object sender, EventArgs e)
        {
            List1.Items.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text);
            request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    List1.Items.Add(line);
                }
            }

            MessageBox.Show("LIST виконано");
        }

        private void mkd_Click(object sender, EventArgs e)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + NewDirTxt.Text);
            request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            MessageBox.Show("Каталог " + NewDirTxt.Text + " створено");

            response.Close();
        }

        private void rmd_Click(object sender, EventArgs e)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + RemoveDirTxt.Text);
            request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            MessageBox.Show("Каталог " + RemoveDirTxt.Text + " видалено");

            response.Close();
        }

        private void rename_Click(object sender, EventArgs e)
        {
            string oldFileName = List1.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(oldFileName) && !string.IsNullOrEmpty(newNameTxt.Text))
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + oldFileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = newNameTxt.Text;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                MessageBox.Show(oldFileName + " прейменовано на " + newNameTxt.Text);

                response.Close();
            }
        }

        private void stor_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text + openFileDialog1.SafeFileName);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream fileStream = openFileDialog1.OpenFile())
                using (Stream ftpStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }

                MessageBox.Show(openFileDialog1.SafeFileName + " завантажено");
            }
        }

        private void stou_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(HostTxt.Text);
                request.Credentials = new NetworkCredential(UserTxt.Text, PasswordTxt.Text);
                request.Method = WebRequestMethods.Ftp.UploadFileWithUniqueName;

                using (Stream fileStream = openFileDialog1.OpenFile())
                using (Stream ftpStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }

                MessageBox.Show("Файл завантажено з унікальною назвою!");
            }

        }
        private void LoadSettings()
        {
            string settingsFilePath = "settings.txt";

            if (File.Exists(settingsFilePath))
            {
                using (StreamReader reader = new StreamReader(settingsFilePath))
                {
                    hostTxtBox.Text = reader.ReadLine();
                    UsernameTxt.Text = reader.ReadLine();
                    passwordTxtBox.Text = reader.ReadLine();
                }
            }
        }

        private void settings_Click(object sender, EventArgs e)
        {
            Setting settingsForm = new Setting();
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                LoadSettings();
            }
        }
    }
}
