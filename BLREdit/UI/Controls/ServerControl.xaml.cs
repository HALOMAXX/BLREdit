using BLREdit.Game;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl : UserControl
    {
        static readonly EventTrigger trigger;

        static ServerControl()
        {
            var animation = new DoubleAnimationUsingKeyFrames()
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
            };

            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-256, KeyTime.FromTimeSpan(new(0, 0, 0, 0, 0))));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-256, KeyTime.FromTimeSpan(new(0, 0, 0, 0, 500))));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(new(0, 0, 0, 0, 5500))));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(new(0, 0, 0, 0, 6000))));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            Storyboard.SetTargetName(animation, "MapImage");
            Storyboard.SetTargetProperty(animation, new(Canvas.TopProperty));

            var beginStoryboard = new BeginStoryboard() { Storyboard = storyboard };

            trigger = new EventTrigger() { RoutedEvent = Image.LoadedEvent };
            trigger.Actions.Add(beginStoryboard);
        }

        public ServerControl()
        {
            InitializeComponent();
            MapImage.Triggers.Add(trigger);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (this.DataContext is BLRServer server)
                {
                    server.ConnectToServerCommand.Execute(null);
                }
            }
        }
    }
}
