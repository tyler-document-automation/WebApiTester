namespace WebApiTester 
{
    public class BatchCreateInfo
    {
        #region Constructors
        /// <summary>
        /// Create a batch with provided information
        /// </summary>
        /// <param name="className"></param>
        /// <param name="name"></param>
        /// <param name="workflowId"></param>
        /// <param name="runId"></param>
        /// <param name="documents"></param>
        /// <param name="priority"></param>
        public BatchCreateInfo(string className, string name, int workflowId, string runId, BatchDocumentInfo[] documents)
        {
            this.className = className;
            this.name = name;
            this.workflowId = workflowId;
            this.runId = runId;
            //this.priority = priority;
            this.documents = documents;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Batch Definition Class Name
        /// </summary>
        public string className { get; set; }

        /// <summary>
        /// Batch Name
        /// </summary>
        public string name { get; set; }


        /// <summary>
        /// Batch priority
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// Workflow Id
        /// </summary>
        public int workflowId { get; set; }

        /// <summary>
        /// File Information array
        /// </summary>
        public BatchDocumentInfo[] documents { get; set; }

        /// <summary>
        /// runID, the file drop folder to run different experiments 
        /// </summary>
        public string runId { get; set; }

        
        #endregion
    }
}
