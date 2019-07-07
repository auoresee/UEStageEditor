Public Class EnemyData
    Implements ICloneable

    Public image_path As String
    Public spawn_x As Integer
    Public spawn_y As Integer
    Public spawn_time As Integer
    Public spawn_manual As Boolean
    Public category As Byte
    Public type As Integer
    Public hp As Integer
    Public script As String

    Sub New(ByVal p2 As String, p3 As Integer, p4 As Integer, p5 As Integer, p6 As Boolean, p7 As Byte, p8 As Integer, p9 As Integer, p10 As String)
        'name = p1 廃止
        image_path = p2
        spawn_x = p3
        spawn_y = p4
        spawn_time = p5
        spawn_manual = p6
        category = p7
        type = p8
        hp = p9
        script = p10
    End Sub

    Function Clone() As Object Implements ICloneable.Clone
        Return New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, category, type, hp, script)
    End Function
End Class
