using Microsoft.VisualBasic;

namespace CPADProject
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private async void StartBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PlayPage());
        }
    }
}
