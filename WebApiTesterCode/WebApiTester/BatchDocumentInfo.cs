namespace WebApiTester
{
    public class BatchDocumentInfo
    {
        #region Constructors
      
        public BatchDocumentInfo(string name, string file, string data, string docType)
        {
            this.name = name;
            this.file = file;
            this.data = data;
            this.docType = docType;
        }
        #endregion
        #region Properties
        /// <summary>
        /// File Path
        /// </summary>
        public string file { get; set; }

        public string name { get; set; }
        public string data { get; set; }

        public string docType { get; set; }

        #endregion
    }
}
