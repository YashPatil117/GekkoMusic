using GekkoMusic.ViewModels;
using GekkoMusic.Services;
namespace GekkoMusic.Cards;


public partial class LikedSongs : ContentPage
{
    public LikedSongs()
    {
        InitializeComponent();

        // manual but VALID
        var storage = new LikedSongStorageService();
        BindingContext = new LikedSongsViewModel(storage);
    }
    //public LikedSongs(LikedSongsViewModel vm)
    //{
    //	InitializeComponent();
    //       BindingContext = vm;
    //   }
    private async void OnPointerEnteredicon(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement view)
        {
            await view.ScaleTo(1.2, 100, Easing.CubicOut);
        }
    }
    private async void OnPointerExitedicon(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement view)
        {
            await view.ScaleTo(1.0, 100, Easing.CubicOut);


        }
    }
    private static readonly Color NormalColor = Color.FromArgb("#121212");
    private static readonly Color HoverColor = Color.FromArgb("#1A1A1A");

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Grid g)
            g.BackgroundColor = HoverColor;
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Grid g)
            g.BackgroundColor = NormalColor;
    }

}