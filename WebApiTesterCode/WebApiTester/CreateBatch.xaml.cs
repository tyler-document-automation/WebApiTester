using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApiTester
{
    /// <summary>
    /// Interaction logic for CreateBatch.xaml
    /// </summary>
    public partial class CreateBatch
    {
        private static string FilePath = "";
        public CreateBatch()
        {
            InitializeComponent();

        }

        private void ButtonBrowseClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image Files (*.tif, *.pdf)|*.tif; *.pdf|All Files|*.*"
            };

            // Show open file dialog box
            var result = dlg.ShowDialog();
            // Process open file dialog box results
            if (result == true)
            {
                // Open document 
                if (FilePathCheckBox.IsChecked == true)
                {
                    FilePath = dlg.FileName;
                    FilePathTextbox.Text = dlg.FileName;
                }
                else
                {
                    FilePath = dlg.FileName;
                    byte[] bytes = File.ReadAllBytes(dlg.FileName);
                    FilePathTextbox.Text = Convert.ToBase64String(bytes);

                }

            }
        }

        private void Button_CreateBatch(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            try
            {
                var batchdoc = new BatchDocumentInfo((FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? "" : Path.GetFileName(FilePath), (FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? FilePath : "", (FilePathCheckBox.IsChecked != null && FilePathCheckBox.IsChecked == true) ? "" : FilePathTextbox.Text, DocTypeTextbox.Text);
                int workflowId = 0;

                if (!string.IsNullOrWhiteSpace(WorkflowTextbox.Text))
                {
                    bool canConvert = int.TryParse(WorkflowTextbox.Text, out workflowId);
                    if (!canConvert)
                    {
                        StatusLabel.Content = "The input workflowID is not a valid integer.";
                        return;
                    }
                }
                var batchDocs = new List<BatchDocumentInfo>{batchdoc}.ToArray();
                string url = ((MainWindow)Application.Current.MainWindow).WebApiTextbox.Text;
                BatchCreateInfo batchCreateInfo = new BatchCreateInfo(
                    ClassNameTextbox.Text,
                    BatchNameTextbox.Text,
                    int.Parse(WorkflowTextbox.Text),
                    RunIDTextbox.Text,
                    batchDocs
                );

                StatusLabel.Content = "Creating Batch...";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           
                var param = JsonConvert.SerializeObject(batchCreateInfo);
                HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");

                var response = client.PostAsync("/api/Batch/Create", contentPost).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    var body = JObject.Parse(result);
                    var BatchID = body["batchId"];

                    ((MainWindow) Application.Current.MainWindow).BatchIDTextbox.Text = BatchID.ToString();
                    ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                        $"Batch ID: {BatchID} successfully created for batch {batchCreateInfo.name}."
                        + Environment.NewLine
                        + JValue.Parse(result).ToString(Formatting.Indented);
                    StatusLabel.Content = $"Batch ID: {BatchID} successfully created";
                }
                else
                {
                    StatusLabel.Content = $"Failed to create the batch {batchCreateInfo.name}. \nSee main windows for detailed error message.";
                    ((MainWindow) Application.Current.MainWindow).BatchIDTextbox.Text = "";

                    try
                    {
                        ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                            ($"Failed to create the batch {batchCreateInfo.name}. Exception: {result}");
                    }
                    catch (Exception)
                    {
                        ((MainWindow) Application.Current.MainWindow).NoticeTextbox.Text =
                            ($"Failed to create the batch {batchCreateInfo.name}. No result from the Web Api.");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Content = $"Error: {ex.Message}. \nPlease double check url and parameters";
            }
        }

        private void BatchNameTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
