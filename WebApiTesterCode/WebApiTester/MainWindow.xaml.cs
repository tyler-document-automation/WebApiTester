using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApiTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static HttpClient client;
        //string defaultURL = "http://localhost:23760/";
        string defaultURL = $"http://{Environment.MachineName}:8037/";

        /// <summary>
        /// web api client set up and initialize user interface 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetupWebClient(defaultURL, true, false);
            
        }

        private async void SetupWebClient(string url, bool showOkStatus, bool getWithBody)
        {
            if (getWithBody)
            {
                var handler = new WinHttpHandler();
                client = new HttpClient(handler);
            }
            else client = new HttpClient();
            client.BaseAddress = new Uri(url);
            // Environment.MachineName
            WebApiTextbox.Text = client.BaseAddress.ToString();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (!showOkStatus) return;
            try
            {
                HttpResponseMessage response = await client.GetAsync("/");
                //var result = await response.Content.ReadAsStringAsync();
                NoticeTextbox.Text = "Status: "+ response.ReasonPhrase;

            }
            catch
            {
                NoticeTextbox.Text = "Failed to establish connection with web api. \nPlease double check url";
                //throw new Exception("Failed to establish connection with web api. \nPlease double check url");
            }
        }

        /// <summary>
        /// Create Batch call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CreateBatch(object sender, RoutedEventArgs e)
        {
            var Page2 = new CreateBatch(); //create your new form.
            Page2.ShowDialog(); //show the new form.
           // Page2.Close();
        }

        /// <summary>
        /// Batch Status call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_BatchStatus(object sender, RoutedEventArgs e)
        {
            int batchID = 0;
            bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
            if (!canConvert)
            {
                NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                return;
            }

            string result = "";

            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            SetupWebClient(url, false, false);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/Batch/Status/{batchID}");
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }


        ///// <summary>
        ///// Extraction indexing data
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void Button_Extraction(object sender, RoutedEventArgs e)
        //{
        //    int batchID = 0;
        //    bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
        //    if (!canConvert)
        //    {
        //        NoticeTextbox.Text = "The input BatchID is not a valid integer.";
        //        return;
        //    }

        //    string result = "";

        //    string url = ((MainWindow) Application.Current.MainWindow).WebApiTextbox.Text;
        //    SetupWebClient(url, false, false);
        //    try
        //    {
            

        //    //HttpResponseMessage response = await client.GetAsync($"api/Batch/Extraction/{batchID}");
        //    HttpResponseMessage response = await client.GetAsync($"api/Batch/GetBatchData/?batchId={batchID}");

        //    result = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //    {
        //        NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
        //    }
        //    else
        //    {
        //        NoticeTextbox.Text = response.ReasonPhrase;
        //    }
        //    }
        //    catch
        //    {
        //        NoticeTextbox.Text = "Error: No result from the Web Api. \nPlease double check url and parameters";
        //    }

        //    // return result;
        //}

        ///// <summary>
        ///// Batch Redaction Status 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private async void Button_Redaction(object sender, RoutedEventArgs e)
        //{
        //    int batchID = 0;
        //    bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
        //    if (!canConvert)
        //    {
        //        NoticeTextbox.Text = "The input BatchID is not a valid integer.";
        //        return;
        //    }

        //    string result = "";

        //    string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
        //    SetupWebClient(url,false,false);

        //    try { 


        //    //HttpResponseMessage response = await client.GetAsync($"api/Batch/Redaction/{batchID}");
        //    HttpResponseMessage response = await client.GetAsync($"api/Batch/GetBatchRedactions/?idactBatchId={batchID}");

        //    result = await response.Content.ReadAsStringAsync();
        //    if (response.IsSuccessStatusCode)
        //    {
        //        NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
        //    }
        //    else
        //    {
        //        NoticeTextbox.Text = response.ReasonPhrase;
        //    }
        //    }
        //    catch
        //    {
        //        NoticeTextbox.Text = "Error: No result from the Web Api. \nPlease double check url and parameters";
        //    }
        //}


        private async void Button_BatchCollection(object sender, RoutedEventArgs e)
        {
            List<int> batchIdList = new List<int>();

            string[]  stringBatchIds = BatchIDTextbox.Text.Split(',');

            foreach (var stringBatchId in stringBatchIds)
            {
                int batchID = 0;
                bool canConvert = int.TryParse(stringBatchId, out batchID);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input BatchID array contains one or more invalid integers.";
                    return;
                }
                batchIdList.Add(batchID);
            }

            int[] intBatchIds = batchIdList.ToArray();

            string result = "";

            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            SetupWebClient(url, false, true);
            try
            {

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(WebApiTextbox.Text + "api/Batch/Status/Collection"),
                    Method = HttpMethod.Get,
                };
                var param = JsonConvert.SerializeObject(new {batchIds = intBatchIds});
                request.Content = new StringContent(param, Encoding.UTF8, "application/json");
                //request.Content
                HttpResponseMessage response = await client.SendAsync(request);
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }


        private async void Button_ActiveBatch(object sender, RoutedEventArgs e)
        {
            
            string result = "";

            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            SetupWebClient(url, false, false);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/Batch/Status/Active");
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }


        private async void Button_CancelBatch(object sender, RoutedEventArgs e)
        {
            int batchID = 0;
            bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
            if (!canConvert)
            {
                NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                return;
            }

            string result = "";

            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            SetupWebClient(url, false, false);
            try
            {
                var param = JsonConvert.SerializeObject(new {rejectReason = RejectReasonTextbox.Text});
                HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");


                HttpResponseMessage response = await client.PostAsync($"api/Batch/Cancel/{batchID}", contentPost);
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }

        private void WebApiTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //url = WebApiTextbox.Text;
            //RunAsync();

        }

        private void Button_Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(NoticeTextbox.Text);

        }

        private void Button_TestConnect(object sender, RoutedEventArgs e)
        {
            SetupWebClient(WebApiTextbox.Text, true, false);
        }
    }

}

