namespace TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mimConfig = new MIMModels.MIMConfig();

            mimConfig.BaseDN = "DC=africa,DC=enterprise,DC=root";
            mimConfig.provisionWorkflowOverride = true;
            mimConfig.ADMAName = "ad";
            mimConfig.HREmployeesMAName = "SAPTest";
            mimConfig.HRContractorsMAName = "SAPTestTemp";

            mimConfig.GalSyncBCXMA = "bcx";
            mimConfig.GalSyncTelkomMA = "telkom";

            mimConfig.CountryDomains.Add(new MIMModels.MIMRule() { Target = "South Africa",  Value = "bcx.co.za", IsDefault = true });
            mimConfig.CountryDomains.Add(new MIMModels.MIMRule() { Target = "Mozambique", Value = "bcx.co.mz", IsDefault = false });
            mimConfig.CountryDomains.Add(new MIMModels.MIMRule() { Target = "Zambia", Value = "bcx.co.zm", IsDefault = false });
            mimConfig.CountryDomains.Add(new MIMModels.MIMRule() { Target = "Botswana", Value = "bcx.bw", IsDefault = false });
            mimConfig.CountryDomains.Add(new MIMModels.MIMRule() { Target = "Namibia", Value = "bcx.com.na", IsDefault = false });

            mimConfig.OURules.Add(new MIMModels.MIMRule() { Target = "Default", Value = "OU=Provisioned Users,OU=Default", IsDefault = true });
            mimConfig.OURules.Add(new MIMModels.MIMRule() { Target = "Sanlam", Value = "OU=CS Users,OU=Cape Town - Sanlam", IsDefault = false });
            mimConfig.OURules.Add(new MIMModels.MIMRule() { Target = "NDC", Value = "OU=NDC Users,OU=NDC", IsDefault = false });
            mimConfig.OURules.Add(new MIMModels.MIMRule() { Target = "Disabled", Value = "OU=BCX Removed Users", IsDefault = false });

            mimConfig.NamedValues.Add(new MIMModels.RuleNamedValue() { NamedValueID = 1, NamedValue = "CN=MIMProvDB,CN=Databases,CN=Exchange Administrative Group (FYDIBOHF23SPDLT),CN=Administrative Groups,CN=CPX,CN=Microsoft Exchange,CN=Services,CN=Configuration,DC=enterprise,DC=root" });
            mimConfig.NamedValues.Add(new MIMModels.RuleNamedValue() { NamedValueID = 2, NamedValue = "/o=CPX/ou=Exchange Administrative Group (FYDIBOHF23SPDLT)/cn=Configuration/cn=Servers/cn=EXMAZJV01" });

            mimConfig.HomeMDBRules.Add(new MIMModels.MIMRule() { Target = "Default", Value = "", NamedValueID = 1, IsDefault = true });
            mimConfig.HomeMDBRules.Add(new MIMModels.MIMRule() { Target = "Sandton", Value = "CN=FIMProvDBCN=Databases,CN=Exchange Administrative Group (FYDIBOHF23SPDLT),CN=Administrative Groups,CN=CPX,CN=Microsoft Exchange,CN=Services,CN=Configuration,DC=enterprise,DC=root", IsDefault = false});

            mimConfig.EmailServers.Add(new MIMModels.MIMRule() { Target = "Default", NamedValueID = 2, IsDefault = true });

            mimConfig.SaveConfig("MIMConfig2022.json");


            mimConfig = MIMModels.MIMConfig.LoadMIMConfigFromFile("MIMConfig2022.json");
            //System.IO.File.WriteAllText($"Error-MIMConfig2022.json not found-{DateTime.Now.ToString("ddMMMyyyyHHmmss")}.txt", "booyah");
            
            Console.WriteLine("Country Rules Test");
            Console.WriteLine($"Country: France - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.CountryDomains, "France")}");
            Console.WriteLine($"Country: South Africa - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.CountryDomains, "South Africa")}");
            Console.WriteLine($"Country: Mozambique - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.CountryDomains, "mozambique")}");
            Console.WriteLine();

            Console.WriteLine("OU Rules Test");
            Console.WriteLine($"OU: France - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.OURules, "France")}");
            Console.WriteLine($"OU: Sanlam - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.OURules, "Sanlam")}");
            Console.WriteLine($"OU: NDC - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.OURules, "NDC")}");
            Console.WriteLine($"OU: Disabled - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.OURules, "Disabled")}");
            Console.WriteLine();

            Console.WriteLine("HomeMDB Rules Test");
            Console.WriteLine($"HomeMDB: Narnia - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.HomeMDB, "Narnia")}");
            Console.WriteLine($"HomeMDB: Midrand - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.HomeMDB, "Midrand")}");
            Console.WriteLine($"HomeMDB: Sandton - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.HomeMDB, "Sandton")}");
            Console.WriteLine();

            Console.WriteLine("EmailServer Rules Test");
            Console.WriteLine($"EmailServer: Narnia - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.EmailServer, "Narnia")}");
            Console.WriteLine($"EmailServer: Midrand - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.EmailServer, "Midrand")}");
            Console.WriteLine($"EmailServer: Sandton - Value: {mimConfig.GetValue(MIMModels.MIMConfig.RuleTypes.EmailServer, "Sandton")}");
            Console.WriteLine();

        }
    }
}