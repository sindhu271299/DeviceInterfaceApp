
using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace DeviceInterfaceApp
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher watcher;

        public Form1()
        {
            InitializeComponent();
        }

        // Browse button
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the incoming HL7 folder";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = folderDialog.SelectedPath;
                    lblStatus.Text = "Status: Folder selected";
                }
            }
        }

        // Start button
        private void btnstart_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolder.Text.Trim();

            if (string.IsNullOrWhiteSpace(folderPath) ||
                !Directory.Exists(folderPath))
            {
                MessageBox.Show("Please select a valid folder.");
                return;
            }

            // Stop previous monitoring
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }

            // Create folder watcher
            watcher = new FileSystemWatcher();

            watcher.Path = folderPath;
            watcher.Filter = "*.*";

            watcher.NotifyFilter = NotifyFilters.FileName |
                                   NotifyFilters.LastWrite;

            watcher.Created += Watcher_Created;

            watcher.EnableRaisingEvents = true;

            lblStatus.Text = "Status: Listening for incoming files";

            MessageBox.Show("Device Interface started successfully!");
        }

        // Receive incoming file
        private async void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath).ToLower();

            if (extension != ".hl7" &&
                extension != ".txt" &&
                extension != ".csv")
            {
                return;
            }

            // Retry reading if file is still copying
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    await Task.Delay(500);

                    string data = File.ReadAllText(e.FullPath);

                    string fileName = Path.GetFileName(e.FullPath);

                    string receivedDate = DateTime.Now.ToString("dd-MM-yyyy");

                    string receivedTime = DateTime.Now.ToString("hh:mm:ss tt");

                    this.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText(
                            Environment.NewLine +
                            "================================" +
                            Environment.NewLine);

                        richTextBox1.AppendText(
                            "File Name: " + fileName +
                            Environment.NewLine);

                        richTextBox1.AppendText(
                            "Received Date: " + receivedDate +
                            Environment.NewLine);

                        richTextBox1.AppendText(
                            "Received Time: " + receivedTime +
                            Environment.NewLine);

                        richTextBox1.AppendText(
                            "================================" +
                            Environment.NewLine);

                        richTextBox1.AppendText(
                            data + Environment.NewLine);

                        lblStatus.Text = "Status: Data received";
                    });

                    return;
                }
                catch (IOException)
                {
                    // Retry if the file is still being copied
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "File reading error: " + ex.Message);
                    return;
                }
            }

            MessageBox.Show("Could not read the incoming file.");
        }

        // Stop button
        private void btnstop_Click(object sender, EventArgs e)
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }

            lblStatus.Text = "Status: Stopped";
        }
    }
}
