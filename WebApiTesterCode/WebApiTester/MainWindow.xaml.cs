using System;
using System.Collections.Generic;
using System.IO;
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
                NoticeTextbox.Text = "Failed to establish connection with web api. \nPlease double check url.";
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
        private async void Button_RetrieveBatch(object sender, RoutedEventArgs e)
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
                HttpResponseMessage response = await client.GetAsync($"api/Batch/Retrieve/{batchID}");
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }


        private async void Button_BatchStatus(object sender, RoutedEventArgs e)
        {
            
            string result = "";
            List<int> batchIdList = new List<int>();

            string[] stringBatchIds = BatchIDTextbox.Text.Split(',');

            foreach (var stringBatchId in stringBatchIds)
            {
                if (string.IsNullOrWhiteSpace(stringBatchId)) continue;
                int batchID = 0;
                bool canConvert = int.TryParse(stringBatchId, out batchID);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input BatchID array contains one or more invalid integers.";
                    return;
                }
                batchIdList.Add(batchID);
            }

            string workflow = WorkflowTextbox.Text;
            string state = StateTextbox.Text;
            
            //string batchIdParams = JsonConvert.SerializeObject(intBatchIds);
            int workflowId = 0;

            if (!string.IsNullOrWhiteSpace(workflow))
            {
                bool canConvert = int.TryParse(WorkflowTextbox.Text, out workflowId);
                if (!canConvert)
                {
                    NoticeTextbox.Text = "The input workflowID is not a valid integer.";
                    return;
                }
            }

            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            var batchIdUrl = "";
            foreach (var batchId in batchIdList)
            {
                batchIdUrl += $"&batchIds={batchId}";
            }
            
            SetupWebClient(url, false, false);

            try
            {
                var requestUrl = $"api/Batch/Status?workflowId={workflowId}&state={state}" + batchIdUrl;
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
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
                    NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
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

        /// <summary>
        /// Batch Status call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ReleaseData(object sender, RoutedEventArgs e)
        {
            int batchID = 0;
            bool canConvert = int.TryParse(BatchIDTextbox.Text, out batchID);
            if (!canConvert)
            {
                NoticeTextbox.Text = "The input BatchID is not a valid integer.";
                return;
            }

            var includeOcr = IncludeOcrCheckBox.IsChecked == true;  // If set to be true, include Ocr information
            var stream = SavePackageCheckBox.IsChecked == true;       // if stream is set to true, you will save the package as a zipped file. Otherwise, display filepath only.

            string result = "";
            string intellidactId = IntellidactIDTextbox.Text;
            string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
            SetupWebClient(url, false, false);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"api/Batch/Release/{batchID}?IncludeOCR={includeOcr}&stream={stream}&intellidactId={intellidactId}");
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    
                        var body = JObject.Parse(result);
                    if (stream)
                    {
                        var package = body["package"];
                        byte[] zippedPackage = Convert.FromBase64String(package.ToString());
                        string path = Path.GetTempPath();
                        File.WriteAllBytes(@path + $"\\Batch_{batchID}_Release_Package.zip", zippedPackage);
                        FileInfo file = new FileInfo(@path + $"\\Batch_{batchID}_Release_Package.zip");

                        NoticeTextbox.Text = "The zipped release package is saved to the local machine" + $"\nZip file path: \n{file.FullName}\n\n" + "Documents:\n"+body["documents"].ToString(Formatting.Indented);
                    }
                    else
                    {
                        NoticeTextbox.Text = body["documents"].ToString(Formatting.Indented);
                    }
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }

        private async void Button_ArchiveBatch(object sender, RoutedEventArgs e)
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
                var param = JsonConvert.SerializeObject(new { rejectReason = RejectReasonTextbox.Text });

                HttpResponseMessage response = await client.PostAsync($"api/Batch/Archive/{batchID}", null);
                result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    NoticeTextbox.Text = JValue.Parse(result).ToString(Formatting.Indented);
                }
                else
                {
                    NoticeTextbox.Text = response.ReasonPhrase + "\n" + result;
                }
            }
            catch (Exception ex)
            {
                NoticeTextbox.Text = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }
    }

}

