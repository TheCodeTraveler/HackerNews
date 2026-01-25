namespace HackerNews;

partial class AppShell : Shell
{
	public AppShell(NewsPage newsPage)
	{
		Items.Add(newsPage);
	}
}