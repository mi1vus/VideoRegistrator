namespace HttpInterface
{
	public partial class MainWindow
	{
		public MainWindowViewModel MainWindowViewModel = new MainWindowViewModel();

		public MainWindow()
		{
			InitializeComponent();

			var webRequestFactory = new WebRequestFactory();

			var dispatcherInvokator = new DispatcherInvokator();

			var connectionViewModel = InitializeConnectionViewModel();

			var urlViewer = new UrlViewerViewModel();

			var registeredEventViewModel = InitializeRegistereEventsView(connectionViewModel, urlViewer, webRequestFactory);

			var channelsViewModel = InitializeChannelsViewModel(connectionViewModel, urlViewer, webRequestFactory);

			InitializeEventUpdaterViewModel(connectionViewModel, registeredEventViewModel, channelsViewModel, urlViewer, webRequestFactory, dispatcherInvokator);

			InitialzieArchiveEventsViewModel(connectionViewModel, webRequestFactory, registeredEventViewModel, channelsViewModel, urlViewer);

			MainWindowViewModel.UrlViewerViewModel = urlViewer;

			DataContext = MainWindowViewModel;
		}

		private void InitialzieArchiveEventsViewModel(IConnectionParametersModel connectionViewModel,
			IWebRequestFactory webRequestFactory, 
			IRegisteredEventsViewModel registeredEventViewModel,
			IChannelsViewModel channelsViewModel,
			IUrlViewer urlViewer)
		{
			var archiveEventsModel = new ArchiveEventsModel(connectionViewModel, urlViewer, webRequestFactory);

			var archiveEventsViewModel = new ArchiveEventsViewModel(archiveEventsModel, registeredEventViewModel,
				channelsViewModel);

			MainWindowViewModel.ArchiveEventsViewModel = archiveEventsViewModel;
		}

		private void InitializeEventUpdaterViewModel(IConnectionParametersModel connectionViewModel,
			IRegisteredEventsViewModel registeredEventsViewModel,
			IChannelsViewModel channelsViewModel,
			IUrlViewer urlViewer,
			IWebRequestFactory webRequestFactory,
			IDispatcherInvokator dispatcherInvokator)
		{
			var eventUpdaterModel = new EventsUpdaterModel(connectionViewModel, channelsViewModel, urlViewer, webRequestFactory);

			var eventUpdaterViewModel = new EventsUpdaterViewModel(eventUpdaterModel, registeredEventsViewModel, dispatcherInvokator);

			MainWindowViewModel.EventsUpdaterViewModel = eventUpdaterViewModel;
		}

		private ChannelsViewModel InitializeChannelsViewModel(IConnectionParametersModel connectionViewModel, IUrlViewer urlViewer, IWebRequestFactory webRequestFactory)
		{
			var channelsModel = new ChannelsModelModel(connectionViewModel, urlViewer, webRequestFactory);

			var channelsViewModel = new ChannelsViewModel(channelsModel);

			MainWindowViewModel.ChannelsViewModel = channelsViewModel;

			return channelsViewModel;
		}

		private ConnectionViewModel InitializeConnectionViewModel()
		{
			var connectionViewModel = new ConnectionViewModel();

			MainWindowViewModel.ConnectionViewModel = connectionViewModel;

			return connectionViewModel;
		}

		private RegisteredEventsViewModel InitializeRegistereEventsView(IConnectionParametersModel connectionViewModel, IUrlViewer urlViewer,
			IWebRequestFactory webRequestFactory)
		{
			var registeredEventsModel = new RegisteredEventsModelModel(connectionViewModel,  urlViewer, webRequestFactory);
			var registeredEventViewModel = new RegisteredEventsViewModel(registeredEventsModel);

			MainWindowViewModel.RegisteredEventsViewModel = registeredEventViewModel;

			return registeredEventViewModel;
		}

        private void ChannelsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
