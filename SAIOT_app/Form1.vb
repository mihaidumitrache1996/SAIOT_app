Imports System.IO.Ports
Imports System.Runtime.InteropServices

Public Class Form1

    Public Event ScanDataRecieved(ByVal data As String)
    WithEvents comPort As SerialPort

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        Application.Exit()
        End
    End Sub

    Public Const WM_NCLBUTTONDOWN = &HA1
    Public Const HTCAPTION = 2
    Public Declare Sub ReleaseCapture Lib "user32" ()

    Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, <MarshalAs(UnmanagedType.AsAny)> ByVal lParam As Object) As Integer

    Private Sub Me_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If e.Button = MouseButtons.Left Then
            ReleaseCapture()
            SendMessage(Me.Handle.ToInt32, WM_NCLBUTTONDOWN, HTCAPTION, 0&)
        End If
    End Sub

    Sub TextBox1_GotFocus(sender As Object, e As EventArgs) Handles TextBox1.GotFocus
        If TextBox1.Text = "Latitudine" Then TextBox1.Text = ""
        If TextBox1.Text = "Cont" Then TextBox1.Text = ""
    End Sub

    Sub TextBox1_LostFocus(sender As Object, e As EventArgs) Handles TextBox1.LostFocus
        If TextBox1.Text = "" Then
            If RadioButton1.Checked Then TextBox1.Text = "Latitudine"
            If RadioButton2.Checked Then TextBox1.Text = "Cont"
        End If
    End Sub
    Sub TextBox2_GotFocus(sender As Object, e As EventArgs) Handles TextBox2.GotFocus
        If TextBox2.Text = "Longitudine" Then TextBox2.Text = ""
    End Sub

    Sub TextBox2_LostFocus(sender As Object, e As EventArgs) Handles TextBox2.LostFocus
        If TextBox2.Text = "" Then TextBox2.Text = "Longitudine"
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text = "Latitudine" Then GoTo out
        If TextBox2.Visible AndAlso TextBox2.Text = "Longitudine" Then GoTo out
        If TextBox1.Text = "Cont" Then GoTo out
        If TextBox1.Text = "" Then GoTo out
        Dim infoString As String = TextBox1.Text
        If RadioButton1.Checked Then
            If IsNumeric(infoString) AndAlso IsNumeric(TextBox2.Text) Then
                infoString = infoString & " " & TextBox2.Text
            Else
                MsgBox("Ambele campuri trebuie sa fie valori numerice")
                GoTo out
            End If
        End If
        MsgBox(postRequest(routes.weatherForecast, "{""isWeather"": " & RadioButton1.Checked.ToString.ToLower & ", ""info"": """ & infoString & """}"))

out:
    End Sub



    Public Sub Connect()
        Try
            comPort = My.Computer.Ports.OpenSerialPort("COM5", 9600)
        Catch
        End Try
    End Sub

    Public Sub Disconnect()

        If comPort IsNot Nothing AndAlso comPort.IsOpen Then
            comPort.Close()
        End If

    End Sub

    Private Sub comPort_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles comPort.DataReceived
        On Error Resume Next
        Dim str As String = ""
        If e.EventType = SerialData.Chars Then
            Do
                Dim bytecount As Integer = comPort.BytesToRead

                If bytecount = 0 Then
                    Exit Do
                End If
                Dim byteBuffer(bytecount) As Byte

                comPort.Read(byteBuffer, 0, bytecount)
                str = str & System.Text.Encoding.ASCII.GetString(byteBuffer, 0, 1)
                If str = "" Then Exit Do
                Me.Invoke(Sub() ListBox1.Items.Add("Comanda primita: " & str))
                Dim response As String = ""
                Select Case str
                    Case "wheather"
                        response = getRequest(routes.weatherForecast & "/1")
                    Case "account"
                        response = getRequest(routes.weatherForecast & "/2")
                End Select
                If response <> "" Then
                    For i As Integer = 0 To response.Length
                        comPort.Write(response(i))
                        System.Threading.Thread.Sleep(1)
                    Next
                End If
            Loop
        End If

        RaiseEvent ScanDataRecieved(str)
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        TextBox2.Visible = RadioButton1.Checked
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Disconnect()
    End Sub
End Class
