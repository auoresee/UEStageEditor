Imports System.IO
Imports System.Security.Permissions

Public Class FormMain
    Public WithEvents PanelPreview As DoubleBufferingPanel
    Public Const enemy_size As Integer = 32
    Public current_file_name As String = Nothing
    Public enemy_list As New Generic.List(Of EnemyData)
    Public shot_list As New Generic.List(Of ShotData)
    Dim moving As Integer = -1
    Dim mousedown_x As Integer = -1
    Dim mousedown_y As Integer = -1
    Dim cause As Integer = 0
    Const CAUSE_NORMAL = 0
    Const CAUSE_PROGRAM = 1

    Public Property CurEnemyIndex() As Integer
        Get
            Return cur_enemy_index
        End Get
        Set(value As Integer)
            cur_enemy_index = value
            If (value >= 0) Then
                SelectedEnemyTextBox.Text = value
            Else
                SelectedEnemyTextBox.Text = ""
                RadioButton1.Checked = True
            End If
        End Set
    End Property
    Private cur_enemy_index As Integer = -1
    Public Property CopySrcEnemyIndex() As Integer
        Get
            Return copy_src_enemy_index
        End Get
        Set(value As Integer)
            copy_src_enemy_index = value
            If (value >= 0) Then
                NumericUpDown1.Value = value
            Else
                NumericUpDown1.Value = 0
                RadioButton1.Checked = True
            End If
        End Set
    End Property
    Private copy_src_enemy_index As Integer = -1
    Private bg_path As String
    Private bg_image As Image
    Public enemy_pictures As New Generic.List(Of Image)
    Const MenuBarYSize As Integer = 30
    Dim stage_length As Integer

    Dim file_name As String = ""
    Private starting_path As String
    Private ueshooting_path As String = "..\ueshooting"
    'Dim enemy_list As New Generic.List(Of EnemyData) 重複

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        starting_path = IO.Directory.GetCurrentDirectory()
        EnemyList.View = View.Details
        EnemyList.FullRowSelect = True
        EnemyList.Columns.Add("ID", 25, HorizontalAlignment.Left)
        EnemyList.Columns.Add("出現フレーム", 80, HorizontalAlignment.Left)
        ShotList.View = View.Details
        ShotList.FullRowSelect = True
        ShotList.Columns.Add("動作タイプ", 65, HorizontalAlignment.Left)

        SetStageData(StageTimeLengthUpDown.Value, BackgroundPathBox.Text)
        SetEnemyData(enemy_list, -1)

        PanelPreview = New DoubleBufferingPanel()
        PanelPreview.Location = New Point(5, 30)
        PanelPreview.Size = New Size(300, 350)
        Controls.Add(PanelPreview)
        NewEnemyCategoryBox.SelectedIndex = 0
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        LoadSettings()
        LoadPictures()
    End Sub

    Public Sub AddEnemy(copy_src_index As Integer, x As Integer, y As Integer, time As Integer)
        Dim image_path = enemy_list(copy_src_index).image_path
        Dim spawn_x As Integer = x
        Dim spawn_y As Integer = y
        Dim spawn_time As Integer = time
        Dim spawn_manual As Boolean = False
        Dim enemy_category As Byte = enemy_list(copy_src_index).category
        Dim enemy_type As Integer = enemy_list(copy_src_index).type
        Dim hp As Integer = enemy_list(copy_src_index).hp
        Dim script As String = EnemyScriptTextBox.Text
        Dim cur_enemy As EnemyData = New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, enemy_category, enemy_type, hp, script)
        AddEnemyToList(cur_enemy)
    End Sub

    Public Sub AddEnemy(x As Integer, y As Integer, time As Integer)
        Dim image_path = ""
        Dim spawn_x As Integer = x
        Dim spawn_y As Integer = y
        Dim spawn_time As Integer = time
        Dim spawn_manual As Boolean = False
        Dim enemy_category As Byte = 0
        Dim enemy_type As Integer = 0
        Dim hp As Integer = 1
        Dim cur_enemy As EnemyData = New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, enemy_category, enemy_type, hp, "")
        AddEnemyToList(cur_enemy)
    End Sub

    Public Sub AddEnemy(x As Integer, y As Integer, time As Integer, category As Integer, type As Integer)
        Dim image_path = ""
        Dim spawn_x As Integer = x
        Dim spawn_y As Integer = y
        Dim spawn_time As Integer = time
        Dim spawn_manual As Boolean = False
        Dim enemy_category As Byte = category
        Dim enemy_type As Integer = type
        Dim hp As Integer = 1
        Dim cur_enemy As EnemyData = New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, enemy_category, enemy_type, hp, "")
        AddEnemyToList(cur_enemy)
    End Sub

    Public Sub SelectEnemy(enemy_index As Integer)
        EnemyList.Items(enemy_index).Selected = True
        EnemyList.Select()
        cur_enemy_index = enemy_index
    End Sub

    Private Sub AddEnemyToList(enemy As EnemyData)
        EnemyList_InsertCorrectTime(enemy, StageTimeLengthUpDown.Value)
        EnemyList_SetList(enemy_list)
        'If (form_preview IsNot Nothing) Then
        'form_preview.Refresh()
        PanelPreview.Refresh()
        'End If
    End Sub

    Private Sub AddShotToList(shot As ShotData)
        If shot_list.Any(Function(s) s.action_type = shot.action_type) Then
            MessageBox.Show("動作タイプが重複しています",
                "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation)
            Return
        End If
        shot_list.Add(shot)
        shot_list.Sort()
        ShotList_SetList(shot_list)
    End Sub

    Private Sub EnemyAddButton_Click(sender As Object, e As EventArgs) Handles EnemyAddButton.Click
        Dim cur_enemy As EnemyData
        Dim image_path = EnemyImagePathBox.Text
        Dim spawn_x As Integer = EnemySpawnXUpDown.Value
        Dim spawn_y As Integer = EnemySpawnYUpDown.Value
        Dim spawn_time As Integer = EnemySpawnTimeUpDown.Value
        Dim spawn_manual As Boolean = EnemySpawnManualCheckBox.Checked
        Dim enemy_category As Byte
        If (EnemyCategoryBox.SelectedIndex < 0 OrElse EnemyCategoryBox.SelectedIndex > 2) Then
            enemy_category = 0
        Else
            enemy_category = EnemyCategoryBox.SelectedIndex
        End If
        Dim enemy_type As Integer = EnemyTypeUpDown.Value
        Dim hp As Integer = EnemyHPUpDown.Value
        Dim script As String = EnemyScriptTextBox.Text
        cur_enemy = New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, enemy_category, enemy_type, hp, script)
        'cur_action_list = New Generic.List(Of EnemyAction)
        CurEnemyIndex = enemy_list.Count
        If spawn_time <> -1 Then
            FrameTrackBar.Value = spawn_time    'こちらを変更するとFrameUpDownも変更される
        End If
        AddEnemyToList(cur_enemy)
    End Sub

    Private Sub EnemyOverrideButton_Click(sender As Object, e As EventArgs) Handles EnemyOverrideButton.Click
        Dim cur_enemy As EnemyData
        Dim image_path = EnemyImagePathBox.Text
        Dim spawn_x As Integer = EnemySpawnXUpDown.Value
        Dim spawn_y As Integer = EnemySpawnYUpDown.Value
        Dim spawn_time As Integer = EnemySpawnTimeUpDown.Value
        Dim spawn_manual As Boolean = EnemySpawnManualCheckBox.Checked
        Dim enemy_category As Byte
        If (EnemyCategoryBox.SelectedIndex < 0 OrElse EnemyCategoryBox.SelectedIndex > 2) Then
            enemy_category = 0
        Else
            enemy_category = EnemyCategoryBox.SelectedIndex
        End If
        Dim enemy_type As Integer = EnemyTypeUpDown.Value
        Dim hp As Integer = EnemyHPUpDown.Value
        Dim script As String = EnemyScriptTextBox.Text
        cur_enemy = New EnemyData(image_path, spawn_x, spawn_y, spawn_time, spawn_manual, enemy_category, enemy_type, hp, script)
        If (EnemyList.SelectedItems.Count <= 0) Then
            CurEnemyIndex = enemy_list.Count
            FrameTrackBar.Value = spawn_time    'こちらを変更するとFrameUpDownも変更される
            AddEnemyToList(cur_enemy)
        Else
            Dim index = EnemyList.SelectedItems(0).Index
            enemy_list(index) = cur_enemy
            EnemyList_SetList(enemy_list)
            FrameTrackBar.Value = spawn_time    'こちらを変更するとFrameUpDownも変更される
            PanelPreview.Refresh()
        End If
    End Sub

    Private Sub ShotAddButton_Click(sender As Object, e As EventArgs) Handles ShotAddButton.Click
        Dim cur_shot As ShotData
        Dim action_type As Integer = ShotActionTypeUpDown.Value()
        Dim script As String = ShotScriptTextBox.Text
        cur_shot = New ShotData(action_type, script)
        'cur_action_list = New Generic.List(Of EnemyAction)
        AddShotToList(cur_shot)
        ShotActionTypeUpDown.Value += 1
    End Sub

    Private Sub ShotOverrideButton_Click(sender As Object, e As EventArgs) Handles ShotOverrideButton.Click
        If (ShotList.SelectedItems.Count <= 0) Then
            Return
        End If
        Dim cur_shot As ShotData
        Dim action_type As Integer = ShotActionTypeUpDown.Value()
        Dim script As String = ShotScriptTextBox.Text
        cur_shot = New ShotData(action_type, script)
        ListOverrideShot(cur_shot)
    End Sub

    Public Function DeepCopyAll(Of T As ICloneable)(list As List(Of T))
        Dim ret As New List(Of T)
        For i = 0 To list.Count - 1
            ret.Add(list(i).Clone())
        Next
        Return ret
    End Function

    Private Sub EnemyList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EnemyList.SelectedIndexChanged
        If (cause = CAUSE_PROGRAM) Then Return
        If (EnemyList.SelectedItems.Count <= 0) Then Return
        Dim index As Integer = EnemyList.SelectedItems(0).Index
        SetSelectedEnemyInfo(index)
    End Sub

    Private Sub SetSelectedEnemyInfo(index As Integer)
        CurEnemyIndex = index
        EnemyImagePathBox.Text = enemy_list(index).image_path
        EnemySpawnXUpDown.Value = enemy_list(index).spawn_x
        EnemySpawnYUpDown.Value = enemy_list(index).spawn_y
        cause = CAUSE_PROGRAM
        EnemySpawnTimeUpDown.Value = enemy_list(index).spawn_time
        EnemySpawnManualCheckBox.Checked = enemy_list(index).spawn_manual
        If (EnemySpawnManualCheckBox.Checked) Then
            EnemySpawnTimeUpDown.Enabled = False
        End If
        cause = CAUSE_NORMAL
        EnemyCategoryBox.SelectedIndex = enemy_list(index).category
        EnemyTypeUpDown.Value = enemy_list(index).type
        EnemyHPUpDown.Value = enemy_list(index).hp
        EnemyScriptTextBox.Text = enemy_list.Item(index).script
        PanelPreview.Refresh()
    End Sub

    Private Sub EnemyList_SetList(enemies As List(Of EnemyData))
        Dim temp1() As String
        EnemyList.Items.Clear()
        If (enemies.Count < 1) Then
            'If (form_preview IsNot Nothing) Then
            'form_preview.SetEnemyData(enemies, -1)
            'End If
            Return
        End If

        For i = 0 To enemies.Count - 1
            temp1 = {i.ToString, enemies.Item(i).spawn_time.ToString}
            EnemyList.Items.Add(New ListViewItem(temp1))
        Next

        'If (form_preview IsNot Nothing) Then
        'form_preview.SetEnemyData(enemies, -1)
        SetEnemyData(enemies, CurEnemyIndex)   '互換性のため
        'End If
    End Sub

    Private Sub ShotList_SetList(shots As List(Of ShotData))
        Dim temp1() As String

        ShotList.Items.Clear()
        If (shots.Count < 1) Then
            'If (form_preview IsNot Nothing) Then
            'form_preview.SetEnemyData(enemies, -1)
            'End If
            Return
        End If

        For i = 0 To shots.Count - 1
            temp1 = {shots(i).action_type.ToString}
            ShotList.Items.Add(New ListViewItem(temp1))
        Next
    End Sub

    Private Sub EnemyList_RemoveAt(ByVal index As Integer)
        If (index < 0 OrElse index >= EnemyList.Items.Count) Then
            Return
        End If
        'If (index = form_preview.CurEnemyIndex) Then
        'form_preview.CurEnemyIndex = -1
        'End If
        'If (index = form_preview.CopySrcEnemyIndex) Then
        'form_preview.CopySrcEnemyIndex = -1
        'End If
        If (index = CurEnemyIndex) Then
            CurEnemyIndex = -1
        End If
        If (index = CopySrcEnemyIndex) Then
            CopySrcEnemyIndex = -1
        End If
        EnemyList.Items.RemoveAt(index)
    End Sub

    Private Sub ShotList_RemoveAt(ByVal index As Integer)
        If (index < 0 OrElse index >= ShotList.Items.Count) Then
            Return
        End If
        ShotList.Items.RemoveAt(index)
    End Sub

    Private Sub EnemyBox_Clear()
        EnemyImagePathBox.Text = ""
        EnemySpawnXUpDown.Value = 0
        EnemySpawnYUpDown.Value = 0
        EnemySpawnTimeUpDown.Value = 0
        NewEnemyCategoryBox.SelectedIndex = 0
        NewEnemyTypeUpDown.Value = 0
    End Sub

    Private Sub EnemyRemoveButton_Click(sender As Object, e As EventArgs) Handles EnemyRemoveButton.Click
        If (EnemyList.SelectedItems.Count <= 0) Then
            Return
        End If
        enemy_list.RemoveAt(EnemyList.SelectedItems(0).Index)
        EnemyList_RemoveAt(EnemyList.SelectedItems(0).Index)
        EnemyList_SetList(enemy_list)
        PanelPreview.Refresh()
    End Sub

    Private Sub ShotRemoveButton_Click(sender As Object, e As EventArgs) Handles ShotRemoveButton.Click
        If (ShotList.SelectedItems.Count <= 0) Then
            Return
        End If
        shot_list.RemoveAt(ShotList.SelectedItems(0).Index)
        ShotList_RemoveAt(ShotList.SelectedItems(0).Index)
        ShotList_SetList(shot_list)
    End Sub

    Private Sub EnemyRemoveAllButton_Click(sender As Object, e As EventArgs) Handles EnemyRemoveAllButton.Click
        Dim result As DialogResult
        result = MessageBox.Show("リストを全て消去しますか？",
                        "StageEditor",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation)
        If (result = DialogResult.Yes) Then
            EnemyList.Items.Clear()
            enemy_list.Clear()
            'If (form_preview IsNot Nothing) Then
            'form_preview.CurEnemyIndex = -1
            'form_preview.CopySrcEnemyIndex = -1
            'form_preview.SetEnemyData(enemy_list, -1)
            PanelPreview.Refresh()
            CurEnemyIndex = -1
            CopySrcEnemyIndex = -1
            SetEnemyData(enemy_list, -1)
            'End If
        End If
    End Sub

    Private Sub ShotRemoveAllButton_Click(sender As Object, e As EventArgs) Handles ShotRemoveAllButton.Click
        Dim result As DialogResult
        result = MessageBox.Show("リストを全て消去しますか？",
                        "StageEditor",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation)
        If (result = DialogResult.Yes) Then
            ShotList.Items.Clear()
            shot_list.Clear()
        End If
    End Sub

    Private Sub EnemyScriptRemoveAllButton_Click(sender As Object, e As EventArgs) Handles EnemyScriptDeleteButton.Click
        If (MessageBox.Show("スクリプトを削除しますか？",
                "削除の確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation).Equals(DialogResult.Yes)) Then
            EnemyScriptTextBox.Text = ""
        End If
    End Sub

    Private Sub ShotScriptRemoveAllButton_Click(sender As Object, e As EventArgs) Handles ShotScriptDeleteButton.Click
        If (MessageBox.Show("スクリプトを削除しますか？",
                "削除の確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation).Equals(DialogResult.Yes)) Then
            ShotScriptTextBox.Text = ""
        End If
    End Sub

    Private Sub GlobalScriptRemoveAllButton_Click(sender As Object, e As EventArgs) Handles GlobalScriptDeleteButton.Click
        If (MessageBox.Show("スクリプトを削除しますか？",
                "削除の確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation).Equals(DialogResult.Yes)) Then
            GlobalScriptTextBox.Text = ""
        End If
    End Sub

    Private Sub ToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem3.Click
        Dim sr As StageReader = New StageReader
        Dim temp_name As String
        Dim ofd As New OpenFileDialog()
        ofd.Filter = "データ ファイル(*.dat)|*.dat"
        ofd.Title = "開く"
        ofd.RestoreDirectory = True
        If ofd.ShowDialog() = DialogResult.Cancel Then
            Return
        End If
        temp_name = ofd.FileName

        Try
            sr.LoadStage(temp_name, StageNameBox.Text, StageTimeLengthUpDown.Value, BackgroundPathBox.Text, FirstBGMUpDown.Value,
                         UsingBGMPathBox.Lines, enemy_list, shot_list, GlobalScriptTextBox.Text)
        Catch exception As Exception
            MessageBox.Show("ファイルの読み込みに失敗しました。")
        End Try
        EnemyList_SetList(enemy_list)
        EnemyList_Sort()
        shot_list.Sort()
        ShotList_SetList(shot_list)
    End Sub

    Private Sub SaveNewMenuItem_Click(sender As Object, e As EventArgs) Handles SaveNewMenuItem.Click
        Dim temp_name As String
        Dim sfd As New SaveFileDialog()
        sfd.Filter = "データ ファイル(*.dat)|*.dat"
        sfd.Title = "名前を付けて保存"
        sfd.RestoreDirectory = True
        If sfd.ShowDialog() = DialogResult.Cancel Then
            Return
        End If
        temp_name = sfd.FileName

        Dim sw As StageWriter = New StageWriter
        Try
            sw.SaveStage(temp_name, StageNameBox.Text, StageTimeLengthUpDown.Value, BackgroundPathBox.Text, FirstBGMUpDown.Value,
                         UsingBGMPathBox.Lines, 0, False, enemy_list, shot_list, GlobalScriptTextBox.Text)
            current_file_name = temp_name
        Catch exception As Exception
            MessageBox.Show("ファイルの保存に失敗しました。")
        End Try
    End Sub

    Private Function EnemyList_InsertCorrectTime(ByVal enemy As EnemyData, stage_len As Integer) As Integer
        If (enemy.spawn_time >= stage_len) Then
            enemy.spawn_time = stage_len - 1
        End If

        For i = 0 To enemy_list.Count - 1
            If (enemy.spawn_time < enemy_list(i).spawn_time) Then
                enemy_list.Insert(i, enemy)
                'If (i = form_preview.CurEnemyIndex) Then
                'form_preview.CurEnemyIndex = form_preview.CurEnemyIndex + 1
                'End If
                'If (i = form_preview.CopySrcEnemyIndex) Then
                'form_preview.CopySrcEnemyIndex = form_preview.CopySrcEnemyIndex + 1
                'End If
                If (i = CurEnemyIndex) Then
                    CurEnemyIndex = CurEnemyIndex + 1
                End If
                If (i = CopySrcEnemyIndex) Then
                    CopySrcEnemyIndex = CopySrcEnemyIndex + 1
                End If
                Return i
            End If
        Next

        enemy_list.Add(enemy)
        Return enemy_list.Count - 1
    End Function

    'Private Sub PreviewToolStripMenuItem_Click(sender As Object, e As EventArgs)
    'If form_preview Is Nothing Then
    'form_preview = New FormPreview
    'form_preview.Opener = Me
    'form_preview.SetStageData(StageTimeLengthUpDown.Value, BGPathBox.Text)
    'form_preview.SetEnemyData(enemy_list, -1)
    'SetStageData(StageTimeLengthUpDown.Value, BGPathBox.Text)
    'SetEnemyData(enemy_list, -1)
    'End If
    'form_preview.Show()
    'End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If (CheckBox1.Checked) Then
            EnemyImagePathBox.Enabled = False
        Else
            EnemyImagePathBox.Enabled = True
        End If
    End Sub

    Private Sub StageTimeLengthUpDown_ValueChanged(sender As Object, e As EventArgs) Handles StageTimeLengthUpDown.ValueChanged
        'If (form_preview IsNot Nothing) Then
        'form_preview.SetStageLength(StageTimeLengthUpDown.Value)
        SetStageLength(StageTimeLengthUpDown.Value)
        'End If
    End Sub

    Private Sub EnemyList_Sort()
        enemy_list = enemy_list.OrderBy(Function(n) n.spawn_time).ToList()
    End Sub

    Private Sub BackgroundPathBox_Leave(sender As Object, e As EventArgs) Handles BackgroundPathBox.Leave
        'If (form_preview IsNot Nothing) Then
        'form_preview.LoadBG(BGPathBox.Text)
        'form_preview.Refresh()
        LoadBackground(BackgroundPathBox.Text)
        Refresh()
        'End If
    End Sub

    Private Sub BackgroundReferenceButton_Click(sender As Object, e As EventArgs) Handles BackgroundReferenceButton.Click
        OpenFileDialog1.InitialDirectory = ueshooting_path + "\picture"
        If (OpenFileDialog1.ShowDialog() <> DialogResult.OK) Then Return
        OpenFileDialog1.FileName = OpenFileDialog1.FileName.Replace(ueshooting_path + "\", "")
        BackgroundPathBox.Text = OpenFileDialog1.FileName
        'If (form_preview IsNot Nothing) Then
        'form_preview.LoadBG(BGPathBox.Text)
        'form_preview.Refresh()
        LoadBackground(ueshooting_path + "\" + BackgroundPathBox.Text)
        Refresh()
        'End If
    End Sub

    Public Sub SetStageLength(p1 As Integer)
        stage_length = p1
        FrameTrackBar.Maximum = stage_length
        FrameUpDown.Maximum = stage_length
    End Sub

    Public Sub SetStageData(p1 As Integer, p2 As String)
        stage_length = p1
        FrameTrackBar.Maximum = stage_length
        FrameUpDown.Maximum = stage_length
        bg_path = p2
    End Sub

    Public Sub SetEnemyData(p1 As Generic.List(Of EnemyData), p2 As Integer)
        enemy_list = p1
        If (cur_enemy_index >= 0) Then
            cur_enemy_index = p2
        End If
        If (enemy_list.Count > 0) Then
            NumericUpDown1.Maximum = enemy_list.Count - 1
            If (NumericUpDown1.Value > NumericUpDown1.Maximum) Then
                NumericUpDown1.Value = NumericUpDown1.Maximum
                copy_src_enemy_index = NumericUpDown1.Value
            End If
        Else
            NumericUpDown1.Maximum = 0
            NumericUpDown1.Value = 0
            copy_src_enemy_index = -1
            RadioButton1.Checked = True
        End If
    End Sub

    'Public Sub SelectEnemy(p1 As Integer)
    '    If (cur_enemy_index >= 0) Then
    '        cur_enemy_index = p1
    '    End If
    'End Sub

    '<SecurityPermission(SecurityAction.Demand, _
    'Flags:=SecurityPermissionFlag.UnmanagedCode)> _
    'Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    '    Const WM_SYSCOMMAND As Integer = &H112
    '    Const SC_CLOSE As Long = &HF060L

    '    If m.Msg = WM_SYSCOMMAND AndAlso _
    '        (m.WParam.ToInt64() And &HFFF0L) = SC_CLOSE Then
    '        Me.Hide()
    '        Return
    '    End If

    '    MyBase.WndProc(m)
    'End Sub

    Private Sub PanelPreview_Click(sender As Object, e As MouseEventArgs) Handles PanelPreview.Click
        'If (e.X > 0 AndAlso e.X < 640 AndAlso e.Y > MenuBarYSize AndAlso e.Y < 480 + MenuBarYSize) Then
        PreviewScreenClicked(e.X, e.Y)
        'End If
    End Sub

    Private Sub PreviewScreenClicked(p_x As Integer, p_y As Integer)
        Dim x As Integer = CnvToRealXPos(p_x)
        Dim y As Integer = CnvToRealYPos(p_y)
        Dim nearest_enemy As Integer
        If (x <> mousedown_x OrElse y <> mousedown_y) Then
            Return
        End If

        nearest_enemy = GetMouseSelectedEnemy(p_x, p_y)

        If (nearest_enemy <> -1) Then
            cur_enemy_index = nearest_enemy
            SelectedEnemyTextBox.Text = nearest_enemy
            'Dim form1 As Form1 = DirectCast(Opener, Form1)
            'form1.SelectEnemy(nearest_enemy)
            'form1.Activate()
            SelectEnemy(nearest_enemy)
            SetSelectedEnemyInfo(nearest_enemy)
            Activate()
        Else
            cur_enemy_index = -1
            SelectedEnemyTextBox.Text = ""
        End If
    End Sub

    Private Function GetMouseSelectedEnemy(p_x As Integer, p_y As Integer) As Integer
        Dim x As Integer = CnvToRealXPos(p_x)
        Dim y As Integer = CnvToRealYPos(p_y)
        Dim nearest_enemy As Integer = -1
        Dim nearest_enemy_distance As Double = 100
        Dim cur_enemy As EnemyData

        If (enemy_list Is Nothing) Then Return -1
        For i = 0 To enemy_list.Count - 1
            cur_enemy = enemy_list(i)
            If (cur_enemy.spawn_time > FrameUpDown.Value) Then
                Continue For
            End If
            If (cur_enemy.spawn_x > x - enemy_size / 2 AndAlso cur_enemy.spawn_y > y - enemy_size / 2 AndAlso
                cur_enemy.spawn_x < x + enemy_size / 2 AndAlso cur_enemy.spawn_y < y + enemy_size / 2) Then
                Dim cur_enemy_distance As Integer = GetDistance(cur_enemy.spawn_x, cur_enemy.spawn_y, x, y)
                If (cur_enemy_distance < nearest_enemy_distance) Then
                    nearest_enemy = i
                    nearest_enemy_distance = cur_enemy_distance
                End If
            End If
        Next

        Return nearest_enemy
    End Function

    Private Function GetDistance(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Integer
        Return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))
    End Function

    Private Sub PanelPreview_DoubleClick(sender As Object, e As MouseEventArgs) Handles PanelPreview.DoubleClick
        If (CurEnemyIndex < 0) Then
            CurEnemyIndex = enemy_list.Count
        End If
        If (RadioButton1.Checked) Then
            'DirectCast(Opener, Form1).AddEnemy(e.X, e.Y, FrameUpDown.Value, EnemyCategoryBox.SelectedIndex, EnemyTypeUpDown.Value)
            AddEnemy(CnvToRealXPos(e.X), CnvToRealYPos(e.Y), FrameUpDown.Value, NewEnemyCategoryBox.SelectedIndex, NewEnemyTypeUpDown.Value)
        Else
            'DirectCast(Opener, Form1).AddEnemy(NumericUpDown1.Value, e.X, e.Y, FrameUpDown.Value)
            AddEnemy(NumericUpDown1.Value, CnvToRealXPos(e.X), CnvToRealYPos(e.Y), FrameUpDown.Value)
        End If
    End Sub

    Private Sub PanelPreview_MouseDown(sender As Object, e As MouseEventArgs) Handles PanelPreview.MouseDown
        Dim temp As Integer = GetMouseSelectedEnemy(e.X, e.Y)
        If (temp <> -1) Then
            moving = temp
        End If
        mousedown_x = CnvToRealXPos(e.X)
        mousedown_y = CnvToRealYPos(e.Y)
    End Sub

    Private Sub PanelPreview_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelPreview.MouseMove
        If (moving <> -1) Then
            If (CnvToRealXPos(e.X) < -120) Then
                enemy_list(moving).spawn_x = -120
            ElseIf (CnvToRealXPos(e.X) > 504) Then
                enemy_list(moving).spawn_x = 504
            Else
                enemy_list(moving).spawn_x = CnvToRealXPos(e.X)
            End If
            If (CnvToRealYPos(e.Y) < -90) Then
                enemy_list(moving).spawn_y = -90
            ElseIf (CnvToRealYPos(e.Y) > 570) Then
                enemy_list(moving).spawn_y = 570
            Else
                enemy_list(moving).spawn_y = CnvToRealYPos(e.Y)
            End If

            PanelPreview.Refresh()
        End If
    End Sub

    Private Sub PanelPreview_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelPreview.MouseUp
        If (moving <> -1) Then
            If (EnemyList.SelectedItems.Count > 0 AndAlso moving = EnemyList.SelectedItems(0).Index) Then
                SetSelectedEnemyInfo(EnemyList.SelectedItems(0).Index)
            End If
            moving = -1
        End If
    End Sub

    Private Sub PanelPreview_MouseLeave(sender As Object, e As EventArgs) Handles PanelPreview.MouseLeave
        If (moving <> -1) Then
            If (EnemyList.SelectedItems.Count > 0 AndAlso moving = EnemyList.SelectedItems(0).Index) Then
                SetSelectedEnemyInfo(EnemyList.SelectedItems(0).Index)
            End If
            moving = -1
        End If
    End Sub

    Private Sub FrameTrackBar_Scroll(sender As Object, e As EventArgs) Handles FrameTrackBar.Scroll
        FrameUpDown.Value = FrameTrackBar.Value
    End Sub

    Private Sub FrameTrackBar_MouseUp(sender As Object, e As EventArgs) Handles FrameTrackBar.MouseUp
        PanelPreview.Refresh()
    End Sub

    Private Sub FrameUpDown_ValueChanged(sender As Object, e As EventArgs) Handles FrameUpDown.ValueChanged
        FrameTrackBar.Value = FrameUpDown.Value
        PanelPreview.Refresh()
    End Sub

    Private Sub PanelPreview_Paint(sender As Object, e As PaintEventArgs) Handles PanelPreview.Paint
        MyBase.OnPaint(e)
        DrawBackground(e)
        If (enemy_list Is Nothing) Then
            Return
        End If
        For i = 0 To enemy_list.Count - 1
            Dim enemy = enemy_list(i)
            If (enemy.spawn_time > FrameUpDown.Value) Then
                Continue For
            End If

            If (enemy.spawn_time = FrameUpDown.Value) Then
                DrawEnemy(enemy, e, CurEnemyIndex = i, False)
            Else
                DrawEnemy(enemy, e, CurEnemyIndex = i, True)
            End If
        Next
    End Sub

    Private Sub DrawBackground(e As PaintEventArgs)
        If (bg_image IsNot Nothing) Then
            e.Graphics.DrawImage(bg_image, CnvToPanelXPos(0), CnvToPanelYPos(0),
                                 CnvToPanelLength(384), CnvToPanelLength(448))
        End If
        e.Graphics.DrawRectangle(Pens.Red, New Rectangle(CnvToPanelXPos(0), CnvToPanelYPos(0),
                                                         CnvToPanelLength(384), CnvToPanelLength(448)))
    End Sub

    Private Sub DrawEnemy(enemy As EnemyData, e As PaintEventArgs, focused As Boolean, translucent As Boolean)
        If (translucent) Then
            'ColorMatrixオブジェクトの作成
            Dim cm As New System.Drawing.Imaging.ColorMatrix()
            'ColorMatrixの行列の値を変更して、アルファ値が0.5に変更されるようにする
            cm.Matrix00 = 1
            cm.Matrix11 = 1
            cm.Matrix22 = 1
            cm.Matrix33 = 0.3F
            cm.Matrix44 = 1

            'ImageAttributesオブジェクトの作成
            Dim ia As New System.Drawing.Imaging.ImageAttributes()
            'ColorMatrixを設定する
            ia.SetColorMatrix(cm)
            e.Graphics.DrawImage(enemy_pictures(enemy.type), New Rectangle(CnvToPanelXPos(enemy.spawn_x - enemy_size / 2), CnvToPanelYPos(enemy.spawn_y - enemy_size / 2),
                                 CnvToPanelLength(enemy_size), CnvToPanelLength(enemy_size)), 0, 0, 32, 32, GraphicsUnit.Pixel, ia)
        Else
            Dim enemytype As Integer = enemy.type
            If (enemytype >= enemy_pictures.Count) Then
                enemytype = 0
            End If
            e.Graphics.DrawImage(enemy_pictures(enemytype), CnvToPanelXPos(enemy.spawn_x - enemy_size / 2), CnvToPanelYPos(enemy.spawn_y - enemy_size / 2),
                                 CnvToPanelLength(enemy_size), CnvToPanelLength(enemy_size))
        End If
            If (focused) Then
            e.Graphics.DrawRectangle(Pens.Red, New Rectangle(CnvToPanelXPos(enemy.spawn_x - enemy_size / 2), CnvToPanelYPos(enemy.spawn_y - enemy_size / 2),
                                                             CnvToPanelLength(enemy_size), CnvToPanelLength(enemy_size)))
        End If
    End Sub

    Private Sub LoadPictures()
        Directory.SetCurrentDirectory(starting_path)
        Try
            LoadBackground()
        Catch
        End Try
        Try
            enemy_pictures.Add(Image.FromFile("pictures\\mob\\mob_0.gif"))
            enemy_pictures.Add(Image.FromFile("pictures\\mob\\mob_1.gif"))
            enemy_pictures.Add(Image.FromFile("pictures\\mob\\mob_2.gif"))
            enemy_pictures.Add(Image.FromFile("pictures\\mob\\mob_3.gif"))
        Catch
        End Try
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If (RadioButton1.Checked) Then
            Label1.Enabled = True
            Label2.Enabled = True
            NewEnemyCategoryBox.Enabled = True
            NewEnemyTypeUpDown.Enabled = True
        Else
            Label1.Enabled = False
            Label2.Enabled = False
            NewEnemyCategoryBox.Enabled = False
            NewEnemyTypeUpDown.Enabled = False
        End If
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If (RadioButton2.Checked) Then
            If (enemy_list Is Nothing OrElse enemy_list.Count = 0) Then
                MessageBox.Show("敵データが1つもありません。", _
                "StageEditor", _
                MessageBoxButtons.OK, _
                MessageBoxIcon.Exclamation)
                RadioButton1.Checked = True
                Return
            End If
            Label3.Enabled = True
            NumericUpDown1.Enabled = True
        Else
            Label3.Enabled = False
            NumericUpDown1.Enabled = False
        End If
    End Sub

    Public Sub LoadBackground()
        If (bg_path = "") Then
            Return
        End If
        If (Not System.IO.File.Exists(bg_path)) Then
            Return
        End If
        bg_image = Image.FromFile(bg_path)
        'PanelPreview.BackgroundImage = bg_image
    End Sub

    Public Sub LoadBackground(filename As String)
        If (filename = "") Then
            Return
        End If
        If (filename = bg_path) Then
            Return
        End If
        If (Not System.IO.File.Exists(filename)) Then
            Return
        End If
        bg_path = filename
        bg_image = Image.FromFile(filename)
        'PanelPreview.BackgroundImage = bg_image
    End Sub

    'Private Sub EnemySpawnXUpDown_ValueChanged(sender As Object, e As EventArgs) Handles EnemySpawnXUpDown.ValueChanged
    '    If (cause = CAUSE_PROGRAM) Then Return
    '    If (EnemyList.SelectedItems.Count <= 0) Then Return
    '    Dim index = EnemyList.SelectedItems(0).Index
    '    enemy_list.Item(index).spawn_x = EnemySpawnXUpDown.Value
    '    PanelPreview.Refresh()
    'End Sub

    'Private Sub EnemySpawnYUpDown_ValueChanged(sender As Object, e As EventArgs) Handles EnemySpawnYUpDown.ValueChanged
    '    If (cause = CAUSE_PROGRAM) Then Return
    '    If (EnemyList.SelectedItems.Count <= 0) Then Return
    '    Dim index = EnemyList.SelectedItems(0).Index
    '    enemy_list.Item(index).spawn_y = EnemySpawnYUpDown.Value
    '    PanelPreview.Refresh()
    'End Sub

    'Private Sub EnemySpawnTimeUpDown_ValueChanged(sender As Object, e As EventArgs) Handles EnemySpawnTimeUpDown.ValueChanged
    '    If (cause = CAUSE_PROGRAM) Then Return
    '    If (EnemyList.SelectedItems.Count <= 0) Then Return
    '    Dim index = CurEnemyIndex
    '    If (index < 0) Then Return
    '    Dim enemy = enemy_list.Item(index)
    '    enemy_list.RemoveAt(index)
    '    enemy.spawn_time = EnemySpawnTimeUpDown.Value
    '    CurEnemyIndex = EnemyList_InsertCorrectTime(enemy, StageTimeLengthUpDown.Value)
    '    EnemyList_SetList(enemy_list)
    '    cause = CAUSE_PROGRAM
    '    For Each item As ListViewItem In EnemyList.SelectedItems
    '        item.Focused = False
    '        item.Selected = False
    '    Next
    '    cause = CAUSE_NORMAL
    '    EnemyList.Items(CurEnemyIndex).Focused = True
    '    EnemyList.Items(CurEnemyIndex).Selected = True
    '    PanelPreview.Refresh()
    'End Sub

    Private Sub EnemyCategoryBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles EnemyCategoryBox.SelectedIndexChanged
        If (cause = CAUSE_PROGRAM) Then Return
        If (EnemyList.SelectedItems.Count <= 0) Then Return
        Dim index = EnemyList.SelectedItems(0).Index
        enemy_list.Item(index).category = CType(EnemyCategoryBox.SelectedIndex, Byte)
    End Sub

    Private Sub EnemyTypeUpDown_ValueChanged(sender As Object, e As EventArgs) Handles EnemyTypeUpDown.ValueChanged
        If (cause = CAUSE_PROGRAM) Then Return
        If (EnemyList.SelectedItems.Count <= 0) Then Return
        Dim index = EnemyList.SelectedItems(0).Index
        enemy_list.Item(index).type = EnemyTypeUpDown.Value
        PanelPreview.Refresh()
    End Sub

    Private Sub SpawnLengthUpDown_ValueChanged(sender As Object, e As EventArgs)
        If (cause = CAUSE_PROGRAM) Then Return
        If (EnemyList.SelectedItems.Count <= 0) Then Return
        Dim index = EnemyList.SelectedItems(0).Index
    End Sub

    'Convert coordinate (returns Integer!)
    Private Function CnvToPanelXPos(p1 As Double) As Integer
        Return CType(Math.Round((p1 + 120) / 11 * 5), Integer) 'Math.Round((p1 + 72) / 11 * 5) */, Integer)
    End Function

    'Convert coordinate (returns Integer!)
    Private Function CnvToPanelYPos(p1 As Double) As Integer
        Return CType(Math.Round((p1 + 90) / 11 * 5), Integer)
    End Function

    'Convert coordinate (returns Integer!)
    Private Function CnvToPanelLength(p1 As Double) As Integer
        Return CType(Math.Round(p1 / 11 * 5), Integer)
    End Function

    'Convert coordinate (returns Double!)
    Private Function CnvToRealXPos(p1 As Double) As Double
        Return CType(Math.Round(p1 / 5 * 11 - 120), Integer)
    End Function

    'Convert coordinate (returns Double!)
    Private Function CnvToRealYPos(p1 As Double) As Double
        Return CType(Math.Round(p1 / 5 * 11 - 90), Integer)
    End Function

    'Convert coordinate (returns Double!)
    Private Function CnvToRealLength(p1 As Double) As Double
        Return CType(Math.Round(p1 / 5 * 11), Integer)
    End Function

    Private Sub 現在位置から実行ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 現在位置から実行ToolStripMenuItem.Click
        Dim sw As StageWriter = New StageWriter
        Try
            sw.SaveStage(ueshooting_path + "\stage\debug.dat", StageNameBox.Text, StageTimeLengthUpDown.Value,
                         BackgroundPathBox.Text, FirstBGMUpDown.Value, UsingBGMPathBox.Lines, FrameUpDown.Value, InvincibleToolStripMenuItem.Checked,
                         enemy_list, shot_list, GlobalScriptTextBox.Text)
        Catch Exception As Exception
            MessageBox.Show("ファイルの保存に失敗しました。")
        End Try
        Dim temp = IO.Directory.GetCurrentDirectory()
        Try
            IO.Directory.SetCurrentDirectory(ueshooting_path)
            Dim p = Process.Start("java", "-classpath bin ueshooting.main.GameMain debug.dat")
        Catch exception As Exception
            MessageBox.Show("UEShootingが見つかりません。")
        End Try
        IO.Directory.SetCurrentDirectory(temp)
    End Sub

    Private Sub ステージの初めから実行ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ステージの初めから実行ToolStripMenuItem.Click
        Dim sw As StageWriter = New StageWriter
        Try
            sw.SaveStage(ueshooting_path + "\stage\debug.dat", StageNameBox.Text, StageTimeLengthUpDown.Value,
                         BackgroundPathBox.Text, FirstBGMUpDown.Value, UsingBGMPathBox.Lines, 0, InvincibleToolStripMenuItem.Checked,
                         enemy_list, shot_list, GlobalScriptTextBox.Text)
        Catch Exception As Exception
            MessageBox.Show("ファイルの保存に失敗しました。")
        End Try
        Dim temp = IO.Directory.GetCurrentDirectory()
        Try
            IO.Directory.SetCurrentDirectory(ueshooting_path)
            Dim p = Process.Start("java", "-classpath bin ueshooting.main.GameMain debug.dat")
        Catch exception As Exception
            MessageBox.Show("UEShootingが見つかりません。")
        End Try
        IO.Directory.SetCurrentDirectory(temp)
    End Sub

    Private Sub InvincibleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InvincibleToolStripMenuItem.Click
        InvincibleToolStripMenuItem.Checked = Not InvincibleToolStripMenuItem.Checked
    End Sub

    Private Sub ShotList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ShotList.SelectedIndexChanged
        If (ShotList.SelectedItems.Count <= 0) Then Return
        Dim index As Integer = ShotList.SelectedItems(0).Index
        SetSelectedShotInfo(index)
    End Sub

    Private Sub SetSelectedShotInfo(index As Integer)
        Dim shot = shot_list(index)
        ShotActionTypeUpDown.Value = shot.action_type
        ShotScriptTextBox.Text = shot.script
    End Sub

    Private Sub ListOverrideShot(cur_shot As ShotData)
        Dim index = shot_list.FindLastIndex(Function(shot) shot.action_type = cur_shot.action_type)
        If (index <> -1) Then
            shot_list(index) = cur_shot
        Else
            shot_list.Add(cur_shot)
        End If
        shot_list.Sort()
        ShotList_SetList(shot_list)
    End Sub

    Private Sub SaveOverrideMenuItem_Click(sender As Object, e As EventArgs) Handles SaveOverrideMenuItem.Click
        If (file_name IsNot Nothing) Then
            Dim sw As StageWriter = New StageWriter
            Try
                sw.SaveStage(current_file_name, StageNameBox.Text, StageTimeLengthUpDown.Value, BackgroundPathBox.Text, FirstBGMUpDown.Value,
                             UsingBGMPathBox.Lines, 0, False, enemy_list, shot_list, GlobalScriptTextBox.Text)
            Catch exception As Exception
                MessageBox.Show("ファイルの保存に失敗しました。")
            End Try
        Else
            SaveNewMenuItem_Click(sender, e)
        End If
    End Sub

    Private Sub UsingBGMReferenceButton_Click(sender As Object, e As EventArgs) Handles UsingBGMReferenceButton.Click
        OpenFileDialog2.InitialDirectory = ueshooting_path + "\music"
        If (OpenFileDialog2.ShowDialog() <> DialogResult.OK) Then Return
        OpenFileDialog2.FileName = OpenFileDialog2.FileName.Replace(ueshooting_path + "\", "")
        If (OpenFileDialog2.FileName IsNot Nothing AndAlso OpenFileDialog2.FileName.Length > 0) Then
            If (UsingBGMPathBox.Text.Length > 0) Then
                If (Not UsingBGMPathBox.Text(UsingBGMPathBox.Text.Length - 1) = Chr(&HA)) Then
                    'UsingBGMPathBox.Text = UsingBGMPathBox.Text.Insert(UsingBGMPathBox.Text.Length - 2, vbCrLf)
                    UsingBGMPathBox.Text = UsingBGMPathBox.Text + vbCrLf
                End If
                UsingBGMPathBox.Text += OpenFileDialog2.FileName
            Else
                UsingBGMPathBox.Text = OpenFileDialog2.FileName
            End If
        End If
    End Sub

    Private Sub SetPathOfUEShootingToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UEShootingの場所を設定ToolStripMenuItem.Click
        FolderBrowserDialog1.ShowDialog()
        If (FolderBrowserDialog1.SelectedPath IsNot Nothing AndAlso FolderBrowserDialog1.SelectedPath.Length > 0) Then
            ueshooting_path = FolderBrowserDialog1.SelectedPath
            IO.Directory.SetCurrentDirectory(ueshooting_path)
            SaveSettings()
        End If
    End Sub

    Private Sub LoadSettings()
        Dim s As IO.Stream
        Dim sr As IO.StreamReader

        IO.Directory.SetCurrentDirectory(starting_path)
        Try
            s = IO.File.OpenRead("setting.ini")
            sr = New IO.StreamReader(s)
        Catch ex As Exception
            MessageBox.Show("UEShootingのフォルダを設定してください")
            SetPathOfUEShootingToolStripMenuItem_Click(Nothing, Nothing)
            Return
        End Try

        ueshooting_path = sr.ReadToEnd()

        sr.Close()
        s.Close()

        IO.Directory.SetCurrentDirectory(ueshooting_path)
    End Sub

    Private Sub SaveSettings()
        Dim s As IO.Stream
        Dim sw As IO.StreamWriter

        IO.Directory.SetCurrentDirectory(starting_path)
        Try
            s = IO.File.Create("setting.ini")
            sw = New IO.StreamWriter(s)
        Catch ex As Exception
            MessageBox.Show(ex.ToString())
            Return
        End Try

        sw.Write(ueshooting_path)

        sw.Close()
        s.Close()

        IO.Directory.SetCurrentDirectory(ueshooting_path)
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles EnemySpawnManualCheckBox.CheckedChanged
        If (EnemySpawnManualCheckBox.Checked) Then
            EnemySpawnTimeUpDown.Enabled = False
        Else
            EnemySpawnTimeUpDown.Enabled = True
        End If
    End Sub
End Class