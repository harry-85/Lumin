using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HeliosClockApp.Controls
{
    [ContentProperty("VerticalContent")]
    public class VerticalContentView : ContentView
    {
        public View VerticalContent
        {
            get => (View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, Verticalize(value));
        }

        public double ContentRotation { get; set; } = -90;

        private View Verticalize(View toBeRotated)
        {
            if (toBeRotated == null)
                return null;

            toBeRotated.Rotation = ContentRotation;
            var result = new RelativeLayout();

            result.Children.Add(toBeRotated,
            xConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.X - ((parent.Height - parent.Width) / 2);
            }),
            yConstraint: Constraint.RelativeToParent((parent) =>
            {
                return (parent.Height / 2) - (parent.Width / 2);
            }),
            widthConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.Height;
            }),
            heightConstraint: Constraint.RelativeToParent((parent) =>
            {
                return parent.Width;
            }));

            return result;
        }
    }
}
