Namespace DataObject
    ''' <summary>
    ''' Data Object for SharePoint Field Choices
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ColumnChoiceData

        Private _name As String

        ''' <summary>
        ''' Name of a choice among choices specified by the user in SharePoint
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

    End Class
End Namespace