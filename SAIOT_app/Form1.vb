Imports System.IO.Ports
Imports System.Runtime.InteropServices

Public Class Form1

    Public Event ScanDataRecieved(ByVal data As String)
    WithEvents comPort As SerialPort


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim x As String = ""
        For Each sp As String In My.Computer.Ports.SerialPortNames
            x = x + " " + sp
        Next
        Connect()
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
            comPort = My.Computer.Ports.OpenSerialPort("COM3", 115200, Parity.None, 5, StopBits.One)
            comPort.ReadTimeout = 1000
            AddHandler comPort.DataReceived, AddressOf comPort_DataReceived


            '  comPort.Write("1")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub Disconnect()

        If comPort IsNot Nothing AndAlso comPort.IsOpen Then
            comPort.Close()
        End If

    End Sub

    Dim receivedCommand As Boolean = False
    Public cachedString = ""
    Private Sub comPort_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs)
        Try
            Dim str As String = ""
            If e.EventType = SerialData.Chars Then
                Do
                    Dim bytecount As Integer = comPort.BytesToRead

                    If bytecount = 0 Then
                        Exit Do
                    End If
                    'Dim byteBuffer(bytecount) As Byte
                    Try
                        str = comPort.ReadLine()

                    Catch
                        Exit Do
                    End Try
                    'comPort.Read(byteBuffer, 0, bytecount)
                    'str = str & System.Text.Encoding.ASCII.GetString(byteBuffer, 0, 1)
                    If str = "" Then Exit Do

                    Me.Invoke(Sub() ListBox1.Items.Add("Comanda primita: " & str))

                    Me.Invoke(Sub() Panel1.VerticalScroll.Maximum = ListBox1.Height)
                    Dim response As String = ""
                    Select Case str.Trim
                        Case "0"
                            comPort.Write(response("@"))
                            System.Threading.Thread.Sleep(1)
                            comPort.Write(response("$"))
                        Case "GET  a", "wheather"

                            If Not cachedString = "" Then
                                comPort.Write(cachedString)
                                Me.Invoke(Sub() ListBox1.Items.Add("Comanda trimisa: " & cachedString))
                                cachedString = ""
                                receivedCommand = False
                                Return
                            End If
                            If Not receivedCommand Then
                                receivedCommand = True
                            Else
                                receivedCommand = False
                                Return
                            End If
                            response = getRequest(routes.weatherForecast & "/1")
                        Case "GET  b", "account"
                            If Not cachedString = "" Then
                                comPort.Write(cachedString)
                                Me.Invoke(Sub() ListBox1.Items.Add("Comanda trimisa: " & cachedString))
                                cachedString = ""
                                receivedCommand = False
                                Return
                            End If
                            If Not receivedCommand Then
                                receivedCommand = True
                            Else
                                receivedCommand = False
                                Return
                            End If

                            response = getRequest(routes.weatherForecast & "/2")

                        Case "GET  c"
                            comPort.Write("p")
                            Return
                    End Select
                    If response <> "" Then
                        If (response.Length = 1) Then response = "0" & str
                        response = response.Replace("-", "0")
                        If (response.Length > 2) Then response = "99"

                        comPort.Write(response.Substring(0, 1))
                        cachedString = response.Substring(1, 1)
                        'For i As Integer = 0 To response.Length - 1
                        '    comPort.Write(response(i))
                        '    System.Threading.Thread.Sleep(100)
                        'Next

                        Me.Invoke(Sub() ListBox1.Items.Add("Comanda trimisa: " & response.Substring(0, 1)))
                        Me.Invoke(Sub() Panel1.VerticalScroll.Maximum = ListBox1.Height)
                    End If
                Loop
            End If

            RaiseEvent ScanDataRecieved(str)
        Catch
        End Try
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        TextBox2.Visible = RadioButton1.Checked
        If RadioButton1.Checked Then
            If TextBox1.Text = "" OrElse TextBox1.Text = "Cont" Then TextBox1.Text = "Latitudine"
        Else
            If TextBox1.Text = "" OrElse TextBox1.Text = "Latitudine" Then TextBox1.Text = "Cont"
        End If
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Disconnect()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        comPort.Write(TextBox3.Text)
        TextBox3.Text = ""
    End Sub
End Class
