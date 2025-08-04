using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMModels
{
    public class MIMConfig
    {

        public static MIMConfig LoadMIMConfigFromFile(string Filename)
        {

            var FileContent = System.IO.File.ReadAllText(Filename);
            var LoadedMIMConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<MIMConfig>(FileContent);
            return LoadedMIMConfig;
        }

        public void SaveConfig(string Filename)
        {
            var serializedConfig = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(Filename, serializedConfig);
        }

        public string BaseDN { get; set; }
        public bool provisionWorkflowOverride { get; set; }
        public string ADMAName { get; set; }

        public string HREmployeesMAName { get; set; }
        public string HRContractorsMAName { get; set; }
        public string GalSyncTelkomMA { get; set; }
        public string GalSyncBCXMA { get; set; }

        public List<MIMRule> CountryDomains { get; set; } = new List<MIMRule>();

        public List<MIMRule> OURules { get; set; } = new List<MIMRule>();

        public List<MIMRule> HomeMDBRules { get; set; } = new List<MIMRule>();
        public List<MIMRule> EmailServers { get; set; } = new List<MIMRule>();
        public List<RuleNamedValue> NamedValues { get; set; } = new List<RuleNamedValue>();

        public string GetValue(RuleTypes RuleType, string Target)
        {
            switch (RuleType)
            {
                case RuleTypes.HomeMDB:
                    if (HomeMDBRules.Any(q => q.Target.ToLower() == Target.ToLower()))
                    {
                        var FoundValue = HomeMDBRules.First(q => q.Target.ToLower() == Target.ToLower());
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                    else
                    {
                        var FoundValue = HomeMDBRules.First(q => q.IsDefault);
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                case RuleTypes.EmailServer:
                    if (EmailServers.Any(q => q.Target.ToLower() == Target.ToLower()))
                    {
                        var FoundValue = EmailServers.First(q => q.Target.ToLower() == Target.ToLower());
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                    else
                    {
                        var FoundValue = EmailServers.First(q => q.IsDefault);
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                case RuleTypes.OURules:
                    if (OURules.Any(q => q.Target.ToLower() == Target.ToLower()))
                    {
                        var FoundValue = OURules.First(q => q.Target.ToLower() == Target.ToLower());
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                    else
                    {
                        var FoundValue = OURules.First(q => q.IsDefault);
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                case RuleTypes.CountryDomains:
                    if (CountryDomains.Any(q => q.Target.ToLower() == Target.ToLower()))
                    {
                        var FoundValue = CountryDomains.First(q => q.Target.ToLower() == Target.ToLower());
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                    else
                    {
                        var FoundValue = CountryDomains.First(q => q.IsDefault);
                        if (string.IsNullOrEmpty(FoundValue.Value))
                        {
                            if (NamedValues.Any(q => q.NamedValueID == FoundValue.NamedValueID))
                            {
                                return NamedValues.First(q => q.NamedValueID == FoundValue.NamedValueID).NamedValue;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            return FoundValue.Value;
                        }
                    }
                default:
                    throw new NotSupportedException("You must specify a valid RuleType");
            }
        }

            public enum RuleTypes
        {
            HomeMDB,
            EmailServer,
            OURules,
            CountryDomains
        }
    }

    public class RuleNamedValue
    {
        public int NamedValueID { get; set; }
        public string NamedValue { get; set; }
    }


    public class MIMRule
    {
        public string Target { get; set; }
        public string Value { get; set; }

        public int? NamedValueID { get; set; }
        public bool IsDefault { get; set; } = false;
    }


}
