using Microsoft.UI.Xaml.Media.Animation;

namespace DataAnonymizer.Consts;

internal static class TransitionInfo
{
    public static NavigationTransitionInfo Default => DrillIn;

    public static NavigationTransitionInfo Common => new CommonNavigationTransitionInfo();
    public static NavigationTransitionInfo Continuum => new ContinuumNavigationTransitionInfo();
    public static NavigationTransitionInfo DrillIn => new DrillInNavigationTransitionInfo();
    public static NavigationTransitionInfo Entrance => new EntranceNavigationTransitionInfo();
    public static NavigationTransitionInfo Slide => new SlideNavigationTransitionInfo();
    public static NavigationTransitionInfo Suppress => new SuppressNavigationTransitionInfo();
}