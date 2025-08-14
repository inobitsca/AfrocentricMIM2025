using Microsoft.MetadirectoryServices;
using MIMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRExtension
{
    public class MAExtensionObject : IMASynchronization
    {
        MIMConfig mimConfig = null;

        void IMASynchronization.MapAttributesForImport(string FlowRuleName, CSEntry csentry, MVEntry mventry)
        {
            // Set the Format for the Date/Time attributes.
            // Dim FormatStringA As String = "yyyy/MM/dd"
            string FormatStringB = "yyyy-MM-ddT00:00:00.000";

            switch (FlowRuleName)
            {

                case "cd.person:MIMIDhash":
                    {
                        if (csentry["IDnr"].IsPresent)
                        {
                            string UnhashedIDNumber = csentry["IDnr"].Value;
                            string HashedIDNumber = InoUtils.ComputeSha256Hash(UnhashedIDNumber);
                            mventry["MIMIDhash"].Value = HashedIDNumber;
                        }
                        break;
                    }

                case "cd.person:TransitionEmployeeStatus":
                    {
                        if (csentry["STATUS"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                        
                            var ActiveContractor = false;
                            var ActiveEmployee = false;

                            var AllContractorConnectors = mventry.ConnectedMAs[mimConfig.HRContractorsMAName].Connectors;
                            foreach (var item in AllContractorConnectors)
                            {
                                if (item["STATUS"].BooleanValue  == true)
                                {
                                    ActiveContractor = true;
                                }
                            }

                            var AllEmployeeConnectors = mventry.ConnectedMAs[mimConfig.HREmployeesMAName].Connectors;
                            foreach (var item in AllEmployeeConnectors)
                            {
                                if (item["STATUS"].BooleanValue == true)
                                {
                                    ActiveEmployee = true;
                                }
                            }

                            if (ActiveEmployee == true || ActiveContractor == true)
                            {
                                mventry["employeeStatus"].Value = "Active";
                            }
                            else
                            {
                                mventry["employeeStatus"].Value = "Terminated";
                            }
                        }

                        break;
                    }

                case "cd.person:EmployeeNumber->mv.person:hrSourceMA":
                    {
                        if (csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                            {
                                string x = csentry.MA.Name.ToString().ToLower();
                                if (x.Contains("contractor") == true)
                                    mventry["hrSourceMA"].Value = "contractor";
                                else if (x.Contains("staff") == true)
                                    mventry["hrSourceMA"].Value = "staff";
                            }
                        }

                        break;
                    }

                case "cd.person:EmployeeStartDate->mv.person:employeeStartDate":
                    {
                        if (csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                            {
                                if (csentry["EmployeeStartDate"].IsPresent)
                                {
                                    string CSDate;
                                    CSDate = csentry["EmployeeStartDate"].Value; // Read the date from the CS
                                    DateTime ParsedDate;
                                    if (DateTime.TryParse(CSDate, out ParsedDate))
                                    {
                                        //DateTime ParsedDate = DateTime.Parse(CSDate); // This interprets the string as a date
                                        string NewlyFormattedDate = ParsedDate.ToString(FormatStringB); // This formats the date into the desired format
                                        mventry["EmployeeStartDate"].Value = NewlyFormattedDate; // This puts the newly formatted date into the MV
                                    }
                                    else
                                        mventry["EmployeeStartDate"].Delete();// Not a valid date, or an Empty String, lets clear the attribute in the Metaverse
                                }
                                else
                                    mventry["EmployeeStartDate"].Delete();// No Date present, lets clear the attribute in the Metaverse
                            }
                        }

                        break;
                    }

                case "cd.person:EmployeeEndDate->mv.person:employeeEndDate":
                    {
                        // Format the EmployeeEndDate in the MV to "yyyy/MM/ddTHH:mm:ss.000".
                        if (csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                            {
                                // End date of "9999-12-31 00:00:00" is the default value for no end date
                                if (csentry["EmployeeEndDate"].IsPresent)
                                {
                                    if (csentry["EmployeeEndDate"].Value == "9999-12-31 00:00:00")
                                        mventry["employeeEndDate"].Delete();
                                    else
                                    {
                                        string CSDate;
                                        CSDate = csentry["EmployeeEndDate"].Value; // Read the date from the CS
                                        DateTime ParsedDate;
                                        if (DateTime.TryParse(CSDate, out ParsedDate))
                                        {
                                            string NewlyFormattedDate = ParsedDate.ToString(FormatStringB); // This formats the date into the desired format
                                            mventry["employeeEndDate"].Value = NewlyFormattedDate; // This puts the newly formatted date into the MV
                                        }
                                        else
                                            mventry["employeeEndDate"].Delete();// Not a valid date, or an Empty String, lets clear the attribute in the Metaverse
                                    }
                                }
                                else
                                    mventry["employeeEndDate"].Delete();// No TerminationDate present, lets clear the attribute in the Metaverse
                            }
                        }

                        break;
                    }

                case "cd.person:FirstName->mv.person:fullName":
                    {
                        if (csentry["FirstName"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["fullname"].Value = csentry["FirstName"].Value;
                        }

                        break;
                    }

                case "cd.person:PhoneFax->mv.person:facsimileTelephoneNumber":
                    {
                        if (csentry["PhoneFax"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["facsimileTelephoneNumber"].Value = csentry["PhoneFax"].Value;
                        }

                        break;
                    }

                case "cd.person:GeoLocation->mv.person:location":
                    {
                        if (csentry["Geolocation"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["location"].Value = csentry["GeoLocation"].Value;
                        }

                        break;
                    }

                case "cd.person:AddrRegion->mv.person:region":
                    {
                        if (csentry["AddrRegion"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["region"].Value = csentry["AddrRegion"].Value;
                        }

                        break;
                    }

                case "cd.person:PostalCode->mv.person:postalCode":
                    {
                        if (csentry["PostalCode"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["postalCode"].Value = csentry["PostalCode"].Value;
                        }

                        break;
                    }

                case "cd.person:IDNr->mv.person:IDNumber":
                    {
                        if (csentry["IDNr"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["IDNumber"].Value = csentry["IDNr"].Value;
                        }

                        break;
                    }

                case "cd.person:OffAddSuburb->mv.person:suburb":
                    {
                        if (csentry["OffAddSuburb"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["suburb"].Value = csentry["OffAddSuburb"].Value;
                        }

                        break;
                    }

                case "cd.person:OffAddStrNbr->mv.person:streetNbr":
                    {
                        if (csentry["OffAddStrNbr"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["streetNbr"].Value = csentry["OffAddStrNbr"].Value;
                        }

                        break;
                    }

                case "cd.person:Manager->mv.person:managerName":
                    {
                        if (csentry["Manager"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["managerName"].Value = csentry["Manager"].Value;
                        }

                        break;
                    }

                case "cd.person:ManagerPernr->mv.person:superiorPosition":
                    {
                        if (csentry["ManagerPernr"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["superiorPosition"].Value = csentry["ManagerPernr"].Value.ToString();
                        }

                        break;
                    }

                case "cd.person:<dn>->mv.person:ProvisionRequestAD":
                    {
                        if ((mventry["objectSid"].IsPresent == false) & (mventry["ProvisionRequestAD"].IsPresent == false) & mventry["employeeStatus"].IsPresent & mventry["Known_As"].IsPresent & mventry["sn"].IsPresent & mventry["displayName"].IsPresent)
                        {
                            if (mventry["employeeStatus"].Value == "Active")
                                mventry["ProvisionRequestAD"].Value = "Not Approved";
                        }

                        break;
                    }

                case "cd.person:EmployeeNumber->mv.person:employeeNumber":
                    {
                        // possible problem for new entries if values are not present
                        // added the mv-employeeNumber is present check in first if to exclude new entries from older where Id numbers are not present
                        // note: if the STATUS value = true (Active) then we process else not
                        // parsedate results are: if parsedate is less than today then return value is -1
                        // if parsedate is equal to today then return value is 0
                        // if parsedate is greater than today then return value is 1

                        if (mventry["IDNumber"].IsPresent & mventry["employeeNumber"].IsPresent)
                        {
                            if (csentry["IDNr"].IsPresent & csentry["EmployeeNumber"].IsPresent & csentry["STATUS"].IsPresent & csentry["EmployeeType"].IsPresent & csentry["EmployeeStartDate"].IsPresent)
                            {
                                if (csentry["IDNr"].Value == mventry["IDNumber"].Value & csentry["EmployeeNumber"].Value != mventry["employeeNumber"].Value)
                                {
                                    if (Convert.ToBoolean(csentry["STATUS"].Value) == true)
                                    {
                                        string CSDate;
                                        CSDate = csentry["EmployeeStartDate"].Value; // Read the date from the CS
                                        DateTime ParsedDate;
                                        if (DateTime.TryParse(CSDate, out ParsedDate))
                                        {
                                            int dateCompResult = DateTime.Compare(ParsedDate, DateTime.Today);
                                            if (dateCompResult <= 0)
                                                mventry["employeeNumber"].Value = Convert.ToString(csentry["EmployeeNumber"].Value);
                                        }
                                    }
                                }
                            }
                        }
                        else if (!mventry["employeeNumber"].IsPresent & !mventry["IDNumber"].IsPresent)
                            mventry["employeeNumber"].Value = Convert.ToString(csentry["EmployeeNumber"].Value);
                        else if (mventry["employeeNumber"].IsPresent & !mventry["IDNumber"].IsPresent)
                            mventry["employeeNumber"].Value = Convert.ToString(csentry["EmployeeNumber"].Value);
                        break;
                    }

                case "cd.person:EmployeeNumber->mv.person:employeeNr":
                    {
                        if (csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["employeeNr"].Value = Convert.ToString(csentry["EmployeeNumber"].Value);
                        }

                        break;
                    }

                case "cd.person:Known_As->mv.person:Known_As":
                    {
                        if (csentry["Known_As"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["Known_As"].Value = csentry["Known_As"].Value.Trim();
                        }

                        break;
                    }

                case "cd.person:LastName->mv.person:sn":
                    {
                        if (csentry["LastName"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["sn"].Value = csentry["LastName"].Value.Trim();
                        }

                        break;
                    }

                case "cd.person:PhoneOffice->mv.person:telephoneNumber":
                    {
                        if (csentry["PhoneOffice"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["telephoneNumber"].Value = csentry["PhoneOffice"].Value;
                        }

                        break;
                    }

                case "cd.person:PhoneIntOffice->mv.person:officeIntPhone":
                    {
                        if (csentry["PhoneIntOffice"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["officeIntPhone"].Value = csentry["PhoneIntOffice"].Value;
                        }

                        break;
                    }

                case "cd.person:PhoneMobile->mv.person:mobile":
                    {
                        if (csentry["PhoneMobile"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["mobile"].Value = csentry["PhoneMobile"].Value;
                        }

                        break;
                    }



                case "cd.person:STATUS->mv.person:employeeStatus":
                    {
                        if (csentry["STATUS"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                            {
                                switch (csentry["STATUS"].Value)
                                {
                                    case "True":
                                        {
                                            mventry["employeeStatus"].Value = "Active";
                                            break;
                                        }

                                    case "False":
                                        {
                                            mventry["employeeStatus"].Value = "Terminated";
                                            break;
                                        }
                                }
                            }
                        }

                        break;
                    }

                case "cd.person:Country->mv.person:country":
                    {
                        if (csentry["Country"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["country"].Value = csentry["Country"].Value;
                        }

                        break;
                    }

                case "cd.person:OffAddStrName->mv.person:street":
                    {
                        if (csentry["OffAddStrName"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["street"].Value = csentry["OffAddStrName"].Value;
                        }

                        break;
                    }

                case "cd.person:City->mv.person:city":
                    {
                        if (csentry["City"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["city"].Value = csentry["City"].Value;
                        }

                        break;
                    }

                case "cd.person:EmployeeType->mv.person:employeeType":
                    {
                        if (csentry["EmployeeType"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["employeeType"].Value = csentry["EmployeeType"].Value;
                        } // Some new settings and not sure how to handle this one

                        break;
                    }

                case "cd.person:ManagerPernr->mv.person:manager":
                    {
                        break;
                    }

                case "cd.person:LastName,Known_As->mv.person:displayName":
                    {
                        if (csentry["LastName"].IsPresent & csentry["Known_As"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                            {
                                string sn = csentry["LastName"].Value.Trim();
                                string Known_As = csentry["Known_As"].Value.Trim();
                                mventry["displayName"].Value = Known_As + " " + sn + " - " + "BCX";
                            }
                        }

                        break;
                    }

                case "cd.person:JobTitle->mv.person:jobTitle":
                    {
                        if (csentry["JobTitle"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["jobTitle"].Value = csentry["JobTitle"].Value;
                        }

                        break;
                    }

                case "cd.person:Department->mv.person:department":
                    {
                        if (csentry["Department"].IsPresent & csentry["EmployeeNumber"].IsPresent & mventry["EmployeeNumber"].IsPresent)
                        {
                            if (csentry["EmployeeNumber"].Value == mventry["EmployeeNumber"].Value)
                                mventry["department"].Value = csentry["Department"].Value;
                        }

                        break;
                    }

                default:
                    {
                        // TODO: remove the following statement and add your default script here
                        throw new EntryPointNotImplementedException();
                        break;
                    }
            }
        }

        DeprovisionAction IMASynchronization.Deprovision(CSEntry csentry)
        {
            throw new NotImplementedException();
        }

        bool IMASynchronization.FilterForDisconnection(CSEntry csentry)
        {
            throw new NotImplementedException();
        }

        void IMASynchronization.Initialize()
        {
            try
            {
                mimConfig = MIMModels.MIMConfig.LoadMIMConfigFromFile("MIMConfig2022.json");
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText($"Error-MIMConfig2022.json not found-{DateTime.Now.ToString("ddMMMyyyyHHmmss")}.txt", ex.ToString());
                throw ex;
            }
        }

        void IMASynchronization.MapAttributesForExport(string FlowRuleName, MVEntry mventry, CSEntry csentry)
        {
            throw new NotImplementedException();
        }

        void IMASynchronization.MapAttributesForJoin(string FlowRuleName, CSEntry csentry, ref ValueCollection values)
        {
            throw new NotImplementedException();
        }

        bool IMASynchronization.ResolveJoinSearch(string joinCriteriaName, CSEntry csentry, MVEntry[] rgmventry, out int imventry, ref string MVObjectType)
        {
            throw new NotImplementedException();
        }

        bool IMASynchronization.ShouldProjectToMV(CSEntry csentry, out string MVObjectType)
        {
            throw new NotImplementedException();
        }

        void IMASynchronization.Terminate()
        {
        }
    }
}
