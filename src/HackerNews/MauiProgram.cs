using Azure;
using Azure.AI.TextAnalytics;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace HackerNews;

public class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup();

		// App
		builder.Services.AddSingleton<App>();
		builder.Services.AddSingleton<AppShell>();

		// Services
		builder.Services.AddSingleton(Browser.Default);
		builder.Services.AddSingleton<TextAnalysisService>();
		builder.Services.AddSingleton<HackerNewsAPIService>();
		builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(TextAnalysisConstants.BaseUrl), new AzureKeyCredential(TextAnalysisConstants.SentimentKey)));

		builder.Services.AddRefitClient<IHackerNewsAPI>()
			.ConfigureHttpClient(static client => client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0"))
			.AddStandardResilienceHandler(static options => options.Retry = new MobileHttpRetryStrategyOptions());

		// Pages + View Models
		builder.Services.AddTransientWithShellRoute<NewsPage, NewsViewModel>($"//{nameof(NewsPage)}");

		return builder.Build();
	}

	sealed class MobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public MobileHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 3;
			UseJitter = true;
			Delay = TimeSpan.FromSeconds(2);
		}
	}
}