using MIMModels;

namespace MVExtension
{
    public class MVExtensionObject : IMVSynchronization
    {
        MIMConfig mimConfig = null;
        void IMVSynchronization.Initialize()
        {
            try
            {
                mimConfig = MIMModels.MIMConfig.LoadMIMConfigFromFile("MIMConfig2025.json");
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText($"Error-MIMConfig2022.json not found-{DateTime.Now.ToString("ddMMMyyyyHHmmss")}.txt", ex.ToString());
                throw ex;
            }

        }

        void IMVSynchronization.Provision(MVEntry mventry)
        {
            // MIM still runs each provisioning extension against all metaverse objects so if it is not the person object exit the sub
            switch (mventry.ObjectType)
            {
                case "person":
                    {


                        // 'Clean up the Portal - Delete Users without a DisplayName
                        if (!mventry["displayName"].IsPresent)
                            mventry.ConnectedMAs["MIM"].Connectors.DeprovisionAll();

                        // 'Clean up the Portal - Delete Terminated Users
                        if (mventry["employeeStatus"].IsPresent)
                        {
                            if (mventry["employeeStatus"].Value.ToLower() == "terminated")
                                mventry.ConnectedMAs["MIM"].Connectors.DeprovisionAll();
                        }




                        string UtilityMA = "Utility DB MA";

                        // We need a Personal present in order to continue.
                        // Also check if objectSID is present. Is it is not present there should not be a AD account and can continue to provision.
                        if ((mventry["employeeStatus"].IsPresent))
                        {

                            // Instantiate an XmlDocument object.
                            //System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
                            // Load the configuration.xml
                            //xmldoc.Load(@"C:\Program Files\Microsoft Forefront Identity Manager\2010\Synchronization Service\Extensions\Configuration.xml");
                            // string OU = null;

                            // Get the BASEDN for AD from config file
                            var BASEDN = mimConfig.BaseDN;  //xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/BASEDN");
                            var AD_maName = mimConfig.ADMAName; //xmldoc.SelectSingleNode("//maType[.='AD']/parent::node()/maName");

                            ConnectedMA AD_MA = null/* TODO Change to default(_) if this is not a reference type */;
                            CSEntry csentry = null/* TODO Change to default(_) if this is not a reference type */;
                            ReferenceValue dn = null/* TODO Change to default(_) if this is not a reference type */;
                            string ProvisionADUser = null;



                            // Only Provsion Active Account in SAP
                            if (mventry["employeeStatus"].Value == "Active" & (mventry["objectSID"].IsPresent == false) & mventry["sn"].IsPresent & mventry["nickName"].IsPresent & mventry["displayName"].IsPresent)
                            {


                                // Should the user be provisioned?
                                // Check the provisionWorkflowOverride value. If true go and provision the user without approval
                                // Else wait for approval from HR to provision user account in AD
                                if (mimConfig.provisionWorkflowOverride)
                                    ProvisionADUser = "Yes";
                                else if (mventry["ProvisionRequestAD"].IsPresent)
                                {
                                    if (mventry["ProvisionRequestAD"].Value == "Approved")
                                        ProvisionADUser = "Yes";
                                    else
                                        ProvisionADUser = "No";
                                }
                                else
                                    ProvisionADUser = "No";

                                // Ready for user to be provisioned
                                if (ProvisionADUser == "Yes")
                                {

                                    var cityOU = mimConfig.GetValue(MIMConfig.RuleTypes.OURules, mventry["city"].Value.ToLower());
                                    var homeMDB = mimConfig.GetValue(MIMConfig.RuleTypes.HomeMDB, mventry["city"].Value.ToLower());
                                    var msExchHomeServerName = mimConfig.GetValue(MIMConfig.RuleTypes.EmailServer, mventry["city"].Value.ToLower());
                                    var domain = mimConfig.GetValue(MIMConfig.RuleTypes.CountryDomains, mventry["country"].Value.ToLower());

                                    var OU = cityOU + "," + BASEDN;

                                    // Need to create a sAMAccountName to populate the rdn value
                                    // Construct the logon value NickName + lastname
                                    string sn_format = mventry["sn"].Value.ToLower().Replace(" ", "");
                                    //string sn = mventry["sn"].Value;

                                    //string nickName = mventry["nickName"].Value;
                                    string nickName_format = mventry["nickName"].Value.ToLower().Replace(" ", "");


                                    // Go and generate a unique accountName
                                    string SAMaccountName = GetUniqueSAMAccountName(nickName_format, sn_format, "accountName");

                                    string UPN = GetUniqueUPNAccountName($"{nickName_format}.{sn_format}", "userPrincipalName", $"@{domain}");



                                    // 
                                    // 
                                    // Build the new CN value and then check if this is unique before commiting the move
                                    string CNName = mventry["nickName"].Value.Replace(" ", "") + " " + mventry["sn"].Value.Replace(" ", "");
                                    string DNName;
                                    // DNName = "CN=" + CNName + "," + OU
                                    // Check if the DN is unique
                                    DNName = GetUniqueDNName(CNName, OU, "dn");
                                    // Format rdn into the following "CN=John Doe"
                                    string[] DNNameSplit = DNName.Split(',');
                                    var rdn = DNNameSplit[0];


                                    // Read the name of the AD Management Agent from the XML file
                                    AD_MA = mventry.ConnectedMAs[AD_maName];
                                    // Now we have all the info to construct the full dn value
                                    dn = AD_MA.EscapeDNComponent(rdn).Concat(OU);


                                    // Now we are ready to provision the user if it does not exist in AD
                                    // Check that the user is not in AD and also needs to be a active record before provisioning it.
                                    if ((AD_MA.Connectors.Count == 0))
                                    {


                                        // Check if this is a normal user or a mail enabled user account
                                        // Can use this later on to do selective mailbox provisioning
                                        bool provisionMailbox = true;
                                        switch (true)
                                        {
                                            case object _ when provisionMailbox // Case True then provision the mailbox user
                                           :
                                                {
                                                    try
                                                    {
                                                        string MailboxNickName;
                                                        MailboxNickName = SAMaccountName;
                                                        csentry = ExchangeUtils.CreateMailbox(AD_MA, dn, MailboxNickName, homeMDB);
                                                        // Add msExchHomeServerName value
                                                        csentry["msExchHomeServerName"].Value = msExchHomeServerName;
                                                    }
                                                    catch (ObjectAlreadyExistsException)
                                                    {
                                                    }
                                                    // Ignore if the object already exists; join rules will join the existing object
                                                    catch (NoSuchAttributeException)
                                                    {
                                                    }
                                                    // Ignor if the attribute on the mventry object is not availible at this time
                                                    catch (Exception)
                                                    {
                                                        // Log exception messages to the event log and throw them to the ILM Identity Manager
                                                        // Logging.Log("Caught exception " & ex.Message, True, 1)
                                                        // EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                                                        throw;
                                                    }

                                                    break;
                                                }

                                            default:
                                                {
                                                    // Add the new user object to the connector space.
                                                    csentry = AD_MA.Connectors.StartNewConnector("user");
                                                    csentry.DN = dn;
                                                    break;
                                                }
                                        }



                                        // Set the initial password
                                        string NewPass = null;
                                        Random RandomPass = new Random();
                                        int RandomPassNumber;
                                        // Generate a random number between 100000000 and 999999999
                                        RandomPassNumber = RandomPass.Next(100000000, 999999999);
                                        NewPass = "P@ss" + RandomPassNumber.ToString() + "w0rd";
                                        csentry["unicodepwd"].Value = NewPass;

                                        // Set the account status as a normal user account for the provisioning of the user.
                                        int ADS_UF_NORMAL_ACCOUNT = 0x200;
                                        csentry["useraccountcontrol"].IntegerValue = ADS_UF_NORMAL_ACCOUNT;

                                        // Set the account password to change at next logon
                                        // csentry["pwdLastSet").IntegerValue = 0

                                        // OK to set next three attributes if they will never change
                                        csentry["sAMAccountName"].Value = SAMaccountName;
                                        csentry["userPrincipalName"].Value = UPN;
                                        // Set the displayName
                                        csentry["displayName"].Value = mventry["displayName"].Value;
                                        csentry["givenName"].Value = mventry["nickName"].Value;
                                        csentry["sn"].Value = mventry["sn"].Value;

                                        // Commit all the values to the CS
                                        try
                                        {
                                            csentry.CommitNewConnector();
                                        }
                                        catch (ObjectAlreadyExistsException)
                                        {
                                        }
                                        // Ignore if the object already exists; join rules will join the existing object
                                        catch (NoSuchAttributeException)
                                        {
                                        }
                                        // Ignor if the attribute on the mventry object is not availible at this time
                                        catch (Exception)
                                        {
                                            // Log exception messages to the event log and throw them to the ILM Identity Manager
                                            // Logging.Log("Caught exception " & ex.Message, True, 1)
                                            // EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                                            throw;
                                        }
                                    } // If (AD_MA.Connectors.Count = 0)
                                } // if ProvisionADUser = "Yes"
                            }
                            else if ((mventry["employeeStatus"].Value == "Terminated") & (mventry["objectSID"].IsPresent))
                            {
                                // Get the current CN value for the user
                                if (mventry["cn"].IsPresent)
                                {

                                    // Build the OU value
                                    var OU = mimConfig.GetValue(MIMConfig.RuleTypes.OURules, "Disabled") + "," + BASEDN;

                                    // 
                                    // 
                                    // Build the new DN value and then check if this is unique before commiting the move
                                    string CNName = mventry["cn"].Value;
                                    string DNName = "CN=" + CNName + "," + OU;
                                    // Check if the DN is unique
                                    // DNName = GetUniqueDNName(DNName, "dn")
                                    // DNName = GetUniqueDNName(CNName, OU, "dn")
                                    // Format rdn into the following "CN=John Doe"
                                    string[] DNNameSplit = DNName.Split(',');
                                    var rdn = DNNameSplit[0];


                                    // Read the name of the AD Management Agent from the XML file
                                    AD_MA = mventry.ConnectedMAs[AD_maName];
                                    // Now we have all the info to construct the full dn value
                                    dn = AD_MA.EscapeDNComponent(rdn).Concat(OU);



                                    if (AD_MA.Connectors.Count == 1)
                                    {
                                        csentry = AD_MA.Connectors.ByIndex[0];
                                        csentry.DN = dn;
                                    }
                                }
                            }


                            // 2022 - new Transitioning Logic
                            var ActiveContractor = false;
                            var ActiveEmployee = false;
                            DateTime LatestContractorStartDate = DateTime.Parse("1 Jan 1900");
                            DateTime LatestEmployeeStartDate = DateTime.Parse("1 Jan 1900");

                            var DoomedContractors = new List<CSEntry>();

                            var AllContractorConnectors = mventry.ConnectedMAs[mimConfig.HRContractorsMAName].Connectors;
                            foreach (var item in AllContractorConnectors)
                            {
                                if (item["EmplStatus"].BooleanValue == true)
                                {
                                    ActiveContractor = true;
                                    if (item["EmployeeStartDate"].IsPresent)
                                    {
                                        if (DateTime.TryParse(item["EmployeeStartDate"].StringValue, out DateTime csSD))
                                        {
                                            if (csSD > LatestContractorStartDate)
                                            {
                                                LatestContractorStartDate = csSD;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new AttributeNotPresentException("EmployeeStartDate");
                                    }
                                }
                                else
                                {
                                    // If Contractor is not active, deprovision it.
                                    // item.Deprovision();  // Cannot deprovision here, since we are looping through the collection
                                    if (AllContractorConnectors.Count > 1)
                                    {
                                        DoomedContractors.Add(item);
                                    }
                                }
                            }

                            foreach (var item in DoomedContractors)
                            {
                                item.Deprovision();
                            }

                            var DoomedEmployees = new List<CSEntry>();
                            var AllEmployeeConnectors = mventry.ConnectedMAs[mimConfig.HREmployeesMAName].Connectors;
                            var NumberOfEmployeeObjects = AllEmployeeConnectors.Count;

                            foreach (var item in AllEmployeeConnectors)
                            {
                                if (item["EmplStatus"].BooleanValue == true)
                                {
                                    ActiveEmployee = true;
                                    if (item["EmployeeStartDate"].IsPresent)
                                    {
                                        if (DateTime.TryParse(item["EmployeeStartDate"].StringValue, out DateTime csSD))
                                        {
                                            if (csSD > LatestEmployeeStartDate)
                                            {
                                                LatestEmployeeStartDate = csSD;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new AttributeNotPresentException("EmployeeStartDate");
                                    }
                                }
                                else
                                {
                                    // If Employees is not active, deprovision it.
                                    //item.Deprovision(); // Cannot do this here, since we are looping through the collection.
                                    if (AllEmployeeConnectors.Count > 1)
                                    {
                                        DoomedEmployees.Add(item);
                                    }
                                }
                            }

                            foreach (var item in DoomedEmployees)
                            {
                                item.Deprovision();
                            }

                            if (ActiveEmployee == true && LatestEmployeeStartDate == DateTime.Parse("1 Jan 1900")) // There is an Active Employee with no end date
                            {
                                //mventry["employeeStatus"].Value = "Active";
                                AllContractorConnectors.DeprovisionAll();
                                //foreach (var item in AllContractorConnectors)
                                //{
                                //    item.Deprovision(); // Disconnect the Contractor record(s)
                                //}
                            }

                            if (ActiveEmployee == false && ActiveContractor == true)
                            {
                                //mventry["employeeStatus"].Value = "Active";
                                AllEmployeeConnectors.DeprovisionAll();
                                //foreach (var item in AllEmployeeConnectors)
                                //{
                                //    item.Deprovision(); // Disconnect the Employee record(s)
                                //}
                            }

                            if (ActiveEmployee == true && ActiveContractor == true)
                            {
                                //mventry["employeeStatus"].Value = "Active";
                                if (LatestContractorStartDate > LatestEmployeeStartDate)
                                {
                                    AllEmployeeConnectors.DeprovisionAll();
                                    //foreach (var item in AllEmployeeConnectors)
                                    //{
                                    //    item.Deprovision();
                                    //}
                                }
                                else
                                {
                                    AllContractorConnectors.DeprovisionAll();
                                    //foreach (var item in AllContractorConnectors)
                                    //{
                                    //    item.Deprovision();
                                    //}
                                }
                            }
                            else
                            {
                                //mventry["employeeStatus"].Value = "Terminated";
                            }
                        }
                        // End of new Transitioning Logic

                        bool DoesExist = false;
                        bool ShouldExist = false;

                        if (mventry.ConnectedMAs[UtilityMA].Connectors.Count > 0)
                            DoesExist = true;

                        if (mventry["employeeNumber"].IsPresent)
                            ShouldExist = true;

                        if (ShouldExist & !DoesExist)
                        {
                            CSEntry csentry;
                            csentry = mventry.ConnectedMAs[UtilityMA].Connectors.StartNewConnector("position");
                            csentry["DN"].Value = mventry["employeeNumber"].Value;

                            try
                            {
                                csentry.CommitNewConnector();
                            }
                            catch (ObjectAlreadyExistsException)
                            {
                            }
                            // Ignore if the object already exists; join rules will join the existing object
                            catch (NoSuchAttributeException)
                            {
                            }
                            // Ignor if the attribute on the mventry object is not availible at this time
                            catch (Exception)
                            {
                                // Log exception messages to the event log and throw them to the ILM Identity Manager
                                // Logging.Log("Caught exception " & ex.Message, True, 1)
                                // EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                                throw;
                            }
                        }
                        else if (DoesExist & !ShouldExist)
                        {
                            CSEntry csentry;
                            csentry = mventry.ConnectedMAs[UtilityMA].Connectors.ByIndex[0];
                            csentry.Deprovision();
                        }

                        break;
                    }

                case "group":
                    {
                        break;
                    }

                default:
                    {
                        break;
                    }
            }


        }

        bool IMVSynchronization.ShouldDeleteFromMV(CSEntry csentry, MVEntry mventry)
        {
            throw new NotImplementedException();
        }

        void IMVSynchronization.Terminate()
        {
        }

        private string GetUniqueDNName(string CNName_to_Check, string OU_to_Check, string attribute_to_check)
        {
            string Result = "";
            // "CN=" + CNName + "," + OU
            // This function checks the MV for a unique DN value
            // If is is not unique it will add a number at the end starting at 2.

            MVEntry[] findResultList;
            string checkedName = "CN=" + CNName_to_Check + "," + OU_to_Check;
            Result = null;

            // Create a unique naming attribute by adding a number to 
            // the existing strAcctName value. 
            int nameSuffix;
            for (nameSuffix = 2; nameSuffix <= 20; nameSuffix++)
            {
                // Check if the passed strAcctName value exists in the metaverse by 
                // using the Utils.FindMVEntries method.
                findResultList = InoUtils.FindMVEntries(attribute_to_check, checkedName, 1);

                // If the value does not exist in the metaverse, use the passed value 
                // as the metaverse value. 

                if (findResultList.Length == 0)
                {
                    Result = checkedName;
                    break;
                }

                // If the passed value already exists, concatenate the counter number 
                // to the passed value and verify this new value exists. Repeat 
                // this step until a unique value is created. 
                // checkedName = Name_to_Check + " " + nameSuffix.ToString
                CNName_to_Check = CNName_to_Check + nameSuffix.ToString();
                checkedName = "CN=" + CNName_to_Check + "," + OU_to_Check;
            }
            return Result;
        }

        private string GetUniqueSAMAccountName(string nickName_format, string sn_format, string attribute_to_check)
        {
            string Result = "";
            // This function will check if the accountname is unique in the MV
            // If is is not unique it will add the next letter of the surname

            MVEntry[] findResultList;
            // Create a accountName to test
            string checkedName = nickName_format + sn_format.Substring(0, 1);
            Result = null;

            int nameSuffix;
            for (nameSuffix = 2; nameSuffix <= 20; nameSuffix++)
            {
                // Check if the passed checkedName value exists in the metaverse by 
                // using the Utils.FindMVEntries method. 
                findResultList = InoUtils.FindMVEntries(attribute_to_check, checkedName, 1);

                // If the value does not exist in the metaverse, use the passed value 
                // as the metaverse value. 
                if (findResultList.Length == 0)
                {
                    Result = checkedName;
                    break;
                }

                // Continue to add the next value of the surname until a unique name if found. 
                checkedName = nickName_format + sn_format.Substring(0, nameSuffix);
            }
            return Result;
        }

        private string GetUniqueUPNAccountName(string NameToCheck, string attribute_to_check, string domainSuffix)
        {
            string Result = "";
            // This function will check if the accountname is unique in the MV
            // If is is not unique it will add the next letter of the surname

            MVEntry[] findResultList;
            // Create a accountName to test
            string checkedName = NameToCheck + domainSuffix;
            Result = null;

            int nameSuffix;
            for (nameSuffix = 2; nameSuffix <= 20; nameSuffix++)
            {
                // Check if the passed checkedName value exists in the metaverse by 
                // using the Utils.FindMVEntries method. 
                findResultList = InoUtils.FindMVEntries(attribute_to_check, checkedName, 1);

                // If the value does not exist in the metaverse, use the passed value 
                // as the metaverse value. 
                if (findResultList.Length == 0)
                {
                    Result = checkedName;
                    break;
                }

                // Continue to add the next value of the surname until a unique name if found. 
                checkedName = NameToCheck + nameSuffix.ToString() + domainSuffix;
            }
            return Result;
        }


    }

    public class IMVSynchronization
    {
    }
}
