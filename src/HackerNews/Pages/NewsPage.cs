﻿using System.Collections;
using CommunityToolkit.Maui.Markup;

namespace HackerNews;

class NewsPage : BaseContentPage<NewsViewModel>
{
	readonly IBrowser _browser;
	readonly IDispatcher _dispatcher;

	public NewsPage(IBrowser browser,
		IDispatcher dispatcher,
		NewsViewModel newsViewModel) : base(newsViewModel, "Top Stories")
	{
		_browser = browser;
		_dispatcher = dispatcher;

		BindingContext.PullToRefreshFailed += HandlePullToRefreshFailed;

		Content = new RefreshView
			{
				RefreshColor = Colors.Black,

				Content = new CollectionView
					{
						BackgroundColor = Color.FromArgb("F6F6EF"),
						SelectionMode = SelectionMode.Single,
						ItemTemplate = new StoryDataTemplate(),

					}.Bind(CollectionView.ItemsSourceProperty,
						getter: static (NewsViewModel vm) => vm.TopStoryCollection)
					.Invoke(collectionView => collectionView.SelectionChanged += HandleSelectionChanged)

			}.Bind(RefreshView.IsRefreshingProperty,
				getter: static (NewsViewModel vm) => vm.IsListRefreshing,
				setter: static (vm, isRefreshing) => vm.IsListRefreshing = isRefreshing)
			.Bind(RefreshView.CommandProperty,
				getter: static (NewsViewModel vm) => vm.RefreshCommand,
				mode: BindingMode.OneTime);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (Content is RefreshView { Content: CollectionView collectionView } refreshView
			&& IsNullOrEmpty(collectionView.ItemsSource))
		{
			refreshView.IsRefreshing = true;
		}

		static bool IsNullOrEmpty(in IEnumerable? enumerable)
		{
			if (enumerable is null)
				return false;

			var enumerator = enumerable.GetEnumerator() ?? throw new InvalidOperationException("Enumerator not found");
			using var disposable = (IDisposable)enumerator;
			return !enumerator.MoveNext();
		}
	}

	async void HandleSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		var collectionView = (CollectionView)sender;
		collectionView.SelectedItem = null;

		if (e.CurrentSelection.FirstOrDefault() is StoryModel storyModel)
		{
			if (!string.IsNullOrEmpty(storyModel.Url))
			{
				var browserOptions = new BrowserLaunchOptions
				{
					PreferredControlColor = ColorConstants.BrowserNavigationBarTextColor,
					PreferredToolbarColor = ColorConstants.BrowserNavigationBarBackgroundColor
				};

				await _browser.OpenAsync(storyModel.Url, browserOptions);
			}
			else
			{
				await DisplayAlert("Invalid Article", "ASK HN articles have no url", "OK");
			}
		}
	}

	void HandlePullToRefreshFailed(object? sender, string message) =>
		_dispatcher.DispatchAsync(() => DisplayAlert("Refresh Failed", message, "OK"));
}