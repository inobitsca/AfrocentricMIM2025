using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.ChangeActorId
{
    public partial class ChangeActorId
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
            this.UpdateUser = new Microsoft.ResourceManagement.Workflow.Activities.UpdateResourceActivity();
            this.InitialiseUpdateUser = new System.Workflow.Activities.CodeActivity();
            this.ReadUser = new Microsoft.ResourceManagement.Workflow.Activities.ReadResourceActivity();
            this.InisialiseReadUser = new System.Workflow.Activities.CodeActivity();
            this.ReadCurrentRequestActivity = new Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity();
            // 
            // UpdateUser
            // 
            this.UpdateUser.ActorId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.UpdateUser.ApplyAuthorizationPolicy = false;
            this.UpdateUser.Name = "UpdateUser";
            this.UpdateUser.ResourceId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.UpdateUser.UpdateParameters = null;
            // 
            // InitialiseUpdateUser
            // 
            this.InitialiseUpdateUser.Name = "InitialiseUpdateUser";
            this.InitialiseUpdateUser.ExecuteCode += new System.EventHandler(this.InitialiseUpdateUser_ExecuteCode);
            // 
            // ReadUser
            // 
            this.ReadUser.ActorId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.ReadUser.Name = "ReadUser";
            this.ReadUser.Resource = null;
            this.ReadUser.ResourceId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.ReadUser.SelectionAttributes = null;
            // 
            // InisialiseReadUser
            // 
            this.InisialiseReadUser.Name = "InisialiseReadUser";
            this.InisialiseReadUser.ExecuteCode += new System.EventHandler(this.InisialiseReadUser_ExecuteCode);
            // 
            // ReadCurrentRequestActivity
            // 
            activitybind1.Name = "ChangeActorId";
            activitybind1.Path = "ReadCurrentRequestActivity_CurrentRequest";
            this.ReadCurrentRequestActivity.Name = "ReadCurrentRequestActivity";
            this.ReadCurrentRequestActivity.SetBinding(Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity.CurrentRequestProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind1)));
            // 
            // ChangeActorId
            // 
            this.Activities.Add(this.ReadCurrentRequestActivity);
            this.Activities.Add(this.InisialiseReadUser);
            this.Activities.Add(this.ReadUser);
            this.Activities.Add(this.InitialiseUpdateUser);
            this.Activities.Add(this.UpdateUser);
            this.Name = "ChangeActorId";
            this.CanModifyActivities = false;

        }

        #endregion

        private CodeActivity InisialiseReadUser;

        private Microsoft.ResourceManagement.Workflow.Activities.UpdateResourceActivity UpdateUser;

        private CodeActivity InitialiseUpdateUser;

        private Microsoft.ResourceManagement.Workflow.Activities.ReadResourceActivity ReadUser;

        private Microsoft.ResourceManagement.Workflow.Activities.CurrentRequestActivity ReadCurrentRequestActivity;














    }
}
