namespace GekkoMusic.Cards;

public partial class LikedSongs : ContentPage
{
	public LikedSongs()
	{
		InitializeComponent();
	}
    private async void GoBackCommand(object sender, PointerEventArgs e)
    {
        await Navigation.PopAsync();
    }
}