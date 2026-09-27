using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Byoe.BasicFramework;

namespace Byoe.LogNet
{
    /// <summary>
    /// 日志查看器的窗口类，用于分析统计日志数据，实例化的时候可以直接日志文件，然后直接显示出文件内容出来，然后可以根据日志的等级，或是关键字进行搜索信息<br />
    /// The window class of the log viewer is used to analyze the statistical log data, and when instantiating, you can directly log the file, 
    /// and then directly display the file content, and then you can search for information according to the level of the log or keyword
    /// </summary>
    public partial class FormLogNetView : Form
    {
        private string logFilePath = string.Empty;
        /// <summary>
		/// 获取或设置当前日志选择窗口的默认的路径信息<br />
		/// Get or set the default path information of the current log selection window
		/// </summary>
		public string OpenDialogDefaultPath { get; set; }

        /// <summary>
        /// 实例化一个默认的日志查看器的窗口<br />
        /// Instantiates a default log viewer window
        /// </summary>
        public FormLogNetView()
        {
            InitializeComponent();
        }

        /// <summary>
		/// 指定一个日志路径实例化一个日志查看界面
		/// </summary>
		/// <param name="log">日志的路径</param>
		public FormLogNetView(string log)
        {
            InitializeComponent();
            logFilePath = log;
        }

        /// <inheritdoc />
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!string.IsNullOrEmpty(logFilePath))
            {
                textBox1.Text = logFilePath;
                DealWithFileName(logFilePath);
            }
            if (!string.IsNullOrEmpty(OpenDialogDefaultPath) && string.IsNullOrEmpty(logFilePath))
            {
                textBox1.Text = OpenDialogDefaultPath;
            }
        }
        private void FormLogNetView_Load(object sender, EventArgs e)
        {       
            label1.Text = StringResources.Language.LogNetFilePath;
            userButton1.Text = StringResources.Language.LogNetFileSelect;
            Text = StringResources.Language.LogNetViewer;
        }

        private void userButton1_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(OpenDialogDefaultPath))
            {
                openFileDialog.InitialDirectory = OpenDialogDefaultPath;
            }
            openFileDialog.Filter = StringResources.Language.LogNetFilter;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
                DealWithFileName(openFileDialog.FileName);
            }
        }

        private void DealWithFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            if (!File.Exists(fileName))
            {
                MessageBox.Show(StringResources.Language.FileNotExist);
                return;
            }
            try
            {
                using StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8);
                try
                {
                    logNetAnalysisControl1.SetLogNetSource(streamReader.ReadToEnd());
                }
                catch (Exception ex)
                {
                    SoftBasic.ShowExceptionMessage(ex);
                }
            }
            catch (Exception ex2)
            {
                SoftBasic.ShowExceptionMessage(ex2);
            }
        }

        private void logNetAnalysisControl1_Load(object sender, EventArgs e)
        {
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", "https://github.com/dathlin/C-S-");
            }
            catch
            {
            }
        }

     
    }
}
