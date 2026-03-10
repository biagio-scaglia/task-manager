using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace TaskManager.App.Controls;

public partial class Y2KSearchBar : UserControl
{
    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register("SearchText", typeof(string), typeof(Y2KSearchBar), 
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string SearchText
    {
        get { return (string)GetValue(SearchTextProperty); }
        set { SetValue(SearchTextProperty, value); }
    }

    public Y2KSearchBar()
    {
        InitializeComponent();
    }
}
