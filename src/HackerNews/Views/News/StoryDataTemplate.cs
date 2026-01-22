using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace HackerNews;

public class StoryDataTemplate() : DataTemplate(CreateGrid)
{
	static Grid CreateGrid() => new()
	{
		RowSpacing = 1,

		RowDefinitions = Rows.Define(
			(Row.Title, 20),
			(Row.Description, 20),
			(Row.BottomPadding, 1)),

		Children =
		{
			new Label { LineBreakMode = LineBreakMode.TailTruncation }
				.Row(Row.Title).Top()
				.Font(size: 16).TextColor(ColorConstants.TextCellTextColor)
				.Paddings(10, 0, 10, 0)
				.Bind(Label.TextProperty, static (StoryModel m) => m.Title, mode: BindingMode.OneTime),

			new Label()
				.Row(Row.Description)
				.Font(size: 13).TextColor(ColorConstants.TextCellDetailColor)
				.Paddings(10, 0, 10, 0)
				.Bind(Label.TextProperty, static (StoryModel m) => m.Description, mode: BindingMode.OneTime)
		}
	};

	enum Row { Title, Description, BottomPadding }
}