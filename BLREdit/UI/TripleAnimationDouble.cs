using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BLREdit.UI
{
    public class TripleAnimationDouble
    {
        readonly DoubleAnimation AppearAnimation = new();
        readonly DoubleAnimation StayAnimation = new();
        readonly DoubleAnimation DisappearAnimation = new();
        readonly Storyboard AnimationFlow = new();

        public TripleAnimationDouble(double from, double to, double appearTime, double stayTime, double disappearTime, DependencyObject target, DependencyProperty property)
        {
            AnimationFlow.Children.Add(AppearAnimation);
            AnimationFlow.Children.Add(StayAnimation);
            AnimationFlow.Children.Add(DisappearAnimation);
            AnimationFlow.Duration = new Duration(TimeSpan.FromSeconds(appearTime+stayTime+disappearTime));
            SetSize(from, to);
            Storyboard.SetTarget(AppearAnimation, target);
            Storyboard.SetTarget(StayAnimation, target);
            Storyboard.SetTarget(DisappearAnimation, target);
            var targetProperty = new PropertyPath(property);
            Storyboard.SetTargetProperty(AppearAnimation, targetProperty);
            Storyboard.SetTargetProperty(StayAnimation, targetProperty);
            Storyboard.SetTargetProperty(DisappearAnimation, targetProperty);

            AppearAnimation.Duration = new Duration(TimeSpan.FromSeconds(appearTime));
            StayAnimation.BeginTime = TimeSpan.FromSeconds(appearTime);
            StayAnimation.Duration = new Duration(TimeSpan.FromSeconds(stayTime));
            DisappearAnimation.BeginTime = TimeSpan.FromSeconds(appearTime + stayTime);
            DisappearAnimation.Duration = new Duration(TimeSpan.FromSeconds(disappearTime));
        }

        public TripleAnimationDouble(double from, double to, double appearTime, double stayTime, double disappearTime, FrameworkElement target, DependencyProperty property, ItemCollection list) : this (from, to, appearTime, stayTime, disappearTime, target, property)
        {
            AnimationFlow.Completed += (o, args) => {
                list.Remove(target);
            };
        }

        public void Begin(FrameworkElement containingObject)
        {
            AnimationFlow.Begin(containingObject);
        }

        public void SetSize(double from, double to)
        {
            AppearAnimation.From = from;
            AppearAnimation.To = to;

            StayAnimation.From = to;
            StayAnimation.To = to;

            DisappearAnimation.From = to;
            DisappearAnimation.To = from;
        }
    }
}
