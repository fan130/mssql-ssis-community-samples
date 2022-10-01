Imports <xmlns:z="#RowsetSchema">
Imports <xmlns:rs="urn:schemas-microsoft-com:rowset">
Imports <xmlns:t="http://schemas.microsoft.com/sharepoint/soap/">

Imports System.ComponentModel
Imports System.Net
Imports System.Security.Principal
Imports System.ServiceModel
Imports System.ServiceModel.Channels
Imports Microsoft.Samples.SqlServer.SSIS.SharePointUtility.DataObject



Namespace Adapter

    ''' <summary>
    ''' Performs common operations against the SharePoint Lists Service. Provices pooling of connection information when necessary
    ''' among all related calls.
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class ViewsAdapter : Implements IDisposable

        Private _disposed As Boolean
        Private _sharepointClient As ViewsService.ViewsSoapClient
        Private _sharepointUri As Uri
        Private _webserviceUrl As String = "/_vti_bin/views.asmx"
        Private _credential As System.Net.NetworkCredential

        ''' <summary>
        ''' Constructor keeps an instance of the Views Service handy using passed in network credential
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByVal sharepointUri As Uri, ByVal credential As NetworkCredential)
            InitializeObject(sharepointUri, credential)
        End Sub

        ''' <summary>
        ''' Method to centralize all initializations of this private object
        ''' </summary>
        ''' <param name="sharepointUri">Path to SharePoint</param>
        ''' <param name="credential">Network Credential of User to use</param>
        ''' <remarks></remarks>
        Public Sub InitializeObject(ByVal sharepointUri As Uri, ByVal credential As NetworkCredential)
            Dim sharePointPath As String = sharepointUri.AbsoluteUri.ToLower()
            If (Not sharePointPath.EndsWith(_webserviceUrl)) Then
                _sharepointUri = New Uri(sharePointPath.TrimEnd("/") + _webserviceUrl)
            Else
                _sharepointUri = New Uri(sharePointPath)
            End If
            If (credential Is Nothing) Then
                _credential = CredentialCache.DefaultNetworkCredentials()
            Else
                _credential = credential
            End If

            ResetConnection()
        End Sub

        ''' <summary>
        ''' Resets the conneciton for the current client which is used for the views service
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ResetConnection()
            ' Setup the binding with some enlarged buffers for SharePoint
            Dim binding = New BasicHttpBinding()

            ' Change the security mode if we're using http vs https
            If (_sharepointUri.Scheme.ToLower() = "http") Then
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly
            ElseIf (_sharepointUri.Scheme.ToLower() = "https") Then
                binding.Security.Mode = BasicHttpSecurityMode.Transport
            Else
                Throw New ArgumentException("Sharpeoint URL Scheme is not recognized: " + _sharepointUri.Scheme)
            End If

            ' Send credentials and adjust the buffer sizes (SharePoint can send big packets of data)
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows
            binding.MaxReceivedMessageSize = Int32.MaxValue
            binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue
            binding.ReaderQuotas.MaxDepth = Int32.MaxValue
            binding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue

            binding.OpenTimeout = New TimeSpan(24, 0, 0)
            binding.CloseTimeout = New TimeSpan(24, 0, 0)
            binding.ReceiveTimeout = New TimeSpan(24, 0, 0)
            binding.SendTimeout = New TimeSpan(24, 0, 0)

            ' Create the client with the given settings
            Dim ep = New EndpointAddress(_sharepointUri)

            ' Create the client object
            If (Not _sharepointClient Is Nothing) Then
                Dim dispose As IDisposable = _sharepointClient
                dispose.Dispose()
                _sharepointClient = Nothing
            End If
            _sharepointClient = New ViewsService.ViewsSoapClient(binding, ep)

            ' Only need to add this once, the endpoint will be shared for future instances
            Dim clientCredentials As Description.ClientCredentials = _
                (From e In _sharepointClient.Endpoint.Behaviors _
                 Where TypeOf (e) Is Description.ClientCredentials).Single()
            clientCredentials.Windows.AllowedImpersonationLevel = _
                TokenImpersonationLevel.Impersonation

            clientCredentials.Windows.ClientCredential = _credential

            ' Add authentication support header
            AddCustomHeader(New System.ServiceModel.OperationContextScope(_sharepointClient.InnerChannel))
        End Sub

        ''' <summary>
        ''' Adds header property to state that forms based auth is accepted
        ''' </summary>
        ''' <param name="scope"></param>
        ''' <remarks></remarks>
        Private Sub AddCustomHeader(ByVal scope As System.ServiceModel.OperationContextScope)

            Dim reqProp = New System.ServiceModel.Channels.HttpRequestMessageProperty()
            reqProp.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f")
            System.ServiceModel.OperationContext.Current.OutgoingMessageProperties(System.ServiceModel.Channels.HttpRequestMessageProperty.Name) = reqProp

        End Sub


        ''' <summary>
        ''' Gets the fields in a target SharePoint list
        ''' </summary>
        ''' <param name="listName">Name of a list to load from SharePoint</param>
        ''' <returns>A list of SharepoingField objects that describe the field</returns>
        ''' <remarks></remarks>
        Public Function GetViewList(ByVal listName As String) As IEnumerable(Of ViewData)

            Try
                Dim viewData = GetSharePointListViews(listName)
                Dim viewInfo = _
                        From l In viewData...<t:View> _
                        Select New ViewData With _
                        { _
                            .Name = l.@Name, _
                            .DisplayName = l.@DisplayName _
                        }

                Return viewInfo

            Catch ex As System.ServiceModel.FaultException
                Throw New SharePointUnhandledException("Unhandled SharePoint Exception", ex)
            End Try

        End Function

        ''' <summary>
        ''' Wrapper to get SharePoint list's views from webservice for a given list
        ''' </summary>
        ''' <param name="listName">Name of list to get views for</param>
        ''' <returns>XML from SharePoint API</returns>
        ''' <remarks></remarks>
        Private Function GetSharePointListViews( _
                ByVal listName As String) As XElement

            Dim client = _sharepointClient
            Try
                Return client.GetViewCollection(listName)
            Catch ex As System.ServiceModel.FaultException
                Throw New SharePointUnhandledException("Unspecified SharePoint Error.  A possible reason might be you are trying to retrieve too many items at a time (Batch size)", ex)
            End Try

        End Function


        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    ' Free Managed Objects
                    Dim dispose As IDisposable
                    dispose = _sharepointClient
                    dispose.Dispose()
                    _sharepointClient = Nothing
                End If
                ' Free your own state (unmanaged objects).
                ' Set large fields to null.

            End If
            _disposed = True
        End Sub


#Region " IDisposable Support "

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region


    End Class
End Namespace
