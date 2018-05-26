using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;


using System.IO;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;

using System.Web;
using System.Collections.Generic;

namespace cameraTest
{

  
    public partial class MainPage : ContentPage
    {
       
        int flag = 0;
       

        public MainPage()
        {
            InitializeComponent();

            CameraButton.Clicked += CameraButton_Clicked;
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
           
            
            //var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });

            //if (photo != null)
                //PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                
            /*
                
             
                if (flag == 0)
                    flag = 1;
                else
                    flag = 0; 
              
             
                if(flag == 1)
                    PhotoImage.Source = ImageSource.FromFile("/Users/pranav/Desktop/nemo.jpeg");
                else
                    PhotoImage.Source = ImageSource.FromFile("/Users/pranav/Desktop/turtle.jpeg");
            */
           

            Image ima = new Image();
               ima =  PhotoImage;

            String imageUrl = ima.Source.ToString();

            imageUrl = imageUrl.Substring(6);


        
            //making a request to the cognative api
            MakeRequest(imageUrl,Labb);
         


        }


        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async void MakeRequest(string imageFilePath ,Label Labb)
        {
            var client = new HttpClient();

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["returnFaceId"] = "true";
            queryString["returnFaceLandmarks"] = "false";
            queryString["returnFaceAttributes"] = "emotion";

            var uri = "https://westus.api.cognitive.microsoft.com/face/v1.0/detect?" + queryString;


            // Request headers - replace this example key with your valid key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "2599c6dcb66c484e8feb029d3f1362e5"); // 

            // NOTE: You must use the same region in your REST call as you used to obtain your subscription keys.
            //   For example, if you obtained your subscription keys from westcentralus, replace "westus" in the 
            //   URI below with "westcentralus".
            // "https://westus.api.cognitive.microsoft.com/face/v1.0/detect";
            //"https://westus.api.cognitive.microsoft.com/face/v1.0/detect";

            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                responseContent = response.Content.ReadAsStringAsync().Result;

            }
           
            // Processing the JSON into manageable objects.
            JToken rootToken = JArray.Parse(responseContent).First;

            // First token is always the faceRectangle identified by the API.
            JToken faceRectangleToken = rootToken.First;

            // Second token is all emotion scores.
            JToken scoresToken = rootToken.Last;




            // Show all face rectangle dimensions
            JEnumerable<JToken> faceRectangleSizeList = faceRectangleToken.First.Children();
            foreach (var size in faceRectangleSizeList)
                Console.WriteLine(size);
            

            // Show all scores
            JEnumerable<JToken> scoreList = scoresToken.First.Children();

            //the varible that holds the max value of the emotion returned by the congnative api
            double max = 0.0;

            //string that holds the emotion from the condnative api
            String emo = "";


            //string holds the convertion from a Jtoken to a json string
            String jtokStr = "";
           
            foreach (var score in scoreList)
                jtokStr = score.ToString();
               

            jtokStr = jtokStr.Substring(11);

           

            var values = JsonConvert.DeserializeObject<Dictionary<string, Double>>(jtokStr);




            foreach (KeyValuePair<string, Double> item in values)
            {

                if (item.Value > max)
                    emo = item.Key;
                
            }

            Labb.Text = emo;
           
        }


    }
}
