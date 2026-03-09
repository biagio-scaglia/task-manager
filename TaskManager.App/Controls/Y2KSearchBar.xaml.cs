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

    public static readonly RoutedEvent SearchTextChangedEvent =
        EventManager.RegisterRoutedEvent("SearchTextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Y2KSearchBar));

    public event RoutedEventHandler SearchTextChanged
    {
        add { AddHandler(SearchTextChangedEvent, value); }
        remove { RemoveHandler(SearchTextChangedEvent, value); }
    }

    public Y2KSearchBar()
    {
        InitializeComponent();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchText = SearchBox.Text;
        RaiseEvent(new RoutedEventArgs(SearchTextChangedEvent));
    }
}
