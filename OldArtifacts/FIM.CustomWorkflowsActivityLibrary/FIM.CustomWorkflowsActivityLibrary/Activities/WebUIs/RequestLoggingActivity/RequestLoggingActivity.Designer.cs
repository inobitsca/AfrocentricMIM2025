using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;

namespace FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity
{
    public partial class RequestLoggingActivity
    {
        #region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCode("", "")]
        private void InitializeComponent()
        {
            this.CanModifyActivities = true;
            System.Workflow.ComponentModel.ActivityBind activitybind1 = new System.Workflow.ComponentModel.ActivityBind();
            this.LogRequestDataToFile = new System.Workflow.Activities.CodeActivity();
            this.ReadCurrentRequestActivity = new Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity();
            // 
            // LogRequestDataToFile
            // 
            this.LogRequestDataToFile.Name = "LogRequestDataToFile";
            this.LogRequestDataToFile.ExecuteCode += new System.EventHandler(this.LogRequestDataToFile_ExecuteCode);
            // 
            // ReadCurrentRequestActivity
            // 
            activitybind1.Name = "RequestLoggingActivity";
            activitybind1.Path = "ReadCurrentRequestActivity_CurrentRequest";
            this.ReadCurrentRequestActivity.Name = "ReadCurrentRequestActivity";
            this.ReadCurrentRequestActivity.SetBinding(Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity.CurrentRequestProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind1)));
            // 
            // RequestLoggingActivity
            // 
            this.Activities.Add(this.ReadCurrentRequestActivity);
            this.Activities.Add(this.LogRequestDataToFile);
            this.Name = "RequestLoggingActivity";
            this.CanModifyActivities = false;

        }

        #endregion

        private CodeActivity LogRequestDataToFile;

        private Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity ReadCurrentRequestActivity;

    }
}
