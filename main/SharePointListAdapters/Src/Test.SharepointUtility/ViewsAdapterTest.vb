Imports Microsoft.Samples.SqlServer.SSIS.SharePointUtility.DataObject
Imports Microsoft.Samples.SqlServer.SSIS.SharePointUtility.Adapter
Imports System.Collections.Generic
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting



'''<summary>
'''This is a test class for ViewsAdapterTest and is intended
'''to contain all ViewsAdapterTest Unit Tests
'''</summary>
<TestClass()> _
Public Class ViewsAdapterTest
    Private testContextInstance As TestContext

    ''' <summary>
    ''' SharePoint list that uses HTTPS, will NOT be modified, used for verifying access to list
    ''' </summary>
    ''' <remarks></remarks>
    Private testSslSitePath As String = "https://spsites.microsoft.com/sites/ASMSI"
    Private testSslSiteListName As String = "Team Contacts"
    Private testSslSiteListViewName As String = "Capacity Mgmt"


    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property


    '''<summary>
    '''A test for GetViewList
    '''</summary>
    <TestMethod()> _
    <Ignore()> _
    Public Sub GetViewListTest()
        'Dim sharepointUri As Uri = New Uri(testSslSitePath)
        'Dim target As ViewsAdapter = New ViewsAdapter(sharepointUri)
        'Dim listName As String = testSslSiteListName

        'Dim actual = target.GetViewList(listName)

        '' Assert if the test list view does not exist
        'Dim check = _
        '    From x _
        '    In actual _
        '    Where x.DisplayName.ToUpper() = testSslSiteListViewName.ToUpper() _
        '    Select x

        'Assert.AreEqual(1, check.Count())

    End Sub


End Class
