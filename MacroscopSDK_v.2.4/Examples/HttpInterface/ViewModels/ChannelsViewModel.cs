using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using HttpInterface.Annotations;

namespace HttpInterface
{
	public class ChannelsViewModel : IChannelsViewModel, INotifyPropertyChanged
	{
		private readonly IChannelsModel _channelsModel;
		private readonly ICommand _getChannelsCommand;
		public ICommand GetChannelsCommand
		{
			get { return _getChannelsCommand; }
		}

		private IEnumerable<ChannelInfo> _channelsInfos;

		public IEnumerable<ChannelInfo> ChannelsInfos
		{
			get { return _channelsInfos; }
			private set
			{
				_channelsInfos = value;
				OnPropertyChanged("ChannelsInfos");
			}
		}

		public ChannelInfo SelectedChannelInfo { get; set; }

		public ChannelsViewModel(IChannelsModel channelsModel)
		{
			_channelsModel = channelsModel;

			_getChannelsCommand = new RelayCommand(o => OnGetChannelsCommand());

			SelectedChannelInfo = new ChannelInfo();
		}

		private void OnGetChannelsCommand()
		{
			var task = _channelsModel.GetChannelsInfosTask();
			task.Start();
			task.ContinueWith(t => { ChannelsInfos = t.Result; });
		}

		//for interface
		public Guid SelectedChannelId
		{
			get
			{
				return SelectedChannelInfo != null ? 
					SelectedChannelInfo.Id :
					Guid.Empty;
			}
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
