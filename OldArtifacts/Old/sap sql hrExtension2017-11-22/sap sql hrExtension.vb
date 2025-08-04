Imports Microsoft.MetadirectoryServices

Public Class MAExtensionObject
    Implements IMASynchronization

    Public Sub Initialize() Implements IMASynchronization.Initialize

        ' TODO: Add initialization code here
    End Sub

    Public Sub Terminate() Implements IMASynchronization.Terminate
        ' TODO: Add termination code here
    End Sub

    Public Function ShouldProjectToMV(ByVal csentry As CSEntry, ByRef MVObjectType As String) As Boolean Implements IMASynchronization.ShouldProjectToMV
        ' TODO: Remove this throw statement if you implement this method
        Throw New EntryPointNotImplementedException()
    End Function

    Public Function FilterForDisconnection(ByVal csentry As CSEntry) As Boolean Implements IMASynchronization.FilterForDisconnection
        ' TODO: Add connector filter code here
        Throw New EntryPointNotImplementedException()
    End Function

    Public Sub MapAttributesForJoin(ByVal FlowRuleName As String, ByVal csentry As CSEntry, ByRef values As ValueCollection) Implements IMASynchronization.MapAttributesForJoin
        Select FlowRuleName

            'Cater for duplicate ID number entries
            Case "cd.personIDnumber"
                If Convert.ToBoolean(csentry("EmplStatus").Value) = False Then

                    'trying to skip the duplicate entries that are no longer active 

                End If

                'Cater for duplicate employeeNumbers
            Case "cd.personEmployeeNumber"
                If Convert.ToBoolean(csentry("EmplStatus").Value) = False Then

                    'trying to skip the duplicate entries that are no longer active 

                End If

            Case Else
                Throw New EntryPointNotImplementedException
        End Select
    End Sub

    Public Function ResolveJoinSearch(ByVal joinCriteriaName As String, ByVal csentry As CSEntry, ByVal rgmventry() As MVEntry, ByRef imventry As Integer, ByRef MVObjectType As String) As Boolean Implements IMASynchronization.ResolveJoinSearch
        ' TODO: Add join resolution code here
        Throw New EntryPointNotImplementedException()
    End Function

    Public Sub MapAttributesForImport(ByVal FlowRuleName As String, ByVal csentry As CSEntry, ByVal mventry As MVEntry) Implements IMASynchronization.MapAttributesForImport

        'Set the Format for the Date/Time attributes.
        'Dim FormatStringA As String = "yyyy/MM/dd"
        Dim FormatStringB As String = "yyyy-MM-ddT00:00:00.000"

        Select Case FlowRuleName

            Case "cd.person:EmployeeNumber->mv.person:hrSourceMA"
                If csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        Dim x As String = csentry.MA.Name.ToString.ToLower
                        If x.Contains("contractor") = True Then
                            mventry("hrSourceMA").Value = "contractor"
                        ElseIf x.Contains("staff") = True Then
                            mventry("hrSourceMA").Value = "staff"
                        End If
                    End If
                End If

                'Format the EmployeeStartDate in the MV to "yyyy/MM/ddTHH:mm:ss.000". Set the time value to 00H00.
            Case "cd.person:EmployeeStartDate->mv.person:employeeStartDate"
                If csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        If csentry("EmployeeStartDate").IsPresent Then
                            Dim CSDate As String
                            CSDate = csentry("EmployeeStartDate").Value 'Read the date from the CS
                            If IsDate(CSDate) Then 'Check date is a valid date and not just a blank value
                                Dim ParsedDate As DateTime = DateTime.Parse(CSDate) 'This interprets the string as a date
                                Dim NewlyFormattedDate As String = ParsedDate.ToString(FormatStringB) 'This formats the date into the desired format
                                mventry("EmployeeStartDate").Value = NewlyFormattedDate 'This puts the newly formatted date into the MV
                            Else
                                mventry("EmployeeStartDate").Delete() 'Not a valid date, or an Empty String, lets clear the attribute in the Metaverse
                            End If
                        Else
                            mventry("EmployeeStartDate").Delete() 'No Date present, lets clear the attribute in the Metaverse
                        End If
                    End If
                End If


            Case "cd.person:EmployeeEndDate->mv.person:employeeEndDate"
                'Format the EmployeeEndDate in the MV to "yyyy/MM/ddTHH:mm:ss.000".
                If csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        'End date of "9999-12-31 00:00:00" is the default value for no end date
                        If csentry("EmployeeEndDate").IsPresent Then
                            If csentry("EmployeeEndDate").Value = "9999-12-31 00:00:00" Then
                                mventry("employeeEndDate").Delete()
                            Else
                                Dim CSDate As String
                                CSDate = csentry("EmployeeEndDate").Value 'Read the date from the CS
                                If IsDate(CSDate) Then 'Check date is a valid date and not just a blank value
                                    Dim ParsedDate As DateTime = DateTime.Parse(CSDate) 'This interprets the string as a date
                                    Dim NewlyFormattedDate As String = ParsedDate.ToString(FormatStringB) 'This formats the date into the desired format
                                    mventry("employeeEndDate").Value = NewlyFormattedDate 'This puts the newly formatted date into the MV
                                Else
                                    mventry("employeeEndDate").Delete() 'Not a valid date, or an Empty String, lets clear the attribute in the Metaverse
                                End If
                            End If
                        Else
                            mventry("employeeEndDate").Delete() 'No TerminationDate present, lets clear the attribute in the Metaverse
                        End If
                    End If
                End If


                'Process FirstName if allowed
            Case "cd.person:FirstName->mv.person:fullName"
                If csentry("FirstName").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("fullname").Value = csentry("FirstName").Value
                    End If
                End If

                'Process PhoneFax if allowed
            Case "cd.person:PhoneFax->mv.person:facsimileTelephoneNumber"
                If csentry("PhoneFax").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("facsimileTelephoneNumber").Value = csentry("PhoneFax").Value
                    End If
                End If

                'Process GeoLocation the employeeNumbers match
            Case "cd.person:GeoLocation->mv.person:location"
                If csentry("Geolocation").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("location").Value = csentry("GeoLocation").Value
                    End If
                End If

                'Process AddrRegion the employeeNumbers match
            Case "cd.person:AddrRegion->mv.person:region"
                If csentry("AddrRegion").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("region").Value = csentry("AddrRegion").Value
                    End If
                End If

                'Process PostalCode the employeeNumbers match
            Case "cd.person:PostalCode->mv.person:postalCode"
                If csentry("PostalCode").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("postalCode").Value = csentry("PostalCode").Value
                    End If
                End If

                'Process IDNr the employeeNumbers match
            Case "cd.person:IDNr->mv.person:IDNumber"
                If csentry("IDNr").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("IDNumber").Value = csentry("IDNr").Value
                    End If
                End If


                'Process OffAddSuburb the employeeNumbers match Then
            Case "cd.person:OffAddSuburb->mv.person:suburb"
                If csentry("OffAddSuburb").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("suburb").Value = csentry("OffAddSuburb").Value
                    End If
                End If

                'Process OffAddStrNbr when the employeeNumbers match
            Case "cd.person:OffAddStrNbr->mv.person:streetNbr"
                If csentry("OffAddStrNbr").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("streetNbr").Value = csentry("OffAddStrNbr").Value
                    End If
                End If

                'Process Manager when the employeeNumbers match
            Case "cd.person:Manager->mv.person:managerName"
                If csentry("Manager").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("managerName").Value = csentry("Manager").Value
                    End If
                End If


            Case "cd.person:ManagerPernr->mv.person:superiorPosition"
                If csentry("ManagerPernr").IsPresent And
                  csentry("EmployeeNumber").IsPresent And
                  mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("superiorPosition").Value = csentry("ManagerPernr").Value.ToString
                    End If
                End If




                'Test to see if there need to be a workflow kicked off to provision a new user
                'First test and see if all the attributes are availible

                ' DOES THIS ONE NEED REFINING ?!?!?!?!?!?!?
            Case "cd.person:<dn>->mv.person:ProvisionRequestAD"
                If (mventry("objectSid").IsPresent = False) And
                   (mventry("ProvisionRequestAD").IsPresent = False) And
                    mventry("employeeStatus").IsPresent And
                    mventry("nickName").IsPresent And
                    mventry("sn").IsPresent And
                    mventry("displayName").IsPresent Then
                    If mventry("employeeStatus").Value = "Active" Then
                        mventry("ProvisionRequestAD").Value = "Not Approved"
                    End If
                End If

                'Convert Employee Number from number type to a string type
            Case "cd.person:EmployeeNumber->mv.person:employeeNumber"
                ' possible problem for new entries if values are not present
                'added the mv-employeeNumber is present check in first if to exclude new entries from older where Id numbers are not present
                'note: if the EmplStatus value = true (Active) then we process else not
                'parsedate results are: if parsedate is less than today then return value is -1
                '                       if parsedate is equal to today then return value is 0
                '                       if parsedate is greater than today then return value is 1

                If mventry("IDNumber").IsPresent And
                   mventry("employeeNumber").IsPresent Then
                    If csentry("IDNr").IsPresent And
                       csentry("EmployeeNumber").IsPresent And
                       csentry("EmplStatus").IsPresent And
                       csentry("EmployeeType").IsPresent And
                       csentry("EmployeeStartDate").IsPresent Then
                        If csentry("IDNr").Value = mventry("IDNumber").Value And
                           csentry("EmployeeNumber").Value <> mventry("employeeNumber").Value Then
                            If Convert.ToBoolean(csentry("EmplStatus").Value) = True Then
                                Dim CSDate As String
                                CSDate = csentry("EmployeeStartDate").Value 'Read the date from the CS
                                If IsDate(CSDate) Then 'Check date is a valid date and not just a blank value
                                    Dim ParsedDate As DateTime = DateTime.Parse(CSDate) 'This interprets the string as a date
                                    Dim dateCompResult As Integer = Date.Compare(ParsedDate, Today)
                                    If dateCompResult <= 0 Then
                                        mventry("employeeNumber").Value = Convert.ToString(csentry("EmployeeNumber").Value)
                                    End If
                                End If
                            End If
                        End If
                    End If
                ElseIf Not mventry("employeeNumber").IsPresent And
                       Not mventry("IDNumber").IsPresent Then
                    mventry("employeeNumber").Value = Convert.ToString(csentry("EmployeeNumber").Value)

                ElseIf mventry("employeeNumber").IsPresent And
                   Not mventry("IDNumber").IsPresent Then
                    mventry("employeeNumber").Value = Convert.ToString(csentry("EmployeeNumber").Value)

                End If


                'Convert Employee Number from number type to a string type
            Case "cd.person:EmployeeNumber->mv.person:employeeNr"
                If csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("employeeNr").Value = Convert.ToString(csentry("EmployeeNumber").Value)
                    End If
                End If

                'If the nickname is present then trim the spaces
            Case "cd.person:NickName->mv.person:nickName"
                If csentry("NickName").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("nickName").Value = Trim(csentry("NickName").Value)
                    End If
                End If

                'If the surname is present then trim the spaces
            Case "cd.person:LastName->mv.person:sn"
                If csentry("LastName").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("sn").Value = Trim(csentry("LastName").Value)
                    End If
                End If

                'Process PhoneOffice when the employeeNumbers match
            Case "cd.person:PhoneOffice->mv.person:telephoneNumber"
                If csentry("PhoneOffice").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("telephoneNumber").Value = csentry("PhoneOffice").Value
                    End If
                End If

                'Process PhoneIntOffice when the employeeNumbers match
            Case "cd.person:PhoneIntOffice->mv.person:officeIntPhone"
                If csentry("PhoneIntOffice").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("officeIntPhone").Value = csentry("PhoneIntOffice").Value
                    End If
                End If

                'Process PhoneMobile when the employeeNumbers match
            Case "cd.person:PhoneMobile->mv.person:mobile"
                If csentry("PhoneMobile").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("mobile").Value = csentry("PhoneMobile").Value
                    End If
                End If

                'Set the EmployeeStatus from the Staff and Contractor Employee Status calulated above.
            Case "cd.person:EmplStatus->mv.person:employeeStatus"
                If csentry("EmplStatus").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        Select Case csentry("EmplStatus").Value
                            Case "True"
                                mventry("employeeStatus").Value = "Active"
                            Case "False"
                                mventry("employeeStatus").Value = "Terminated"
                        End Select
                    End If
                End If

                'Process Country the employeeNumbers match
            Case "cd.person:Country->mv.person:country"
                If csentry("Country").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("country").Value = csentry("Country").Value
                    End If
                End If

                'Process OffAddStrName the employeeNumbers match
            Case "cd.person:OffAddStrName->mv.person:street"
                If csentry("OffAddStrName").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("street").Value = csentry("OffAddStrName").Value
                    End If
                End If

                'Process City the employeeNumbers match
            Case "cd.person:City->mv.person:city"
                If csentry("City").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("city").Value = csentry("City").Value
                    End If
                End If

                'Process EmployeeType the employeeNumbers match
            Case "cd.person:EmployeeType->mv.person:employeeType"
                If csentry("EmployeeType").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("employeeType").Value = csentry("EmployeeType").Value
                    End If
                End If ' Some new settings and not sure how to handle this one

                'Process ManagerPernr the employeeNumbers match
            Case "cd.person:ManagerPernr->mv.person:manager"
                ' Came up with an error when setting the rules extention"
                'Error - "Defining a rules extension import attribute flow to a metaverse reference attribute is not allowed"

                'Build the Displayname from the nickName and Surname values and add - BCX to the end.
            Case "cd.person:LastName,NickName->mv.person:displayName"
                If csentry("LastName").IsPresent And
                   csentry("NickName").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        Dim sn As String = Trim(csentry("LastName").Value)
                        Dim Nickname As String = Trim(csentry("NickName").Value)
                        mventry("displayName").Value = Nickname + " " + sn + " - " + "BCX"
                    End If
                End If

                'Process JobTitle if the employeeNumbers match
            Case "cd.person:JobTitle->mv.person:jobTitle"
                If csentry("JobTitle").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("jobTitle").Value = csentry("JobTitle").Value
                    End If
                End If

                'Process Department if the employeeNumbers match
            Case "cd.person:Department->mv.person:department"
                If csentry("Department").IsPresent And
                   csentry("EmployeeNumber").IsPresent And
                   mventry("EmployeeNumber").IsPresent Then
                    If csentry("EmployeeNumber").Value = mventry("EmployeeNumber").Value Then
                        mventry("department").Value = csentry("Department").Value
                    End If
                End If

            Case Else
                ' TODO: remove the following statement and add your default script here
                Throw New EntryPointNotImplementedException()

        End Select
    End Sub

    Public Sub MapAttributesForExport(ByVal FlowRuleName As String, ByVal mventry As MVEntry, ByVal csentry As CSEntry) Implements IMASynchronization.MapAttributesForExport
        ' TODO: Add export attribute flow code here
        Throw New EntryPointNotImplementedException()
    End Sub

    Public Function Deprovision(ByVal csentry As CSEntry) As DeprovisionAction Implements IMASynchronization.Deprovision
        ' TODO: Remove this throw statement if you implement this method
        Throw New EntryPointNotImplementedException()
    End Function
End Class
