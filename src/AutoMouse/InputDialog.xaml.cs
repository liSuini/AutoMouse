using System.Windows;
using System.Windows.Controls;

namespace AutoMouse;

public partial class InputDialog : Window
{
    public string? InputText { get; private set; }

    public InputDialog(string title, string prompt, string defaultValue)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        InputBox.Text = defaultValue;
        InputBox.SelectAll();
        InputBox.Focus();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        InputText = InputBox.Text;
        DialogResult = true;
    }

    private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            InputText = InputBox.Text;
            DialogResult = true;
        }
    }
}
