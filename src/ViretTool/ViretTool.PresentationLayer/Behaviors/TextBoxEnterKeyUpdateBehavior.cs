using System.Windows.Controls;
using System.Windows.Input;

namespace ViretTool.PresentationLayer.Behaviors
{
    public class TextBoxEnterKeyUpdateBehavior : BaseBehavior<TextBox>
    {
        protected override void CleanUp()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
            }
        }

        protected override void WireUp()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && e.Key == Key.Return && e.Key == Key.Enter)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
