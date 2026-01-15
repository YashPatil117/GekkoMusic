namespace GekkoMusic
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }
        private async void GoBackCommand(object sender, PointerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
