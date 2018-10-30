using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using HttpInterface.Annotations;

namespace HttpInterface
{
	public class RegisteredEventsViewModel : IRegisteredEventsViewModel, INotifyPropertyChanged
	{
		private readonly IRegisteredEventsModel _registeredEventsModel;
		private IEnumerable<EventInfo> _registeredEventInfos;

		public IEnumerable<EventInfo> RegisteredEventInfos
		{
			get { return _registeredEventInfos; }
			private set
			{
				_registeredEventInfos = value;
				OnPropertyChanged("RegisteredEventInfos");
			}
		}

		private readonly ICommand _getRegisteredEventsCommand;
		public ICommand GetRegisteredEventsCommand
		{
			get { return _getRegisteredEventsCommand; }
		}

		public EventInfo SelectedEvent { get; set; }

		public RegisteredEventsViewModel(IRegisteredEventsModel registeredEventsModel)
		{
			_registeredEventsModel = registeredEventsModel;

			_getRegisteredEventsCommand = new RelayCommand(o => OnGetEventsCommand());

			SelectedEvent = new EventInfo();
		}

		private void OnGetEventsCommand()
		{
			var task = _registeredEventsModel.GetRegisteredEventInfosTask();
			task.Start();
			task.ContinueWith(t => { RegisteredEventInfos = t.Result; });
		}

		//For interface
		public EventInfo SelectedEventInfo
		{
			get { return SelectedEvent; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
