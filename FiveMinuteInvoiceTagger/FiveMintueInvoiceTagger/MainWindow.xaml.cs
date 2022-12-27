using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using PdfLibCore;
using PdfLibCore.Enums;

namespace FiveMintueInvoiceTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)  
        {  
            try
            {
                ErrorMsg.Content = "";

                //we only want pdf invoices
                Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog(); 
                openFileDlg.Filter = "Pdf Files|*.pdf";
                // Launch OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = openFileDlg.ShowDialog();
                // Get the selected file name and display in a TextBox.
                // Load content of file in a TextBlock
                if (result == true)
                {
                    //Upload to azure blob so Azure AI can access file
                    string invoicePath = await UploadInvoiceForProcessing(openFileDlg.FileName);

                    //perform Invoice tagging
                    Task<List<InvoiceProperty>> invoicePropertiesTask = GetDocumentProperties(invoicePath);

                    //Convert PDF to image so we can view it next to properties
                    UpdateInvoiceImage(openFileDlg.FileName);

                    //Wait for Azure to return results, set it to our data grid
                    DocumentProperties.ItemsSource = await invoicePropertiesTask;

                }
            }
            catch(Exception ex)
            {
                ErrorMsg.Content = ex.Message;
            }
        }

        //Calls azure ai prebuilt model service for invoices
        private async Task<List<InvoiceProperty>> GetDocumentProperties(string InvoicePath)
        {
            
            List<InvoiceProperty> invoiceProperties = new List<InvoiceProperty>();

            //Endpoint and key found in Azure AI service
            string endpoint = "<ai service url>";
            string key = "<ai service key";
            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            //create Uri for the invoice
            Uri invoiceUri = new Uri(InvoicePath);

            //Analyzes the invoice
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", invoiceUri);
            AnalyzeResult result = operation.Value;

            //iterate the results and populates list of field values
            for (int i = 0; i < result.Documents.Count; i++)
            {
                AnalyzedDocument document = result.Documents[i];
                foreach(string field in document.Fields.Keys)
                {
                    DocumentField documentField = document.Fields[field];
                    InvoiceProperty invoiceProperty = new InvoiceProperty()
                        {
                          Field = field,
                          Value = documentField.Content,
                          Score = documentField.Confidence?.ToString()
                        };

                        invoiceProperties.Add(invoiceProperty);
                    }
            }

            return invoiceProperties;
        }

        //upload to azure blob
        private async Task<string> UploadInvoiceForProcessing(string FilePath)
        {
            string cs = "<Blob Sorage Connection String>";
            string fileName = System.IO.Path.GetFileName(FilePath);
            Console.WriteLine("File name {0}", fileName);
            //customer is the name of our blob container where we can view documents in Azure
            //blobs require us to create a connection each time we want to upload a file
            BlobClient blob  = new BlobClient(cs, "invoice", fileName); 

            //Gets a file stream to upload to Azure
            using(FileStream stream = File.Open(FilePath, FileMode.Open))
            {
                var blobInfo = await blob.UploadAsync(stream);
                
            }
            
            return "<Blob Storage base URL>" + fileName;
        }

        //converts PDF to image
        private void UpdateInvoiceImage(string FilePath)
        {
            using(var pdf = new PdfDocument(File.Open(FilePath, FileMode.Open)))
            {
                //for this example we only want to show the first page
                if(pdf.Pages.Count > 0)
                {
                    var pdfPage = pdf.Pages[0];

                    var dpiX= 600D;
                    var dpiY = 600D;
                    var pageWidth = (int) (dpiX * pdfPage.Size.Width / 72);
                    var pageHeight = (int) (dpiY * pdfPage.Size.Height / 72);
                
                    var bitmap = new PdfiumBitmap(pageWidth, pageHeight, true);                                

                    pdfPage.Render(bitmap, PageOrientations.Normal, RenderingFlags.LcdText);
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = bitmap.AsBmpStream(dpiX,dpiY);
                    image.EndInit();
                    InvoiceImage.Source = image;
                }
                
            }
        }
    }

    public class InvoiceProperty
    {
        //Field name found on invoice
        public string Field {get;set;}
        //Field value
        public string Value {get;set;}
        //How confident AI is that field value is correct
        public string Score {get;set;}
    }
}
