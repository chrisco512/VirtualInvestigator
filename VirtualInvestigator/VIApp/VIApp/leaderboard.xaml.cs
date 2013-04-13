using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace VIApp
{
    public partial class leaderboard : PhoneApplicationPage
    {
        public leaderboard()
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                TextBlock temp = new TextBlock();
                TablePanel.RowDefinitions.Add(new RowDefinition());
                temp.FontSize = 20;
                temp.Foreground = new SolidColorBrush(Color.FromArgb(225,225,105,10));
                temp.Text = @"This is a test";
                Grid.SetRow(temp, i);
                Grid.SetColumn(temp, 0);
                TablePanel.Children.Add(temp);
            }
        }
    }
}