
Imports Microsoft.MetadirectoryServices



Public Class MVExtensionObject

    Implements IMVSynchronization



    Private Function GetUniqueDNName(ByVal CNName_to_Check As String, ByVal OU_to_Check As String, ByVal attribute_to_check As String) As String
        '"CN=" + CNName + "," + OU
        'This function checks the MV for a unique DN value
        'If is is not unique it will add a number at the end starting at 2.

        Dim findResultList() As MVEntry
        Dim checkedName As String = "CN=" + CNName_to_Check + "," + OU_to_Check
        GetUniqueDNName = Nothing

        ' Create a unique naming attribute by adding a number to 
        ' the existing strAcctName value. 
        Dim nameSuffix As Integer
        For nameSuffix = 2 To 20
            ' Check if the passed strAcctName value exists in the metaverse by 
            ' using the Utils.FindMVEntries method.
            findResultList = Utils.FindMVEntries(attribute_to_check, checkedName, 1)

            ' If the value does not exist in the metaverse, use the passed value 
            ' as the metaverse value. 

            If findResultList.Length = 0 Then
                GetUniqueDNName = checkedName
                Exit For
            End If

            ' If the passed value already exists, concatenate the counter number 
            ' to the passed value and verify this new value exists. Repeat 
            ' this step until a unique value is created. 
            ' checkedName = Name_to_Check + " " + nameSuffix.ToString
            CNName_to_Check = CNName_to_Check + nameSuffix.ToString
            checkedName = "CN=" + CNName_to_Check + "," + OU_to_Check


        Next

    End Function


    Private Function GetUniqueAccountName(ByVal nickName_format As String, ByVal sn_format As String, ByVal attribute_to_check As String) As String
        'This function will check if the accountname is unique in the MV
        'If is is not unique it will add the next letter of the surname

        Dim findResultList() As MVEntry
        ' Create a accountName to test
        Dim checkedName As String = nickName_format + Left(sn_format, 1)
        GetUniqueAccountName = Nothing

        Dim nameSuffix As Integer
        For nameSuffix = 2 To 20
            ' Check if the passed checkedName value exists in the metaverse by 
            ' using the Utils.FindMVEntries method. 
            findResultList = Utils.FindMVEntries(attribute_to_check, checkedName, 1)

            ' If the value does not exist in the metaverse, use the passed value 
            ' as the metaverse value. 
            If findResultList.Length = 0 Then
                GetUniqueAccountName = checkedName
                Exit For
            End If

            ' Continue to add the next value of the surname until a unique name if found. 
            checkedName = nickName_format + Left(sn_format, nameSuffix)
        Next

    End Function


    Public Sub Initialize() Implements IMVSynchronization.Initialize
        ' TODO: Add initialization code here
    End Sub

    Public Sub Terminate() Implements IMVSynchronization.Terminate
        ' TODO: Add termination code here
    End Sub

    Public Sub Provision(ByVal mventry As MVEntry) Implements IMVSynchronization.Provision

        'FIM still runs each provisioning extension against all metaverse objects so if it is not the person object exit the sub
        Select Case mventry.ObjectType
            'Do for person
            Case "person"


                ''Clean up the Portal - Delete Users without a DisplayName
                If Not mventry("displayName").IsPresent Then
                    mventry.ConnectedMAs("FIM").Connectors.DeprovisionAll()
                End If

                ''Clean up the Portal - Delete Terminated Users
                If mventry("employeeStatus").IsPresent Then
                    If mventry("employeeStatus").Value.ToLower = "terminated" Then
                        mventry.ConnectedMAs("FIM").Connectors.DeprovisionAll()
                    End If
                End If




                Dim UtilityMA As String = "Utility DB MA"

                'We need a Personal present in order to continue.
                'Also check if objectSID is present. Is it is not present there should not be a AD account and can continue to provision.
                If (mventry("employeeStatus").IsPresent) Then

                    'Instantiate an XmlDocument object.
                    Dim xmldoc As New System.Xml.XmlDocument()
                    'Load the configuration.xml
                    xmldoc.Load("C:\Program Files\Microsoft Forefront Identity Manager\2010\Synchronization Service\Extensions\Configuration.xml")
                    Dim OU As String = Nothing

                    'Get the BASEDN for AD from config file
                    Dim BASEDN As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/BASEDN")
                    Dim AD_maName As System.Xml.XmlNode = xmldoc.SelectSingleNode("//maType[.='AD']/parent::node()/maName")

                    Dim rdn As String = Nothing
                    Dim sAMAccountName As String = Nothing
                    Dim AD_MA As ConnectedMA = Nothing
                    Dim csentry As CSEntry = Nothing
                    Dim dn As ReferenceValue = Nothing
                    Dim ProvisionADUser As String = Nothing



                    'Only Provsion Active Account in SAP
                    If mventry("employeeStatus").Value = "Active" And (mventry("objectSID").IsPresent = False) And mventry("sn").IsPresent And mventry("nickName").IsPresent And mventry("displayName").IsPresent Then


                        'Should the user be provisioned?
                        'Check the provisionWorkflowOverride value. If true go and provision the user without approval
                        'Else wait for approval from HR to provision user account in AD
                        Dim provisionWorkflowOverride As System.Xml.XmlNode = xmldoc.SelectSingleNode("//maType[.='AD']/parent::node()/provisionWorkflowOverride")
                        If provisionWorkflowOverride.InnerText.ToLower = "true" Then
                            ProvisionADUser = "Yes"
                        Else
                            If mventry("ProvisionRequestAD").IsPresent Then
                                If mventry("ProvisionRequestAD").Value = "Approved" Then
                                    ProvisionADUser = "Yes"
                                Else
                                    ProvisionADUser = "No"
                                End If
                            Else
                                ProvisionADUser = "No"
                            End If
                        End If

                        'Ready for user to be provisioned
                        If ProvisionADUser = "Yes" Then

                            Dim cityOU As String = Nothing
                            Dim homeMDB As String = Nothing
                            Dim msExchHomeServerName As String = Nothing
                            Dim new_sAMAccountName As String = Nothing
                            Dim new_dn As String = Nothing


                            'Get the Domain for AD from config file
                            Dim domain As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/domain")


                            'Construct the OU, homeMDB and msExchHomeServerName value based on city value
                            'We set the cityOU to this location, which is used later to construct the DN.
                            If mventry("city").IsPresent Then
                                Select Case mventry("city").Value.ToLower
                                    'Get the following information MailboxNickName, homeMDB, OU accourding to the city name in the config file
                                    Case "midrand"
                                        Dim MidrandOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/MidrandOU")
                                        cityOU = MidrandOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Midrand")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameMidrand")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "cape town"
                                        Dim CapeTownOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/CapeTownOU")
                                        cityOU = CapeTownOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_CapeTown")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameCapeTown")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "sanlam"
                                        Dim SanlamOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/SanlamOU")
                                        cityOU = SanlamOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Sanlam")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameSanlam")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "durban"
                                        Dim DurbanOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/DurbanOU")
                                        cityOU = DurbanOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Durban")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameDurban")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "east london"
                                        Dim EastLondonOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/EastLondonOU")
                                        cityOU = EastLondonOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_EastLondon")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameEastLondon")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "ndc"
                                        Dim NDCOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/NDCOU")
                                        cityOU = NDCOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_NDC")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameNDC")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "port elizabeth"
                                        Dim PortElizabethOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/PortElizabethOU")
                                        cityOU = PortElizabethOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_PortElizabeth")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNamePortElizabeth")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "pretoria"
                                        Dim PretoriaOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/PretoriaOU")
                                        cityOU = PretoriaOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Pretoria")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNamePretoria")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case "sandton"
                                        Dim SandtonOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/SandtonOU")
                                        cityOU = SandtonOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Sandton")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameDefault")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                    Case Else 'Send this to the default OU as we don't have a valid city
                                        'Get the defaultOU on the configuration file
                                        Dim DefaultOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/DefaultOU")
                                        cityOU = DefaultOU.InnerText
                                        Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Midrand")
                                        homeMDB = homeMDBtemp.InnerText
                                        Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameDefault")
                                        msExchHomeServerName = msExchHomeServerNametemp.InnerText
                                End Select
                            Else
                                'Get the defaultOU on the configuration file
                                Dim DefaultOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/DefaultOU")
                                cityOU = DefaultOU.InnerText
                                Dim homeMDBtemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/homeMDB_Midrand")
                                homeMDB = homeMDBtemp.InnerText
                                Dim msExchHomeServerNametemp As System.Xml.XmlNode = xmldoc.SelectSingleNode("//MailServer[.='Exchange']/parent::node()/msExchHomeServerNameDefault")
                                msExchHomeServerName = msExchHomeServerNametemp.InnerText
                            End If
                            'We have all the infomation to build the OU value now
                            OU = cityOU + "," + BASEDN.InnerText



                            'This is a new user object and No sAMAccountName value availible
                            'Need to create a sAMAccountName to populate the rdn value
                            'Construct the logon value NickName + lastname
                            Dim sn As String = mventry("sn").Value
                            Dim sn_format As String = sn.ToLower()
                            'Remove any spaces in the value .e.g van heerden to vanheerden
                            sn_format = Replace(sn_format, " ", vbNullString)
                            Dim nickName As String = mventry("nickName").Value
                            Dim nickName_format As String = nickName.ToLower
                            'Remove any spaces in the value
                            nickName_format = Replace(nickName_format, " ", vbNullString)

                            'Go and generate a unique accountName
                            Dim accountName As String = GetUniqueAccountName(nickName_format, sn_format, "accountName")


                            '
                            '
                            'Build the new CN value and then check if this is unique before commiting the move
                            Dim CNName As String = nickName + " " + sn
                            Dim DNName As String
                            'DNName = "CN=" + CNName + "," + OU
                            'Check if the DN is unique
                            DNName = GetUniqueDNName(CNName, OU, "dn")
                            'Format rdn into the following "CN=John Doe"
                            Dim DNNameSplit() As String = DNName.Split(",")
                            rdn = DNNameSplit(0)


                            'Read the name of the AD Management Agent from the XML file
                            AD_MA = mventry.ConnectedMAs(AD_maName.InnerText)
                            'Now we have all the info to construct the full dn value
                            dn = AD_MA.EscapeDNComponent(rdn).Concat(OU)


                            'Now we are ready to provision the user if it does not exist in AD
                            'Check that the user is not in AD and also needs to be a active record before provisioning it.
                            If (AD_MA.Connectors.Count = 0) Then


                                'Check if this is a normal user or a mail enabled user account
                                'Can use this later on to do selective mailbox provisioning
                                Dim provisionMailbox As Boolean = True
                                Select Case True
                                    Case provisionMailbox 'Case True then provision the mailbox user
                                        Try
                                            Dim MailboxNickName As String
                                            MailboxNickName = accountName
                                            csentry = ExchangeUtils.CreateMailbox(AD_MA, dn, MailboxNickName, homeMDB)
                                            'Add msExchHomeServerName value
                                            csentry("msExchHomeServerName").Value = msExchHomeServerName
                                        Catch ex As ObjectAlreadyExistsException
                                            'Ignore if the object already exists; join rules will join the existing object
                                        Catch ex As NoSuchAttributeException
                                            'Ignor if the attribute on the mventry object is not availible at this time
                                        Catch ex As Exception
                                            ' Log exception messages to the event log and throw them to the ILM Identity Manager
                                            ' Logging.Log("Caught exception " & ex.Message, True, 1)
                                            ' EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                                            Throw
                                        End Try

                                    Case Else 'Case false then provision a normal user account.
                                        'Add the new user object to the connector space.
                                        csentry = AD_MA.Connectors.StartNewConnector("user")
                                        csentry.DN = dn
                                End Select



                                'Set the initial password
                                Dim NewPass As String = Nothing
                                Dim RandomPass As New Random()
                                Dim RandomPassNumber As Integer
                                'Generate a random number between 100000000 and 999999999
                                RandomPassNumber = RandomPass.Next(100000000, 999999999)
                                NewPass = "P@ss" + RandomPassNumber.ToString + "w0rd"
                                csentry("unicodepwd").Value = NewPass

                                'Set the account status as a normal user account for the provisioning of the user.
                                Dim ADS_UF_NORMAL_ACCOUNT As Integer = &H200
                                csentry("useraccountcontrol").IntegerValue = ADS_UF_NORMAL_ACCOUNT

                                'Set the account password to change at next logon
                                'csentry("pwdLastSet").IntegerValue = 0

                                'OK to set next three attributes if they will never change
                                csentry("sAMAccountName").Value = accountName
                                csentry("userPrincipalName").Value = accountName + domain.InnerText
                                'Set the displayName
                                csentry("displayName").Value = mventry("displayName").Value
                                csentry("givenName").Value = mventry("nickName").Value
                                csentry("sn").Value = mventry("sn").Value

                                'Commit all the values to the CS
                                Try
                                    csentry.CommitNewConnector()
                                Catch ex As ObjectAlreadyExistsException
                                    'Ignore if the object already exists; join rules will join the existing object
                                Catch ex As NoSuchAttributeException
                                    'Ignor if the attribute on the mventry object is not availible at this time
                                Catch ex As Exception
                                    ' Log exception messages to the event log and throw them to the ILM Identity Manager
                                    ' Logging.Log("Caught exception " & ex.Message, True, 1)
                                    ' EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                                    Throw
                                End Try
                            End If ' If (AD_MA.Connectors.Count = 0)
                        End If 'if ProvisionADUser = "Yes"






                        'If the user is terminated move it to the disabled OU based on the campus
                    ElseIf (mventry("employeeStatus").Value = "Terminated") And (mventry("objectSID").IsPresent) Then
                        'Get the current CN value for the user
                        If mventry("cn").IsPresent Then

                            'Go and get the Disabled OU Value from the config file
                            Dim DisabledOU As System.Xml.XmlNode = xmldoc.SelectSingleNode("//Connector[.='AD']/parent::node()/DisabledOU")
                            'Build the OU value
                            OU = DisabledOU.InnerText + "," + BASEDN.InnerText

                            '
                            '
                            'Build the new DN value and then check if this is unique before commiting the move
                            Dim CNName As String = mventry("cn").Value
                            Dim DNName As String = "CN=" + CNName + "," + OU
                            'Check if the DN is unique
                            ' DNName = GetUniqueDNName(DNName, "dn")
                            'DNName = GetUniqueDNName(CNName, OU, "dn")
                            'Format rdn into the following "CN=John Doe"
                            Dim DNNameSplit() As String = DNName.Split(",")
                            rdn = DNNameSplit(0)


                            'Read the name of the AD Management Agent from the XML file
                            AD_MA = mventry.ConnectedMAs(AD_maName.InnerText)
                            'Now we have all the info to construct the full dn value
                            dn = AD_MA.EscapeDNComponent(rdn).Concat(OU)



                            If AD_MA.Connectors.Count = 1 Then
                                csentry = AD_MA.Connectors.ByIndex(0)
                                csentry.DN = dn
                            End If
                        End If

                    End If 'If mventry("employeeStatus").Value = "Active" And (mventry("objectSID").IsPresent = False) And mventry("sn").IsPresent And mventry("nickName").IsPresent And mventry("displayName").IsPresent Then





                End If ' If (mventry("employeeStatus").IsPresent) Then



                Dim DoesExist As Boolean = False
                Dim ShouldExist As Boolean = False

                If mventry.ConnectedMAs(UtilityMA).Connectors.Count > 0 Then
                    DoesExist = True
                End If

                If mventry("employeeNumber").IsPresent Then
                    ShouldExist = True
                End If

                If ShouldExist And Not DoesExist Then
                    Dim csentry As CSEntry
                    csentry = mventry.ConnectedMAs(UtilityMA).Connectors.StartNewConnector("position")
                    csentry("DN").Value = mventry("employeeNumber").Value

                    Try
                        csentry.CommitNewConnector()
                    Catch ex As ObjectAlreadyExistsException
                        'Ignore if the object already exists; join rules will join the existing object
                    Catch ex As NoSuchAttributeException
                        'Ignor if the attribute on the mventry object is not availible at this time
                    Catch ex As Exception
                        ' Log exception messages to the event log and throw them to the ILM Identity Manager
                        ' Logging.Log("Caught exception " & ex.Message, True, 1)
                        ' EventLogger.WriteToEventLog(ex.Message, EventLogEntryType.Error)
                        Throw
                    End Try


                ElseIf DoesExist And Not ShouldExist Then
                    Dim csentry As CSEntry
                    csentry = mventry.ConnectedMAs(UtilityMA).Connectors.ByIndex(0)
                    csentry.Deprovision()
                End If



            Case "group"
                'This is the section for group objects
                'Group Provisioning is done from the Portal and not implemented here

            Case Else
                ' TODO: Remove this throw statement if you implement this method
                'Throw New EntryPointNotImplementedException()
        End Select

    End Sub

    Public Function ShouldDeleteFromMV(ByVal csentry As CSEntry, ByVal mventry As MVEntry) As Boolean Implements IMVSynchronization.ShouldDeleteFromMV
        ' TODO: Add MV deletion code here
        Throw New EntryPointNotImplementedException()
    End Function
End Class
