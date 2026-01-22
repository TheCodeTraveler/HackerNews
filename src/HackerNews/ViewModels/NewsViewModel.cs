using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HackerNews;

partial class NewsViewModel(TextAnalysisService textAnalysisService, HackerNewsAPIService hackerNewsApiService)
	: BaseViewModel
{
	readonly TextAnalysisService _textAnalysisService = textAnalysisService;
	readonly HackerNewsAPIService _hackerNewsAPIService = hackerNewsApiService;

	readonly WeakEventManager _pullToRefreshEventManager = new();

	[ObservableProperty]
	bool _isListRefreshing;

	public event EventHandler<string> PullToRefreshFailed
	{
		add => _pullToRefreshEventManager.AddEventHandler(value);
		remove => _pullToRefreshEventManager.RemoveEventHandler(value);
	}

	public ObservableCollection<StoryModel> TopStoryCollection { get; } = [];

	static void InsertIntoSortedCollection<T>(ObservableCollection<T> collection, Comparison<T> comparison, T modelToInsert)
	{
		if (collection.Count is 0)
		{
			collection.Add(modelToInsert);
		}
		else
		{
			int index = 0;
			foreach (var model in collection)
			{
				if (comparison(model, modelToInsert) >= 0)
				{
					collection.Insert(index, modelToInsert);
					return;
				}

				index++;
			}

			collection.Insert(index, modelToInsert);
		}
	}

	[RelayCommand]
	async Task Refresh(CancellationToken token)
	{
		TopStoryCollection.Clear();

		try
		{
			await foreach (var story in GetTopStories(StoriesConstants.NumberOfStories, token).ConfigureAwait(false))
			{
				StoryModel? updatedStory = null;

				try
				{
					updatedStory = story with { TitleSentiment = await _textAnalysisService.GetSentiment(story.Title).ConfigureAwait(false) };
				}
				catch (Exception)
				{
					//Todo Add TextAnalysis API Key in TextAnalysisConstants.cs
					updatedStory = story;
				}
				finally
				{
					if (updatedStory is not null && !TopStoryCollection.Any(x => x.Title.Equals(updatedStory.Title)))
						InsertIntoSortedCollection(TopStoryCollection, (a, b) => b.Score.CompareTo(a.Score), updatedStory);
				}

				await Task.Yield();
			}
		}
		catch (Exception e)
		{
			OnPullToRefreshFailed(e.ToString());
		}
		finally
		{
			IsListRefreshing = false;
		}
	}

	async IAsyncEnumerable<StoryModel> GetTopStories(int storyCount, [EnumeratorCancellation] CancellationToken token)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(storyCount);

		var topStoryIds = await _hackerNewsAPIService.GetTopStoryIDs(token).ConfigureAwait(false);

		var getTopStoryTaskList = topStoryIds.Select(id => _hackerNewsAPIService.GetStory(id, token)).ToList();

		await foreach (var topStoryTask in getTopStoryTaskList.ToAsyncEnumerable().WithCancellation(token).ConfigureAwait(false))
		{
			var story = await topStoryTask.ConfigureAwait(false);
			yield return story;

			if (--storyCount <= 0)
			{
				break;
			}
		}
	}

	void OnPullToRefreshFailed(string message) => _pullToRefreshEventManager.HandleEvent(this, message, nameof(PullToRefreshFailed));
}