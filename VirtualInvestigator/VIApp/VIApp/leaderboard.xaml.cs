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
        Grid myGrid;
        public leaderboard()
        {
            myGrid = new Grid();
            InitializeComponent();
            myGrid.Width = 250;
            myGrid.Height = 100;
            myGrid.HorizontalAlignment = HorizontalAlignment.Left;
            myGrid.VerticalAlignment = VerticalAlignment.Top;
            myGrid.ShowGridLines = true;

            ColumnDefinition nameCol = new ColumnDefinition();
            ColumnDefinition scoreCol = new ColumnDefinition();
            myGrid.ColumnDefinitions.Add(nameCol);
            myGrid.ColumnDefinitions.Add(scoreCol);

            for (int i = 0; i < 10; i++)
            {
                TextBox temp = new TextBox();
                myGrid.RowDefinitions.Add(new RowDefinition());
                temp.Text = @"This is a test";
                Grid.SetRow(temp, i);
                Grid.SetColumn(temp, 0);
                myGrid.Children.Add(temp);
            }
        }
    }
}