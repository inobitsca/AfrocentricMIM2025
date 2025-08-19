using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
//The following two namespaces define the FIM object model
using Microsoft.ResourceManagement.WebServices.WSResourceManagement;
using Microsoft.ResourceManagement.Workflow.Activities;


namespace FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.ChangeActorId
{
    public partial class ChangeActorId : SequenceActivity
    {

        const string FIMAdminGuid = "7fb2b853-24f0-4498-9534-4e10589723c4";

        public ChangeActorId()
        {
            InitializeComponent();
        }

        #region Public Workflow Properties

        public static DependencyProperty ReadCurrentRequestActivity_CurrentRequestProperty = DependencyProperty.Register("ReadCurrentRequestActivity_CurrentRequest", typeof(Microsoft.ResourceManagement.WebServices.WSResourceManagement.RequestType), typeof(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.ChangeActorId.ChangeActorId));

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
                return ((Microsoft.ResourceManagement.WebServices.WSResourceManagement.RequestType)(base.GetValue(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.ChangeActorId.ChangeActorId.ReadCurrentRequestActivity_CurrentRequestProperty)));
            }
            set
            {
                base.SetValue(FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.ChangeActorId.ChangeActorId.ReadCurrentRequestActivity_CurrentRequestProperty, value);
            }
        }

        /// <summary>
        ///  Identifies the Actor ID GUID
        /// </summary>
        public static DependencyProperty ActorIdGuidProperty = DependencyProperty.Register("ActorIdGuid", typeof(System.String), typeof(ChangeActorId));
        [Description("Please specify the Actor ID GUID")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string ActorIdGuid
        {
            get
            {
                return ((String)(base.GetValue(ChangeActorId.ActorIdGuidProperty)));
            }
            set
            {
                base.SetValue(ChangeActorId.ActorIdGuidProperty, value);
            }
        }


        #endregion

        private void InisialiseReadUser_ExecuteCode(object sender, EventArgs e)
        {
            //Set the Actor ID for the Read Activity
            ReadUser.ActorId = new Guid(FIMAdminGuid);

            //Set the Resource to retrieve the current request object.
            //Set this to the target ID of the containing workflow
            ReadUser.ResourceId = ReadCurrentRequestActivity.CurrentRequest.Target.GetGuid();

            //Set the selection parameters
            ReadUser.SelectionAttributes = new string[] { "ProvisionRequestAD" };
        }

        private void InitialiseUpdateUser_ExecuteCode(object sender, EventArgs e)
        {
            //Get the object that was read using the Read Activity
            ResourceType user = ReadUser.Resource;

            //Get the ProvisionRequestAD
            string myProvisionReaquestAD = (string)user["ProvisionRequestAD"];
            string ProvisionRequestAD = "";

            //Place logic here

            if ((myProvisionReaquestAD == "Not Approved") | (myProvisionReaquestAD == "Request Approval"))
            {
                ProvisionRequestAD = "Approved";


                //Set the actor ID. This is set in the FIM Custom Activity UI and used to trigger the MPR for the Approval Workflow
                UpdateUser.ActorId = new Guid(ActorIdGuid.ToString());
                UpdateUser.ApplyAuthorizationPolicy = true;
                UpdateUser.ResourceId = ReadCurrentRequestActivity.CurrentRequest.Target.GetGuid();

                //Create a list of UpdateRequestParameter objects
                List<UpdateRequestParameter> updateRequestParameters = new List<UpdateRequestParameter>();

                updateRequestParameters.Add(new UpdateRequestParameter("ProvisionRequestAD", UpdateMode.Modify, ProvisionRequestAD));

                UpdateUser.UpdateParameters = updateRequestParameters.ToArray<UpdateRequestParameter>();

            }
            else
            {
                UpdateUser.ActorId = new Guid(FIMAdminGuid);
                UpdateUser.ResourceId = ReadCurrentRequestActivity.CurrentRequest.Target.GetGuid();
            }
        }



    }
}
