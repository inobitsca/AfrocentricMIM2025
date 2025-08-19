using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Workflow.ComponentModel;
using FIM.CustomWorkflowActivitiesLibrary.Activities.WebUIs.RequestLoggingActivity;
using Microsoft.IdentityManagement.WebUI.Controls;
using Microsoft.ResourceManagement.Workflow.Activities;

namespace FIM.CustomWorkflowActivitiesLibrary.WebUIs
{
    internal class RequestLoggingActivitySettingsPart : ActivitySettingsPart
    {
        /// <summary>
        /// Called when a user clicks the Save button in the Workflow Designer. 
        /// Returns an instance of the RequestLoggingActivity class that 
        /// has its properties set to the values entered into the text box controls
        /// used in the UI of the activity. 
        /// </summary>
        public override Activity GenerateActivityOnWorkflow(SequentialWorkflow workflow)
        {
            if (!this.ValidateInputs())
            {
                return null;
            }
            RequestLoggingActivity LoggingActivity = new RequestLoggingActivity();
            LoggingActivity.LogFilePath = this.GetText("txtLogFilePath");
            LoggingActivity.LogFileName = this.GetText("txtLogFileName");
            return LoggingActivity;
        }


        /// <summary>
        /// Called by FIM when the UI for the activity must be reloaded.
        /// It passes us an instance of our workflow activity so that we can
        /// extract the values of the properties to display in the UI.
        /// </summary>
        public override void LoadActivitySettings(Activity activity)
        {
            RequestLoggingActivity LoggingActivity = activity as RequestLoggingActivity;
            if (null != LoggingActivity)
            {
                this.SetText("txtLogFilePath", LoggingActivity.LogFilePath);
                this.SetText("txtLogFileName", LoggingActivity.LogFileName);
            }
        }


        /// <summary>
        /// Saves the activity settings.
        /// </summary>
        public override ActivitySettingsPartData PersistSettings()
        {
            ActivitySettingsPartData data = new ActivitySettingsPartData();
            data["LogFilePath"] = this.GetText("txtLogFilePath");
            data["LogFileName"] = this.GetText("txtLogFileName");
            return data;
        }


        /// <summary>
        ///  Restores the activity settings in the UI
        /// </summary>
        public override void RestoreSettings(ActivitySettingsPartData data)
        {
            if (null != data)
            {
                this.SetText("txtLogFilePath", (string)data["LogFilePath"]);
                this.SetText("txtLogFileName", (string)data["LogFileName"]);
            }
        }


        /// <summary>
        ///  Switches the activity between read only and read/write mode
        /// </summary>
        public override void SwitchMode(ActivitySettingsPartMode mode)
        {
            bool readOnly = (mode == ActivitySettingsPartMode.View);
            this.SetTextBoxReadOnlyOption("txtLogFilePath", readOnly);
            this.SetTextBoxReadOnlyOption("txtLogFileName", readOnly);
        }


        /// <summary>
        ///  Returns the activity name.
        /// </summary>
        public override string Title
        {
            get { return "Request Logging Activity"; }
        }


        /// <summary>
        ///  In general, this method should be used to validate information entered
        ///  by the user when the activity is added to a workflow in the Workflow
        ///  Designer.
        ///  We could add code to verify that the log file path already exists on
        ///  the server that is hosting the FIM Portal and check that the activity
        ///  has permission to write to that location. However, the code
        ///  would only check if the log file path exists when the
        ///  activity is added to a workflow in the workflow designer. This class
        ///  will not be used when the activity is actually run.
        ///  For this activity we will just return true.
        /// </summary>
        public override bool ValidateInputs()
        {
            return true;
        }


        /// <summary>
        ///  Creates a Table that contains the controls used by the activity UI
        ///  in the Workflow Designer of the FIM portal. Adds that Table to the
        ///  collection of Controls that defines each activity that can be selected
        ///  in the Workflow Designer of the FIM Portal. Calls the base class of 
        ///  ActivitySettingsPart to render the controls in the UI.
        /// </summary>
        protected override void CreateChildControls()
        {
            Table controlLayoutTable;
            controlLayoutTable = new Table();

            //Width is set to 100% of the control size
            controlLayoutTable.Width = Unit.Percentage(100.0);
            controlLayoutTable.BorderWidth = 0;
            controlLayoutTable.CellPadding = 2;
            //Add a TableRow for each textbox in the UI 
            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Log File Path:", "txtLogFilePath", 400, 100, false, "Enter the log file Path."));
            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Log File Name:", "txtLogFileName", 400, 100, false, "Enter the log file Name."));
            this.Controls.Add(controlLayoutTable);

            base.CreateChildControls();
        }

        #region Utility Functions
        //Create a TableRow that contains a label and a textbox.
        private TableRow AddTableRowTextBox(String labelText, String controlID, int width, int
                                             maxLength, Boolean multiLine, String defaultValue)
        {
            TableRow row = new TableRow();
            TableCell labelCell = new TableCell();
            TableCell controlCell = new TableCell();
            Label oLabel = new Label();
            TextBox oText = new TextBox();

            oLabel.Text = labelText;
            oLabel.CssClass = base.LabelCssClass;
            labelCell.Controls.Add(oLabel);
            oText.ID = controlID;
            oText.CssClass = base.TextBoxCssClass;
            oText.Text = defaultValue;
            oText.MaxLength = maxLength;
            oText.Width = width;
            if (multiLine)
            {
                oText.TextMode = TextBoxMode.MultiLine;
                oText.Rows = System.Math.Min(6, (maxLength + 60) / 60);
                oText.Wrap = true;
            }
            controlCell.Controls.Add(oText);
            row.Cells.Add(labelCell);
            row.Cells.Add(controlCell);
            return row;
        }

        string GetText(string textBoxID)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            return textBox.Text ?? String.Empty;
        }
        void SetText(string textBoxID, string text)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            if (textBox != null)
                textBox.Text = text;
            else
                textBox.Text = "";
        }

        //Set the text box to read mode or read/write mode
        void SetTextBoxReadOnlyOption(string textBoxID, bool readOnly)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            textBox.ReadOnly = readOnly;
        }
        #endregion


    }
}

