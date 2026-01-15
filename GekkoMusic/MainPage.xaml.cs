using GekkoMusic.Services;
using GekkoMusic.ViewModels;
using Plugin.Maui.Audio;

namespace GekkoMusic;

public partial class MainPage : ContentPage
{
    private readonly PlayerViewModel _viewModel;
   
    
    // Hover colors
    private static readonly Color HoverColor =
        Colors.LightGray.WithAlpha(0.3f);

    private static readonly Color NormalColor =
        Color.FromArgb("#181818");


    //public MainPage(IAudioManager audioManager)
    //{
    //    InitializeComponent();

    //    //var audioService = new AudioPlayerService(audioManager);
    //    //_viewModel = new PlayerViewModel(audioService);

    //    ///var audioService = new AudioPlayerService(audioManager);
    //   // var ytService = new YoutubeDlpService();

    //    // 2. Create ViewModel
    //    //var viewModel = new PlayerViewModel(audioService, ytService);

    //    // 3. Assign BindingContext
    //    //BindingContext = viewModel;


    //}
    public MainPage(PlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PlayerViewModel vm)
            await vm.InitializeAsync();
    }


    // =========================
    // UI-ONLY CODE 
    // =========================

    private async void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
            await AnimateBackgroundColor(border, NormalColor, HoverColor);
    }

    private async void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
            await AnimateBackgroundColor(border, HoverColor, NormalColor);
    }

    private async void OnPointerEnteredicon(object sender, PointerEventArgs e)
    {
        if(sender is VisualElement view)
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
    private async void OnLabelTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Cards.LikedSongs());
    }


    private Task AnimateBackgroundColor(Border border, Color from, Color to, uint duration = 150)
    {
        border.AbortAnimation("BgColorAnimation");

        var animation = new Animation(v =>
        {
            border.BackgroundColor = new Color(
                (float)(from.Red + (to.Red - from.Red) * v),
                (float)(from.Green + (to.Green - from.Green) * v),
                (float)(from.Blue + (to.Blue - from.Blue) * v),
                (float)(from.Alpha + (to.Alpha - from.Alpha) * v)
            );
        });

        border.Animate("BgColorAnimation", animation, length: duration, easing: Easing.CubicInOut);
        return Task.CompletedTask;
    }
}
