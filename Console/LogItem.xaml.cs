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

namespace EasyCraft.Console
{
    /// <summary>
    /// Interaction logic for LogItem.xaml
    /// </summary>
    public partial class LogItem : UserControl
    {
        public string LogText
        {
            get { return (string)GetValue(LogTextProperty); }
            set { SetValue(LogTextProperty, value); }
        }

        public static readonly DependencyProperty LogTextProperty =
            DependencyProperty.Register("LogText", typeof(string), typeof(LogItem), new PropertyMetadata());

        public string LogType
        {
            get { return (string)GetValue(LogTypeProperty); }
            set { SetValue(LogTypeProperty, value); }
        }

        public static readonly DependencyProperty LogTypeProperty =
            DependencyProperty.Register("LogType", typeof(string), typeof(LogItem), new PropertyMetadata());

        public LogItem()
        {
            InitializeComponent();
        }
    }
}
