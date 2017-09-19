using Newtonsoft.Json;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CustomVision
{
    public partial class MainPage : ContentPage
    {
        private MediaFile _foto = null;
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ElegirImage(object sender, EventArgs e)
        {
            await Plugin.Media.CrossMedia.Current.Initialize();

            _foto = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions());
            Img.Source = FileImageSource.FromFile(_foto.Path);
        }

        private async void TomarFoto(object sender, EventArgs e)
        {
            await Plugin.Media.CrossMedia.Current.Initialize();

            _foto = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
            {
                Directory = "Vision",
                Name = "Target.jpg"
            });
            Img.Source = FileImageSource.FromFile(_foto.Path);
        }

        private async void Clasificar(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Clasificando..."))
            {
                if (_foto == null) return;

                var stream = _foto.GetStream();

                var httpClient = new HttpClient();
                var url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/701b4eed-c0b1-4f4d-86af-873a7b4ad6a3/image";
                httpClient.DefaultRequestHeaders.Add("Prediction-Key", "c1561b9c08134ce69773e368ace40fe6");

                var content = new StreamContent(stream);

                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {

                }

                var json = await response.Content.ReadAsStringAsync();

                var c = JsonConvert.DeserializeObject<ClasificationResponse>(json);

                var p = c.Predictions.FirstOrDefault();
                if (p == null) Acr.UserDialogs.UserDialogs.Instance.Toast("Image not found");

                ResponseLabel.Text = $"{p.Tag} - {p.Probability:p0}";
                Accuracy.Progress = p.Probability;
            }

            Acr.UserDialogs.UserDialogs.Instance.Toast("Clasification finished...");
        }
    }


    public class ClasificationResponse
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public DateTime Created { get; set; }
        public Prediction[] Predictions { get; set; }
    }

    public class Prediction
    {
        public string TagId { get; set; }
        public string Tag { get; set; }
        public float Probability { get; set; }
    }

}
