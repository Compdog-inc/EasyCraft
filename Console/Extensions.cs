using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasyCraft.Console
{
    public static class Extensions
    {
        public static bool IsChildOf(this FrameworkElement c, FrameworkElement parent)
        {
            return (c.Parent != null && c.Parent == parent) || (c.Parent != null && (c.Parent as Control).IsChildOf(parent));
        }
    }
}
