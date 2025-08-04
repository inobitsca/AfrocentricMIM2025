using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Linq;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
//The following two namespaces define the FIM object model
using Microsoft.ResourceManagement.WebServices.WSResourceManagement;
using Microsoft.ResourceManagement.Workflow.Activities;


namespace FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity
{
    public partial class RequestLoggingActivity : SequenceActivity
    {
        public RequestLoggingActivity()
        {
            InitializeComponent();
        }

        #region Public Workflow Properties

        public static DependencyProperty ReadCurrentRequestActivity_CurrentRequestProperty = DependencyProperty.Register("ReadCurrentRequestActivity_CurrentRequest", typeof(Microsoft.ResourceManagement.WebServices.WSResourceManagement.RequestType), typeof(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity.RequestLoggingActivity));

        /// <summary>
        ///  Stores information about the current request
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        [BrowsableAttribute(true)]
        [CategoryAttribute("Misc")]
        public RequestType ReadCurrentRequestActivity_CurrentRequest
        {
            get
            {
                return ((Microsoft.ResourceManagement.WebServices.WSResourceManagement.RequestType)(base.GetValue(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity.RequestLoggingActivity.ReadCurrentRequestActivity_CurrentRequestProperty)));
            }
            set
            {
                base.SetValue(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity.RequestLoggingActivity.ReadCurrentRequestActivity_CurrentRequestProperty, value);
            }
        }

        /// <summary>
        ///  Identifies the Log File Path
        /// </summary>
        public static DependencyProperty LogFilePathProperty = DependencyProperty.Register("LogFilePath", typeof(System.String), typeof(RequestLoggingActivity));
        [Description("Please specify the Log File Path")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string LogFilePath
        {
            get
            {
                return ((String)(base.GetValue(RequestLoggingActivity.LogFilePathProperty)));
            }
            set
            {
                base.SetValue(RequestLoggingActivity.LogFilePathProperty, value);
            }
        }

        /// <summary>
        ///  Identifies the Log File Name
        /// </summary>
        public static DependencyProperty LogFileNameProperty = DependencyProperty.Register("LogFileName",
             typeof(System.String), typeof(RequestLoggingActivity));
        [Description("Please specify the Log File Path")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string LogFileName
        {
            get
            {
                return ((String)(base.GetValue(RequestLoggingActivity.LogFileNameProperty)));
            }
            set
            {
                base.SetValue(RequestLoggingActivity.LogFileNameProperty, value);
            }
        }
        #endregion

        #region Execution Logic

        /// <summary>
        ///  Defines the logic of the LogRequestDataToFile activity.
        ///  This code will be executed when the LogRequestDataToFile activity
        ///  becomes the active workflow.
        /// </summary>
        private void LogRequestDataToFile_ExecuteCode(object sender, EventArgs e)
        {
            try
            {
                //Get current request from previous activity
                RequestType currentRequest = this.ReadCurrentRequestActivity_CurrentRequest;

                // Output the Request type and object type
                this.Log("Request Operation: " + currentRequest.Operation);
                this.Log("Target Object Type: " + currentRequest.TargetObjectType);

                // As UpdateRequestParameter derives from CreateRequestParameter we can simplify the code by deriving
                // from CreateRequestParameter only.
                ReadOnlyCollection<CreateRequestParameter> requestParameters = currentRequest.ParseParameters<CreateRequestParameter>();

                // Loop through CreateRequestParameters and print out each attribute/value pair
                this.Log("Parameters for request: " + currentRequest.ObjectID);
                foreach (CreateRequestParameter requestParameter in requestParameters)
                {
                    if (requestParameter.Value != null)
                        this.Log("     " + requestParameter.PropertyName + ": " + requestParameter.Value.ToString());
                }

                // In order to read the Workflow Dictionary we need to get the containing (parent) workflow
                SequentialWorkflow containingWorkflow = null;
                if (!SequentialWorkflow.TryGetContainingWorkflow(this, out containingWorkflow))
                {
                    throw new InvalidOperationException("Unable to get Containing Workflow");
                }
                this.Log("Containing Workflow Dictionary (WorkflowData):");

                // Loop through Workflow Dictionary and log each attribute/value pair
                foreach (KeyValuePair<string, object> item in containingWorkflow.WorkflowDictionary)
                {
                    this.Log("     " + item.Key + ": " + item.Value.ToString());
                }
                this.Log("\n\n");
            }
            catch (Exception ex)
            {
                this.Log("Logging Activity Exception Thrown: " + ex.Message);
            }
        }
        #endregion

        #region Utility Functions

        // Prefix the current time to the message and log the message to the log file.
        private void Log(string message)
        {
            using (StreamWriter log = new StreamWriter(Path.Combine(this.LogFilePath, this.LogFileName), true))
            {
                log.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ": " + message);
                //since the previous line is part of a "using" block, the file will automatically
                //be closed (even if writing to the file caused an exception to be thrown).
                //For more information see
                // http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            }
        }
        #endregion


    }
}
