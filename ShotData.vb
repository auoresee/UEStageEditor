
Public Class ShotData
    Implements IComparable

    Public action_type As Integer
    Public script As String

    Sub New(p_action_type As Integer, p_script As String)
        ' TODO: Complete member initialization 
        action_type = p_action_type
        script = p_script
    End Sub

    '自分自身がobjより小さいときはマイナスの数、大きいときはプラスの数、
    '同じときは0を返す
    Public Function CompareTo(ByVal obj As Object) As Integer _
        Implements System.IComparable.CompareTo

        'Nothingより大きい
        If obj Is Nothing Then
            Return 1
        End If

        '違う型とは比較できない
        If Not Me.GetType() Is obj.GetType() Then
            Throw New ArgumentException("別の型とは比較できません。", "obj")
        End If

        'action_typeを比較する
        Return Me.action_type.CompareTo(DirectCast(obj, ShotData).action_type)
    End Function
End Class
