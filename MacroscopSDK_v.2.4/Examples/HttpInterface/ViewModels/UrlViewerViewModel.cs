using System.ComponentModel;
using HttpInterface.Annotations;

namespace HttpInterface
{
	public class UrlViewerViewModel : IUrlViewer, INotifyPropertyChanged
	{
		private string _url;

		public string Url
		{
			get { return _url; }
			set
			{
				if (_url == value)
					return;
				_url = value;
				OnPropertyChanged("Url");
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
