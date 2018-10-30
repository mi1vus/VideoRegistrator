using System;
using System.Windows;

namespace HttpInterface
{
	class DispatcherInvokator : IDispatcherInvokator
	{
		public void InvokeAction(Action action)
		{
			if (Application.Current == null)
				return;

			Application.Current.Dispatcher.Invoke(new Action(delegate
			{
				try
				{
					action();
				}
				catch (Exception)
				{
					//
				}
			}));	
		}
	}
}
