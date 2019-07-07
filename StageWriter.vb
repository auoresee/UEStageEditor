Imports System.IO
Imports System.Text

Public Class StageWriter
    Private ReadOnly SPAWN_MANUAL As Integer = 999999

    Public Sub SaveStage(file_name As String, stage_name As String, stage_length As Integer, bg_path As String, first_bgm As Integer,
                         bgm_path_list As String(), start_frame As Decimal, debug_invincible_flag As Boolean,
                         enemy_list As List(Of EnemyData), shot_list As List(Of ShotData), global_script As String)
        If (file_name = "") Then
            Dim sfd As New SaveFileDialog()
            sfd.Filter = "データ ファイル(*.dat)|*.dat"
            sfd.Title = "名前をつけて保存"
            sfd.RestoreDirectory = True
            If sfd.ShowDialog() = DialogResult.Cancel Then
                Return
            End If
            file_name = sfd.FileName
        End If

        Dim sw As Stream
        Dim bw As BinaryWriter

        Try
            sw = File.Create(file_name)
            bw = New BinaryWriter(sw)
        Catch ex As Exception
            Return
        End Try

        Dim flag As Byte
        If (debug_invincible_flag) Then
            flag = 1
        Else
            flag = 0
        End If

        WriteHeader(bw, stage_name)
        WriteStageSector(bw, stage_length, bg_path, first_bgm, bgm_path_list, CType(start_frame, Integer), flag,
                         enemy_list, shot_list, global_script)
        bw.Close()
        sw.Close()

    End Sub

    Private Sub WriteHeader(bw As BinaryWriter, stage_name As String)
        Static STAGE_FILE_SIGNATURE As Byte() = {Asc("S"c), Asc("T"c), &H80, &H9A}
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        bw.Write(STAGE_FILE_SIGNATURE)
        bw.Write(IntToBytes(sjis_enc.GetBytes(stage_name).Length))
        bw.Write(sjis_enc.GetBytes(stage_name))
    End Sub

    Private Sub WriteStageSector(bw As BinaryWriter, stage_length As Integer, bg_path As String, first_bgm As Integer,
                                 bgm_path_list As String(), start_frame As Integer,
                                 flag As Byte, enemy_list As Generic.List(Of EnemyData),
                                 shot_list As Generic.List(Of ShotData), global_script As String)
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        Dim a = bw.BaseStream.Position
        bw.Write(IntToBytes(stage_length))
        bw.Write(IntToBytes(sjis_enc.GetBytes(bg_path).Length))
        bw.Write(sjis_enc.GetBytes(bg_path))
        bw.Write(IntToBytes(first_bgm))
        bw.Write(IntToBytes(bgm_path_list.Length))
        Dim b As Byte()
        For i = 0 To bgm_path_list.Length - 1
            b = sjis_enc.GetBytes(bgm_path_list(i))
            bw.Write(IntToBytes(b.Length))
            bw.Write(b)
        Next
        bw.Write(IntToBytes(start_frame))
        bw.Write(flag)
        bw.Write(New Byte() {0, 0, 0, 0, 0, 0, 0})
        bw.Write(IntToBytes(enemy_list.Count))
        For i = 0 To enemy_list.Count - 1
            WriteEnemyData(bw, enemy_list(i))
        Next
        bw.Write(IntToBytes(shot_list.Count))
        For i = 0 To shot_list.Count - 1
            WriteShotData(bw, shot_list(i))
        Next
        bw.Write(IntToBytes(sjis_enc.GetBytes(global_script).Length))
        bw.Write(sjis_enc.GetBytes(global_script))
    End Sub

    Private Sub WriteEnemyData(bw As BinaryWriter, enemy_data As EnemyData)
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        bw.Write(IntToBytes(sjis_enc.GetBytes(enemy_data.image_path).Length))
        bw.Write(sjis_enc.GetBytes(enemy_data.image_path))
        bw.Write(IntToBytes(enemy_data.spawn_x))
        bw.Write(IntToBytes(enemy_data.spawn_y))
        If enemy_data.spawn_manual Then
            bw.Write(IntToBytes(SPAWN_MANUAL))
        Else
            bw.Write(IntToBytes(enemy_data.spawn_time))
        End If
        bw.Write(IntToBytes(enemy_data.spawn_time))
        bw.Write(enemy_data.category)
        bw.Write(IntToBytes(enemy_data.type))
        bw.Write(IntToBytes(enemy_data.hp))
        bw.Write(IntToBytes(sjis_enc.GetBytes(enemy_data.script).Length))
        bw.Write(sjis_enc.GetBytes(enemy_data.script))
    End Sub

    Private Sub WriteShotData(bw As BinaryWriter, shot_data As ShotData)
        Dim sjis_enc As Encoding = Encoding.GetEncoding("Shift_JIS")
        bw.Write(IntToBytes(shot_data.action_type))
        bw.Write(IntToBytes(sjis_enc.GetBytes(shot_data.script).Length))
        bw.Write(sjis_enc.GetBytes(shot_data.script))
    End Sub

    Private Function IntToBytes(ByVal val As Integer) As Byte()
        Dim ret(3) As Byte
        If (val < 0) Then
            ret(0) = 1
        End If
        ret(0) = CType(sr(val, 24) And &HFF, Byte)
        ret(1) = CType(sr(val, 16) And &HFF, Byte)
        ret(2) = CType(sr(val, 8) And &HFF, Byte)
        ret(3) = CType((val And &HFF), Byte)

        Return ret
    End Function

    Function sr(ByVal x, ByVal n) ' 右シフト(算術(>>)ではなく論理(>>>)シフトに相当)
        If n = 0 Then
            sr = x
        Else
            Dim y
            y = x And &H7FFFFFFF
            Dim z
            If n = 32 - 1 Then
                z = 0
            Else
                z = y \ CLng(2 ^ n)
            End If
            If y <> x Then
                z = z Or CLng(2 ^ (32 - n - 1))
            End If
            sr = z
        End If
    End Function

End Class
