// Tagging logic for extensionAttribute1
public static class UserTagging
{
    public const string TagAttribute = "extensionAttribute1";
    public const string Suspended = "Suspended";
    public const string Reactivate = "Reactivate";

    // Returns true if the user is tagged as suspended
    public static bool IsSuspended(MVEntry mventry)
    {
        var tag = mventry[TagAttribute]?.Value;
        return string.Equals(tag, Suspended, StringComparison.OrdinalIgnoreCase);
    }

    // Returns true if the user is tagged for reactivation
    public static bool IsReactivation(MVEntry mventry)
    {
        var tag = mventry[TagAttribute]?.Value;
        return string.Equals(tag, Reactivate, StringComparison.OrdinalIgnoreCase);
    }
}
﻿using Microsoft.MetadirectoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADExtension
{
    public class MAExtensionObject : IMASynchronization
    {

        void IMASynchronization.MapAttributesForExport(string FlowRuleName, MVEntry mventry, CSEntry csentry)
        {
            switch (FlowRuleName)
            {
                case "cd.user:userAccountControl<-mv.person:employeeStatus,accountName":
                    {
                        // Tagging logic: override default enable/disable based on extensionAttribute1
                        long ADS_UF_NORMAL_ACCOUNT = 0x200;
                        long ADS_UF_ACCOUNTDISABLE = 0x2;
                        long ADS_UF_PASSWD_NOTREQD = 0x20;

                        long currentValue = csentry["userAccountControl"].IntegerValue;

                        if (UserTagging.IsSuspended(mventry))
                        {
                            // Always disable account if tagged as Suspended
                            csentry["userAccountControl"].IntegerValue = currentValue | ADS_UF_ACCOUNTDISABLE;
                        }
                        else if (UserTagging.IsReactivation(mventry))
                        {
                            // Enable account if tagged for Reactivation (override inactivity/disable)
                            csentry["userAccountControl"].IntegerValue = (currentValue | ADS_UF_NORMAL_ACCOUNT) & (~ADS_UF_ACCOUNTDISABLE);
                            // NOTE: Clearing the tag after 24 hours should be handled by a scheduled process or external workflow
                        }
                        else if (mventry["employeeStatus"].IsPresent & csentry["userAccountControl"].IsPresent)
                        {
                            string employeeStatus = mventry["employeeStatus"].Value;
                            switch ((employeeStatus))
                            {
                                case "Active":
                                    {
                                        csentry["userAccountControl"].IntegerValue = (currentValue | ADS_UF_NORMAL_ACCOUNT) & (~ADS_UF_ACCOUNTDISABLE);
                                        break;
                                    }

                                case "Terminated":
                                    {
                                        csentry["userAccountControl"].IntegerValue = currentValue | ADS_UF_ACCOUNTDISABLE;
                                        break;
                                    }
                            }
                        }

                        break;
                    }

                case "cd.user:accountExpires<-mv.person:accountName,employeeEndDate":
                    {
                        if (mventry["employeeEndDate"].IsPresent)
                        {
                            DateTime tempDate;
                            tempDate = Convert.ToDateTime(mventry["employeeEndDate"].Value);
                            // Add a day so that the account expires only expires at the EOD.
                            tempDate = tempDate.AddDays(1);
                            csentry["accountExpires"].Value = tempDate.ToFileTimeUtc().ToString();
                        }
                        else
                            // Set the accountExpires to never expire
                            csentry["accountExpires"].Value = "9223372036854775807";
                        break;
                    }

                case "cd.user:sn<-mv.person:sn":
                    {
                        csentry["sn"].Value = mventry["sn"].Value;
                        break;
                    }

                case "cd.user:extensionAttribute4<-mv.person:employeeType":
                    {
                        csentry["extensionAttribute4"].Value = mventry["employeeType"].Value;
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

        void IMASynchronization.MapAttributesForImport(string FlowRuleName, CSEntry csentry, MVEntry mventry)
        {

            switch (csentry.ObjectType)
            {
                case "user":
                    {
                        switch (FlowRuleName)
                        {
                            case "cd.user:employeeNumber->mv.person:employeeNumber":
                                {
                                    if (!mventry["employeeNumber"].IsPresent)
                                        mventry["employeeNumber"].Value = csentry["employeeNumber"].Value;
                                    break;
                                }

                            case "cd.user:displayName->mv.person:displayName":
                                {
                                    if (!mventry["displayName"].IsPresent)
                                        mventry["displayName"].Value = csentry["displayName"].Value;
                                    break;
                                }

                            case "cd.user:sn->mv.person:sn":
                                {
                                    if (!mventry["sn"].IsPresent)
                                        mventry["sn"].Value = csentry["sn"].Value;
                                    break;
                                }

                            case "cd.user:givenName->mv.person:nickName":
                                {
                                    if (!mventry["nickName"].IsPresent)
                                        mventry["nickName"].Value = csentry["givenName"].Value;
                                    break;
                                }

                            case "cd.user:department->mv.person:department":
                                {
                                    if (!mventry["department"].IsPresent)
                                        mventry["department"].Value = csentry["department"].Value;
                                    break;
                                }

                            case "cd.user:employeeNumber->mv.person:employeeNr":
                                {
                                    if (!mventry["employeeNr"].IsPresent)
                                    {
                                        if (csentry["employeeNumber"].IsPresent)
                                        {
                                            try
                                            {
                                                int tempEmpNr = Convert.ToInt32(csentry["employeeNumber"].Value);
                                                mventry["employeeNr"].Value = tempEmpNr.ToString();
                                            }
                                            catch
                                            {
                                            }
                                        }
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

                        break;
                    }

                case "contact":
                    {
                        switch (FlowRuleName)
                        {
                            case "cd.contact:->mv.person:employeeType":
                                {
                                    mventry["EmployeeType"].Value = "Contact";
                                    break;
                                }

                            case "cd.contact:telephoneNumber->mv.person:telephoneNumber":
                                {
                                    mventry["telephoneNumber"].Value = csentry["telephoneNumber"].Value;
                                    break;
                                }

                            case "cd.contact:department->mv.person:department":
                                {
                                    mventry["department"].Value = csentry["department"].Value;
                                    break;
                                }

                            case "cd.contact:displayName->mv.person:displayName":
                                {
                                    mventry["displayName"].Value = csentry["displayName"].Value;
                                    break;
                                }

                            case "cd.contact:mobile->mv.person:mobile":
                                {
                                    mventry["mobile"].Value = csentry["mobile"].Value;
                                    break;
                                }

                            case "cd.contact:sn->mv.person:sn":
                                {
                                    mventry["sn"].Value = csentry["sn"].Value;
                                    break;
                                }

                            default:
                                {
                                    throw new EntryPointNotImplementedException();
                                    break;
                                }
                        }

                        break;
                    }

                default:
                    {
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

