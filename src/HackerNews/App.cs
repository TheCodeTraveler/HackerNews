﻿using CommunityToolkit.Maui.Markup;

namespace HackerNews;

class App : Application
{
	readonly AppShell _appShell;
	
	public App(AppShell appShell)
	{
		_appShell = appShell;
		
		Resources = new ResourceDictionary()
		{
			new Style<Shell>(
				(Shell.NavBarHasShadowProperty, true),
				(Shell.TitleColorProperty, ColorConstants.NavigationBarTextColor),
				(Shell.DisabledColorProperty, ColorConstants.NavigationBarTextColor),
				(Shell.UnselectedColorProperty, ColorConstants.NavigationBarTextColor),
				(Shell.ForegroundColorProperty, ColorConstants.NavigationBarTextColor),
				(Shell.BackgroundColorProperty, ColorConstants.NavigationBarBackgroundColor)).ApplyToDerivedTypes(true),

			new Style<NavigationPage>(
				(NavigationPage.BarTextColorProperty, ColorConstants.NavigationBarTextColor),
				(NavigationPage.BarBackgroundColorProperty, ColorConstants.NavigationBarBackgroundColor)).ApplyToDerivedTypes(true)
		};
	}

	protected override Window CreateWindow(IActivationState? activationState) => new(_appShell);
}