Imports System.IO
Imports System.Text

Public Class StageReader
    Public Sub LoadStage(ByVal file_name As String, ByRef stage_name As String, ByRef stage_length As Integer, ByRef bg_path As String,
                         ByRef first_bgm As Integer, ByRef bgm_path_list As String(), ByRef enemy_list As Generic.List(Of EnemyData),
                         ByRef shot_list As Generic.List(Of ShotData), ByRef global_script As String)
        'Dim ofd As New OpenFileDialog()
        'ofd.Filter = "データ ファイル(*.dat)|*.dat"
        'ofd.Title = "開く"
        'ofd.RestoreDirectory = True
        'If ofd.ShowDialog() = DialogResult.Cancel Then
        'Throw New Exception()
        'End If
        'temp_name = ofd.FileName

        Dim sr As Stream
        Dim br As BinaryReader

        Try
            sr = File.OpenRead(file_name)
            br = New BinaryReader(sr)
        Catch ex As Exception
            Throw ex
        End Try

        enemy_list.Clear()
        ReadHeader(br, stage_name)
        ReadStageSector(br, stage_length, bg_path, first_bgm, bgm_path_list, enemy_list, shot_list, global_script)
        sr.Close()

    End Sub

    Private Sub ReadHeader(br As BinaryReader, ByRef stage_name As String)
        Static STAGE_FILE_SIGNATURE As Byte() = {Asc("S"c), Asc("T"c), &H80, &H9A}
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        Dim signature As Byte()
        Dim len As Integer
        signature = br.ReadBytes(4)
        If (Not signature.SequenceEqual(STAGE_FILE_SIGNATURE)) Then
            Throw New Exception()
        End If

        len = BytesToInt(br.ReadBytes(4))
        stage_name = sjis_enc.GetString(br.ReadBytes(len))
    End Sub

    Private Sub ReadStageSector(br As BinaryReader, ByRef stage_length As Integer, ByRef bg_path As String, ByRef first_bgm As Integer,
                                ByRef bgm_path_list As String(), ByRef enemy_list As Generic.List(Of EnemyData),
                                ByRef shot_list As Generic.List(Of ShotData), ByRef global_script As String)
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        Dim bgm_num As Integer
        Dim enemy_num As Integer
        Dim shot_num As Integer
        Dim len As Integer
        stage_length = BytesToInt(br.ReadBytes(4))
        len = BytesToInt(br.ReadBytes(4))
        bg_path = sjis_enc.GetString(br.ReadBytes(len))
        first_bgm = BytesToInt(br.ReadBytes(4))
        bgm_num = BytesToInt(br.ReadBytes(4))
        ReDim bgm_path_list(bgm_num - 1)
        For i = 0 To bgm_num - 1
            len = BytesToInt(br.ReadBytes(4))
            bgm_path_list(i) = sjis_enc.GetString(br.ReadBytes(len))
        Next
        Dim a = br.ReadBytes(12)    'Debug area
        enemy_num = BytesToInt(br.ReadBytes(4))
        For i = 0 To enemy_num - 1
            enemy_list.Add(ReadEnemyData(br))
        Next
        shot_num = BytesToInt(br.ReadBytes(4))
        For i = 0 To shot_num - 1
            shot_list.Add(ReadShotData(br))
        Next
        len = BytesToInt(br.ReadBytes(4))
        global_script = sjis_enc.GetString(br.ReadBytes(len))
    End Sub

    Private Function ReadEnemyData(br As BinaryReader) As EnemyData
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        Dim len As Integer
        len = BytesToInt(br.ReadBytes(4))
        Dim image_path As String = sjis_enc.GetString(br.ReadBytes(len))
        Dim spawn_x As Integer = BytesToInt(br.ReadBytes(4))
        Dim spawn_y As Integer = BytesToInt(br.ReadBytes(4))
        Dim spawn_time As Integer = BytesToInt(br.ReadBytes(4))
        Dim spawn_manual As Boolean = False
        If (spawn_time = 999999) Then
            spawn_time = 0
            spawn_manual = True
        End If
        Dim category As Byte = br.ReadByte()
        Dim type As Integer = BytesToInt(br.ReadBytes(4))
        Dim hp As Integer = BytesToInt(br.ReadBytes(4))
        len = BytesToInt(br.ReadBytes(4))
        Dim script As String = sjis_enc.GetString(br.ReadBytes(len))
        Return New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, category, type, hp, script)
    End Function

    Private Function ReadShotData(br As BinaryReader) As ShotData
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        Dim action_type As Integer = BytesToInt(br.ReadBytes(4))
        Dim len = BytesToInt(br.ReadBytes(4))
        Dim script As String = sjis_enc.GetString(br.ReadBytes(len))
        Return New ShotData(action_type, script)
    End Function

    Private Function BytesToInt(bytes As Byte()) As Integer
        Dim temp(3) As Integer
        temp(0) = bytes(0) * &H1000000
        temp(1) = bytes(1) * &H10000
        temp(2) = bytes(2) * &H100
        temp(3) = bytes(3)
        Return temp(0) + temp(1) + temp(2) + temp(3)
    End Function

End Class
