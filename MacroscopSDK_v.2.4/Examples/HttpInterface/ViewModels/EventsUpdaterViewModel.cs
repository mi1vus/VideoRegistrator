using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using HttpInterface.Annotations;

namespace HttpInterface
{
	public class EventsUpdaterViewModel : INotifyPropertyChanged, IDisposable
	{
		private readonly EventsUpdaterModel _eventsUpdaterModel;
		private readonly IRegisteredEventsViewModel _registeredEventsViewModel;
		private readonly IDispatcherInvokator _dispatcherInvokator;

		private readonly ICommand _startEventsUpdaterCommand;
		private ObservableCollection<SubscribedEvent> _subscribedEvents;

		public ICommand StartEventsUpdaterCommand
		{
			get { return _startEventsUpdaterCommand; }
		}

		public ObservableCollection<SubscribedEvent> SubscribedEvents
		{
			get { return _subscribedEvents; }
			set
			{
				_subscribedEvents = value;
				OnPropertyChanged("SubscribedEvents");
			}
		}

		private string _currentStatus = "START";

		public string CurrentStatus
		{
			get { return _currentStatus; }

			private set
			{
				if (value == _currentStatus)
					return;

				_currentStatus = value;
				OnPropertyChanged("CurrentStatus");
			}
		}


		public EventsUpdaterViewModel(EventsUpdaterModel eventsUpdaterModel, IRegisteredEventsViewModel registeredEventsViewModel,
			IDispatcherInvokator dispatcherInvokator)
		{
			_eventsUpdaterModel = eventsUpdaterModel;
			_registeredEventsViewModel = registeredEventsViewModel;
			_dispatcherInvokator = dispatcherInvokator;

			_startEventsUpdaterCommand = new RelayCommand(o => OnStartEventsUpdaterCommand());
			SubscribedEvents = new ObservableCollection<SubscribedEvent>();

			_eventsUpdaterModel.IncomingNewEvent += _eventsUpdaterModel_IncomingNewEvent;
		}

		private void _eventsUpdaterModel_IncomingNewEvent(object sender, SubscribedEventArgs e)
		{
			var action = new Action(delegate
			{
				SubscribedEvents.Add(e.SubscribedEvent);
			});

			_dispatcherInvokator.InvokeAction(action);
		}

		private void OnStartEventsUpdaterCommand()
		{
			var selectedEvent = _registeredEventsViewModel.SelectedEventInfo;
			if (_eventsUpdaterModel.IsStarted())
				_eventsUpdaterModel.Stop();
			else
				_eventsUpdaterModel.Start(selectedEvent);

			CurrentStatus = _eventsUpdaterModel.IsStarted() ? "STOP" : "START";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Dispose()
		{
			_eventsUpdaterModel.Stop();
		}
	}
}
