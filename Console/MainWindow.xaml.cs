using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyCraft.Console
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string LOG_TYPE_NONE = "None";
        private const string LOG_TYPE_INFO = "Info";
        private const string LOG_TYPE_WARNING = "Warning";
        private const string LOG_TYPE_ERROR = "Error";

        private AnonymousPipeClientStream pipeClient;
        private StreamReader pipeReader;

        private delegate void CreateLogStackDelegate(string type, string text);
        private CreateLogStackDelegate __CreateLogStack;
        private void CreateLogStack(string type, string text) => Dispatcher.Invoke(__CreateLogStack, type, text);

        private SolidColorBrush logItemFont;
        private SolidColorBrush logItemNormal;
        private SolidColorBrush logItemDark;
        private SolidColorBrush logItemSelected;
        private SolidColorBrush logItemSelectedFont;

        private List<Brush> logBgs = new List<Brush>();
        private int selectedLogItem = -1;

        private bool descShown = false;
        private bool descSepMDown = false;
        private double startHeight = 100;
        private double startY = 0;

        public MainWindow(string pipeHandle)
        {
            __CreateLogStack = new CreateLogStackDelegate((type, text) =>
            {
                LogItem item = new LogItem()
                {
                    LogType = type,
                    LogText = text,
                    Foreground = logItemFont,
                    Background = logBgs.Count % 2 == 0 ? logItemNormal : logItemDark
                };
                item.MouseLeftButtonDown += new MouseButtonEventHandler(logItem_MouseDown);
                logBgs.Add(item.Background);
                logStack.Children.Add(item);
            });

            logItemFont = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            logItemNormal = new SolidColorBrush(Color.FromRgb(222, 222, 222));
            logItemDark = new SolidColorBrush(Color.FromRgb(216, 216, 216));
            logItemSelected = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            logItemSelectedFont = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            InitializeComponent();

            InitPipe(pipeHandle);
        }

        private void InitPipe(string handle)
        {
            try
            {
                pipeClient = new AnonymousPipeClientStream(PipeDirection.In, handle);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error connecting to: " + handle + ". " + e);
                Environment.Exit(2);
                return;
            }

            pipeReader = new StreamReader(pipeClient);

            string temp;
            do
            {
                temp = pipeReader.ReadLine();
            }
            while (!temp.StartsWith("SYNC"));

            new Task(PipeThread).Start();
        }

        private void PipeThread()
        {
            while (true)
            {
                int logType = -1;
                do
                {
                    logType = pipeReader.Read();
                } while (logType < 0);

                string ltype = LOG_TYPE_NONE;
                switch (logType)
                {
                    case 1:
                        ltype = LOG_TYPE_INFO;
                        break;
                    case 2:
                        ltype = LOG_TYPE_WARNING;
                        break;
                    case 3:
                        ltype = LOG_TYPE_INFO;
                        break;
                }

                StringBuilder logBuilder = new StringBuilder();
                int temp = -1;
                do
                {
                    temp = pipeReader.Read();
                    if (temp >= 0) logBuilder.Append((char)temp);
                } while (temp != 0);
                CreateLogStack(ltype, logBuilder.ToString());
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            pipeReader?.Close();
            pipeReader?.Dispose();
            pipeClient?.Close();
            pipeClient?.Dispose();
        }

        private void descSeparator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == descSeparator && descShown)
            {
                descSepMDown = true;
                startHeight = descRowDef.Height.Value;
                startY = e.GetPosition(this).Y;
            }
        }

        private void descSeparator_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (descShown)
            {
                descSepMDown = false;
                startHeight = descRowDef.Height.Value;
            }
        }

        private void descSeparator_MouseMove(object sender, MouseEventArgs e)
        {
            if (descSepMDown && descShown)
            {
                double dist = e.GetPosition(this).Y - startY;
                descRowDef.Height = new GridLength(startHeight - dist);
            }
        }

        private void logItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is LogItem item)
            {
                SetSelected(selectedLogItem, false);
                selectedLogItem = logStack.Children.IndexOf(item);
                SetSelected(selectedLogItem, true);
            }
            else
            {
                SetSelected(selectedLogItem, false);
                selectedLogItem = -1;
            }
        }

        private void SetSelected(int index, bool selected)
        {
            if (index < 0) return;
            if (logStack.Children[index] is LogItem item)
            {
                item.Background = selected ? logItemSelected : logBgs[index];
                item.Foreground = selected ? logItemSelectedFont : logItemFont;

                if (selected)
                {
                    descText.Text = item.LogText;
                    descShown = true;
                    descPanel.Visibility = Visibility.Visible;
                    descRowDef.Height = new GridLength(startHeight);
                } else
                {
                    descShown = false;
                    descPanel.Visibility = Visibility.Collapsed;
                    descRowDef.Height = new GridLength(0);
                }
            }
        }

        private void ScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is LogItem) && !(e.Source == descPanel || e.Source == descSeparator || e.Source == descSeparatorFill || e.Source == descText || e.Source is Run))
            {
                SetSelected(selectedLogItem, false);
                selectedLogItem = -1;
            }
        }
    }
}