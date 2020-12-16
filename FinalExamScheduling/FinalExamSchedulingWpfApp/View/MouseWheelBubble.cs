using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class MouseWheelBubble
{
	public static void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		// https://stackoverflow.com/questions/1585462/bubbling-scroll-events-from-a-listview-to-its-parent
		if (!e.Handled)
		{
			e.Handled = true;
			var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
			eventArg.RoutedEvent = UIElement.MouseWheelEvent;
			eventArg.Source = sender;
			var parent = ((Control)sender).Parent as UIElement;
			parent.RaiseEvent(eventArg);
		}
	}
}
