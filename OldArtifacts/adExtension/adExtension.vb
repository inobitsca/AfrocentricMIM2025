
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
        ' TODO: Add join mapping code here
        Throw New EntryPointNotImplementedException()
    End Sub

    Public Function ResolveJoinSearch(ByVal joinCriteriaName As String, ByVal csentry As CSEntry, ByVal rgmventry() As MVEntry, ByRef imventry As Integer, ByRef MVObjectType As String) As Boolean Implements IMASynchronization.ResolveJoinSearch
        ' TODO: Add join resolution code here
        Throw New EntryPointNotImplementedException()
    End Function

    Public Sub MapAttributesForImport(ByVal FlowRuleName As String, ByVal csentry As CSEntry, ByVal mventry As MVEntry) Implements IMASynchronization.MapAttributesForImport

        Select Case csentry.ObjectType
            Case "user"
                Select Case FlowRuleName
                    Case "cd.user:employeeNumber->mv.person:employeeNumber"
                        If Not mventry("employeeNumber").IsPresent Then
                            mventry("employeeNumber").Value = csentry("employeeNumber").Value
                        End If

                    Case "cd.user:displayName->mv.person:displayName"
                        If Not mventry("displayName").IsPresent Then
                            mventry("displayName").Value = csentry("displayName").Value
                        End If

                    Case "cd.user:sn->mv.person:sn"
                        If Not mventry("sn").IsPresent Then
                            mventry("sn").Value = csentry("sn").Value
                        End If

                    Case "cd.user:givenName->mv.person:nickName"
                        If Not mventry("nickName").IsPresent Then
                            mventry("nickName").Value = csentry("givenName").Value
                        End If

                        ' The department is for test purposes only and needs to be removed before going live
                        ' remember to check the portal for additional flow rules
                    Case "cd.user:department->mv.person:department"
                        If Not mventry("department").IsPresent Then
                            mventry("department").Value = csentry("department").Value
                        End If

                    Case "cd.user:employeeNumber->mv.person:employeeNr"
                        If Not mventry("employeeNr").IsPresent Then
                            If csentry("employeeNumber").IsPresent Then
                                Try
                                    Dim tempEmpNr As Integer = Convert.ToInt32(csentry("employeeNumber").Value)
                                    mventry("employeeNr").Value = tempEmpNr.ToString
                                Catch
                                    'Add some Error handling. 
                                    'Throw
                                End Try
                            End If
                        End If
                    Case Else
                        ' TODO: remove the following statement and add your default script here
                        Throw New EntryPointNotImplementedException()
                End Select

            Case "contact"
                Select Case FlowRuleName
                    Case "cd.contact:->mv.person:employeeType"
                        mventry("EmployeeType").Value = "Contact"

                    Case "cd.contact:telephoneNumber->mv.person:telephoneNumber"
                        mventry("telephoneNumber").Value = csentry("telephoneNumber").Value

                    Case "cd.contact:department->mv.person:department"
                        mventry("department").Value = csentry("department").Value

                    Case "cd.contact:displayName->mv.person:displayName"
                        mventry("displayName").Value = csentry("displayName").Value

                    Case "cd.contact:mobile->mv.person:mobile"
                        mventry("mobile").Value = csentry("mobile").Value

                    Case "cd.contact:sn->mv.person:sn"
                        mventry("sn").Value = csentry("sn").Value

                    Case Else
                        Throw New EntryPointNotImplementedException()
                End Select
            Case Else
                Throw New EntryPointNotImplementedException()
        End Select
    End Sub

    Public Sub MapAttributesForExport(ByVal FlowRuleName As String, ByVal mventry As MVEntry, ByVal csentry As CSEntry) Implements IMASynchronization.MapAttributesForExport


         Select FlowRuleName

            'Set the AD account to Enable or Disabled accourding to HR
            Case "cd.user:userAccountControl<-mv.person:employeeStatus,accountName"
                'Get the employeeStatus for the record. Need to check the Staff and Contractor status.
                Dim employeeStatus As String = Nothing
                If mventry("employeeStatus").IsPresent And
                   csentry("userAccountControl").IsPresent Then
                    employeeStatus = mventry("employeeStatus").Value

                    Dim ADS_UF_NORMAL_ACCOUNT As Long = &H200
                    Dim ADS_UF_ACCOUNTDISABLE As Long = &H2
                    Dim ADS_UF_PASSWD_NOTREQD As Long = &H20

                    Dim currentValue As Long = csentry("userAccountControl").IntegerValue

                    Select Case (employeeStatus)
                        Case "Active"
                            csentry("userAccountControl").IntegerValue = (currentValue Or ADS_UF_NORMAL_ACCOUNT) _
                                                                            And (Not ADS_UF_ACCOUNTDISABLE)
                        Case "Terminated"
                            'csentry("userAccountControl").IntegerValue = currentValue _
                            '                                        Or ADS_UF_ACCOUNTDISABLE _
                            '   Or ADS_UF_PASSWD_NOTREQD
                            csentry("userAccountControl").IntegerValue = currentValue _
                                                                      Or ADS_UF_ACCOUNTDISABLE
                    End Select
                End If



                'Set the accountExpires value if the EmployeeEndDate value is set
                'The account will expire at the end on the day of expiry
            Case "cd.user:accountExpires<-mv.person:accountName,employeeEndDate"
                If mventry("employeeEndDate").IsPresent Then
                    Dim tempDate As Date
                    tempDate = Convert.ToDateTime(mventry("employeeEndDate").Value)
                    'Add a day so that the account expires only expires at the EOD.
                    tempDate = tempDate.AddDays(1)
                    csentry("accountExpires").Value = tempDate.ToFileTimeUtc().ToString()
                Else
                    'Set the accountExpires to never expire
                    csentry("accountExpires").Value = "9223372036854775807"
                End If

            Case "cd.user:sn<-mv.person:sn"
                csentry("sn").Value = mventry("sn").Value

            Case "cd.user:extensionAttribute4<-mv.person:employeeType"
                csentry("extensionAttribute4").Value = mventry("employeeType").Value

            Case Else
                ' TODO: remove the following statement and add your default script here
                Throw New EntryPointNotImplementedException()



        End Select




    End Sub

    Public Function Deprovision(ByVal csentry As CSEntry) As DeprovisionAction Implements IMASynchronization.Deprovision
        ' TODO: Remove this throw statement if you implement this method
        Throw New EntryPointNotImplementedException()
    End Function
End Class
