namespace HttpInterface
{
	public class MainWindowViewModel
	{
		public ConnectionViewModel ConnectionViewModel { get; set; }
		public EventsUpdaterViewModel EventsUpdaterViewModel { get; set; }
		public RegisteredEventsViewModel RegisteredEventsViewModel { get; set; }
		public ChannelsViewModel ChannelsViewModel { get; set; }
		public ArchiveEventsViewModel ArchiveEventsViewModel { get; set; }
		public UrlViewerViewModel UrlViewerViewModel { get; set; }
	}
}