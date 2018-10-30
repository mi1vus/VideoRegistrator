using System;

namespace HttpInterface
{
	public interface IDispatcherInvokator
	{
		void InvokeAction(Action action);
	}
}
