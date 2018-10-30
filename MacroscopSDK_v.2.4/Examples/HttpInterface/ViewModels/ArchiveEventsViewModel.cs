using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using HttpInterface.Annotations;

namespace HttpInterface
{
	public class ArchiveEventsViewModel : INotifyPropertyChanged
	{
		private readonly IArchiveEventsModel _archiveEventsModel;
		private readonly IRegisteredEventsViewModel _registeredEventsViewModel;
		private readonly IChannelsViewModel _channelsViewModel;
		private readonly ICommand _getArchiveEventsCommand;
		public ICommand GetArchiveEventsCommand
		{
			get { return _getArchiveEventsCommand; }
		}

		private IEnumerable<SubscribedEvent> _archiveEvents;

		public IEnumerable<SubscribedEvent> ArchiveEvents
		{
			get { return _archiveEvents; }
			private set
			{
				_archiveEvents = value;
				OnPropertyChanged("ArchiveEvents");
			}
		}

		public ChannelInfo SelectedChannelInfo { get; set; }

		public String SelectedDateTimeStart { get; set; }

		public String SelectedDateTimeEnd { get; set; }

		public ArchiveEventsViewModel(IArchiveEventsModel archiveEventsModel, IRegisteredEventsViewModel registeredEventsViewModel, IChannelsViewModel channelsViewModel)
		{
			_archiveEventsModel = archiveEventsModel;
			_registeredEventsViewModel = registeredEventsViewModel;
			_channelsViewModel = channelsViewModel;

			var dateTimeNow = DateTime.UtcNow;

			//todo datetime control ?
			SelectedDateTimeStart = dateTimeNow.AddHours(-1).ToString("dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
			SelectedDateTimeEnd = dateTimeNow.AddHours(1).ToString("dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);

			_getArchiveEventsCommand = new RelayCommand(o => OnGetArchiveEventsCommand());
		}

		private void OnGetArchiveEventsCommand()
		{
			var archiveParameters = new ArchiveParameters
			{
				ChannelId = _channelsViewModel.SelectedChannelId,
				DateTimeStart = SelectedDateTimeStart,
				DateTimeEnd = SelectedDateTimeEnd,
				EventId = _registeredEventsViewModel.SelectedEventInfo.Id
			};

			var archiveEventsTask = _archiveEventsModel.GetArchiveEventsTask(archiveParameters);
			archiveEventsTask.Start();
			archiveEventsTask.ContinueWith(t => { ArchiveEvents = t.Result; });
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
