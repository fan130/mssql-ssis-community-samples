Namespace DataObject
    ''' <summary>
    ''' Data Object for SharePoint Field Information
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ColumnData

        Private Shared _columnLengthLookup As Dictionary(Of String, Short)

        Private Const MAX_LENGTH As Short = 4000
        Private Const SP_SPACE As String = "_x0020_"

        Private _name As String
        Private _displayName As String
        Private _type As String
        Private _isReadOnly As Boolean
        Private _IsInView As Boolean
        Private _isHidden As Boolean
        Private _maxLength As Integer
        Private _choices As List(Of ColumnChoiceData)

        ''' <summary>
        ''' Make sure the max length lookup is primed and ready
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            ColumnData.SetupMaxLengthLookup()
        End Sub

        ''' <summary>
        ''' The SharePoint ID for the column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return _name
            End Get
            Friend Set(ByVal value As String)
                _name = value
            End Set
        End Property

        ''' <summary>
        ''' The display name given to the column stored in SharePoint
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DisplayName() As String
            Get
                Return _displayName
            End Get
            Friend Set(ByVal value As String)
                _displayName = value
            End Set
        End Property

        ''' <summary>
        ''' What SharePoint has stored as the value for this column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SharePointType() As String
            Get
                Return _type
            End Get
            Friend Set(ByVal value As String)
                _type = value

                If (Me.MaxLength = 0) Then
                    If (_columnLengthLookup.ContainsKey(_type)) Then
                        Me.MaxLength = _columnLengthLookup(_type)
                    Else
                        ' Worse case, we say this is the max length for an nvarchar field.
                        Me.MaxLength = MAX_LENGTH
                    End If
                End If
            End Set
        End Property

        ''' <summary>
        ''' Whether this field is marked readonly by SharePoint
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsReadOnly() As Boolean
            Get
                Return _isReadOnly
            End Get
            Friend Set(ByVal value As Boolean)
                _isReadOnly = value
            End Set
        End Property

        ''' <summary>
        ''' Whether this field is marked as hidden by SharePoint
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsHidden() As Boolean
            Get
                Return _isHidden
            End Get
            Friend Set(ByVal value As Boolean)
                _isHidden = value
            End Set
        End Property

        ''' <summary>
        ''' Whether this field is marked as hidden by SharePoint
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsInView() As Boolean
            Get
                Return _isInView
            End Get
            Friend Set(ByVal value As Boolean)
                _isInView = value
            End Set
        End Property
        ''' <summary>
        ''' The Max Length of the field, determined by the datatype. This cannot be set to 0. It is -1 if there is no limit. 
        ''' Otherwise, setting the datatype will also set this if it is not defined by SharePoint itself.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MaxLength() As Integer
            Get
                Return _maxLength
            End Get
            Friend Set(ByVal value As Integer)
                If (value <> 0) Then
                    _maxLength = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' List of choices specified for a choice column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Choices() As List(Of ColumnChoiceData)
            Get
                Return _choices
            End Get
            Friend Set(ByVal value As List(Of ColumnChoiceData))
                _choices = value
            End Set
        End Property

        ''' <summary>
        ''' Name of a column with the ugly SharePoint Encoded Space character removed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FriendlyName() As String
            Get
                Return Name.Replace(SP_SPACE, "")
            End Get
        End Property

        ''' <summary>
        ''' Sets up a lookup for the column lengths in case it is not set
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub SetupMaxLengthLookup()

            If (ColumnData._columnLengthLookup Is Nothing) Then

                ColumnData._columnLengthLookup = New Dictionary(Of String, Short)()

                ' Documented Default Column documented:
                ' http://msdn.microsoft.com/en-us/library/microsoft.sharepoint.spfieldtype.aspx
                With ColumnData._columnLengthLookup
                    ' Bits in SharePoint (short)
                    .Add("AllDayEvent", 5)
                    .Add("Attachments", 5)
                    .Add("Boolean", 5)
                    .Add("CrossProjectLink", 5)
                    .Add("Recurrence", 5)

                    ' Datetime
                    .Add("DateTime", 20)

                    ' dynamic? (estimate)
                    .Add("Computed", 4000)

                    ' Multi choice fields
                    .Add("LookupMulti", 4000)
                    .Add("UserMulti", 4000)

                    ' NText
                    .Add("GrdChoice", -1)
                    .Add("MultiChoice", -1)
                    .Add("MultiColumn", -1)
                    .Add("Note", -1)

                    ' Numeric - int, float, etc (estimate)
                    .Add("Counter", 25)
                    .Add("Integer", 25)
                    .Add("Currency", 25)
                    .Add("Number", 25)

                    ' Nvarchar(255) (estimate)
                    .Add("Choice", 255)
                    .Add("Text", 255)
                    .Add("File", 256)

                    ' nvarchar - large
                    .Add("Threading", 4000)
                    .Add("URL", 4000)

                    ' Sql_variant in SharePoint (estimate)
                    .Add("Calculated", 255)

                    ' Uniqueidentifier
                    .Add("Guid", 50)

                    ' Varbinary 
                    .Add("ContentTypeId", 1024)
                    .Add("ThreadIndex", 1024)

                    ' Data might be text or int (need to check)
                    .Add("Lookup", 255)
                    .Add("ModStat", 10)
                    .Add("PageSeparator", 50)
                    .Add("User", 100)
                    .Add("WorkflowEventType", 100)
                    .Add("WorkflowStatus", 100)
                End With
            End If
        End Sub
    End Class
End Namespace
