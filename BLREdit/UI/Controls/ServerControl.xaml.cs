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
        static Storyboard storyboard;

        static List<ServerControl> ServerControlList { get; } = [];

        static bool isPaused = false;

        private static void InitStaticComponents()
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

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            Storyboard.SetTargetName(animation, "MapImage");
            Storyboard.SetTargetProperty(animation, new(Canvas.TopProperty));
        }

        public static void PauseAnimation()
        {
            if (storyboard != null)
            {
                for(int i = ServerControlList.Count-1; i > 0; i--)
                { storyboard.Pause(ServerControlList[i]); }
                isPaused = true;
            }
        }
        public static void ResumeAnimation()
        {
            if (storyboard != null && isPaused)
            {
                for (int i = ServerControlList.Count - 1; i > 0; i--)
                { storyboard.Resume(ServerControlList[i]); }
            }
        }

        public ServerControl()
        {
            InitializeComponent();
            InitStaticComponents();
            try
            {
                storyboard.Begin(this, true);
                if (DataStorage.Settings.LastServerAnimationState.IsNot) { storyboard.Pause(this); }
                ServerControlList.Add(this);
            }
            catch (Exception ex) { LoggingSystem.MessageLog($"{ex.Message}\n{ex.StackTrace}", "ServerControl Init Error"); }
        }

        ~ServerControl()
        {
            storyboard.Remove(this);
            ServerControlList.Remove(this);
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
