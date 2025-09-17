namespace ReadingList.Tui.Configuration;

public static class UiConstants
{
    // Frame dimensions
    public static class Frames
    {
        public const int DefaultMargin = 1;
        public const int DefaultSpacing = 2;
        public const int SmallFrameHeight = 4;
        public const int MediumFrameHeight = 8;
        public const int LargeFrameHeight = 12;
        public const int ExtraLargeFrameHeight = 18;
    }

    // Dialog dimensions
    public static class Dialogs
    {
        public const int SmallWidth = 50;
        public const int MediumWidth = 60;
        public const int LargeWidth = 70;
        public const int ExtraLargeWidth = 80;
        public const int VeryLargeWidth = 85;
        
        public const int SmallHeight = 7;
        public const int MediumHeight = 8;
        public const int LargeHeight = 18;
        public const int ExtraLargeHeight = 20;
        public const int VeryLargeHeight = 22;
        public const int HugeHeight = 25;
        public const int MaxHeight = 30;
    }

    // Control sizes
    public static class Controls
    {
        public const int TextFieldWidth = 25;
        public const int LargeTextFieldWidth = 30;
        public const int SmallTextFieldWidth = 12;
        public const int DateFieldWidth = 12;
        public const int NumberFieldWidth = 8;
        
        public const int ComboBoxWidth = 20;
        public const int LargeComboBoxWidth = 25;
        public const int SmallComboBoxWidth = 15;
        
        public const int ComboBoxHeight = 6;
        public const int LargeComboBoxHeight = 8;
        public const int SmallComboBoxHeight = 7;
        
        public const int TextViewHeight = 7;
    }

    // List counts for data display
    public static class DataLimits
    {
        public const int DefaultTopRatedCount = 5;
        public const int DefaultRecentCompletedCount = 5;
        public const int DefaultTopRatedBooksCount = 10;
        public const int MaxGoalsDisplayed = 3;
        public const int MaxRecentActivityItems = 4;
    }

    // Positioning
    public static class Layout
    {
        public const int StatusBarOffset = 1;
        public const int MenuBarHeight = 1;
        public const int BottomButtonOffset = 3;
        public const int MainMenuCenterOffset = 18;
        public const int MainMenuHeight = 12;
        public const int MainMenuWidth = 36;
    }
}