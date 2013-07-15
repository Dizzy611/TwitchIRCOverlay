Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.Runtime.InteropServices


Public Class IRCOverlayMainWindow

    Public Sub DropFirstElement(Of T)(ByRef a() As T)
        RemoveAt(a, 0)
    End Sub

    Public Sub DropLastElement(Of T)(ByRef a() As T)
        RemoveAt(a, UBound(a))
    End Sub

    Public Sub RemoveAt(Of T)(ByRef arr As T(), ByVal index As Integer)
        Dim uBound = arr.GetUpperBound(0)
        Dim lBound = arr.GetLowerBound(0)
        Dim arrLen = uBound - lBound

        If index < lBound OrElse index > uBound Then
            Throw New ArgumentOutOfRangeException( _
            String.Format("Index must be from {0} to {1}.", lBound, uBound))

        Else
            'create an array 1 element less than the input array
            Dim outArr(arrLen - 1) As T
            'copy the first part of the input array
            Array.Copy(arr, 0, outArr, 0, index)
            'then copy the second part of the input array
            Array.Copy(arr, index + 1, outArr, index, uBound - index)

            arr = outArr
        End If
    End Sub

    <DllImport("user32.dll")> Private Shared Function HideCaret(ByVal hwnd As IntPtr) As Boolean
    End Function
    Dim args As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs
    Public twitchUsername As String
    Dim twitchPassword As String
    Dim requestedWidth As String
    Dim requestedHeight As String
    Public IRCInstance As IRC
    Dim DebugMode As Boolean

    Private Function WhatsMyNameBitch() As String
        Dim fn As String = Process.GetCurrentProcess().MainModule.FileName
        Dim tempArray As String()
        tempArray = fn.Split("\")
        Return tempArray(UBound(tempArray))
    End Function

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetTopMostWindow(Me.Handle, True)
        If args.Count = 5 Then
            DebugMode = True
        End If
        If args.Count < 4 Then
            MsgBox("Syntax: " + WhatsMyNameBitch() + " <width> <height> <username> <password>" + ControlChars.CrLf + ControlChars.CrLf + "Where width and height are the width and height you want the application to be, username is your Twitch.tv/Justin.tv username, and password is your password." + ControlChars.CrLf + ControlChars.CrLf + "Suggested values for width and height: Width should be the same as the width of the program you are overlaying, and height should be ~68, which is 3 lines of text at default DPI")
            Me.Close()
        Else
            If ((Not IsNumeric(args.Item(0))) Or (Not IsNumeric(args.Item(1)))) Then
                MsgBox("Syntax: " + WhatsMyNameBitch() + " <width> <height> <username> <password>" + ControlChars.CrLf + ControlChars.CrLf + "Where width and height are the width and height you want the application to be, username is your Twitch.tv/Justin.tv username, and password is your password." + ControlChars.CrLf + ControlChars.CrLf + "Suggested values for width and height: Width should be the same as the width of the program you are overlaying, and height should be ~68, which is 3 lines of text at default DPI")
                Me.Close()
            Else
                requestedWidth = Convert.ToInt32(args.Item(0))
                requestedHeight = Convert.ToInt32(args.Item(1))
                twitchUsername = args.Item(2)
                twitchPassword = args.Item(3)
                EditBox.Size = New System.Drawing.Size(requestedWidth, EditBox.TextBox1.Size.Height + 20)
                EditBox.TextBox1.Size = New System.Drawing.Size(requestedWidth - 5, EditBox.Size.Height - 8)
                EditBox.Show()
                SetTopMostWindow(EditBox.Handle, True)
                Me.Size = New System.Drawing.Size(requestedWidth, requestedHeight)
                Me.TextBox1.Size = New System.Drawing.Size(requestedWidth, requestedHeight - 8)
                Dim tempString As String = twitchUsername + ".jtvirc.com"
                Me.IRCInstance = New IRC(tempString, 6667)
                If Not IRCInstance.IsConnected() Then
                    MsgBox("Error: Connection Failed")
                    Me.Close()
                Else
                    IRCInstance.Send("PASS " + twitchPassword)
                    IRCInstance.Send("USER " + twitchUsername + " " + twitchUsername + " " + twitchUsername + " " + twitchUsername)
                    IRCInstance.Send("NICK " + twitchUsername)
                    TextBox1.Text = ""
                    HideCaret(Me.TextBox1.Handle)
                    Timer1.Start()
                End If
            End If
        End If
        EditBox.Location = New Point(Me.Location.X, Me.Location.Y + Me.requestedHeight + 7)
    End Sub



    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        EditBox.WindowState = Me.WindowState
        EditBox.Location = New Point(Me.Location.X, Me.Location.Y + Me.requestedHeight + 7)
        If Not IRCInstance.IsConnected() Then
            Me.Close()
        Else
            HideCaret(TextBox1.Handle)
            Dim received_message As String = IRCInstance.ReceiveString()
            Dim seperated_message As String() = received_message.Split(" ")
            If seperated_message(0) = "PING" Then
                IRCInstance.Send("PONG :" + twitchUsername + ".jtvirc.com")
                If DebugMode Then
                    MsgBox("DEBUG: PING? PONG!")
                End If
            ElseIf seperated_message.Length >= 2 Then
                If seperated_message(1) = "376" Then
                    If DebugMode Then
                        MsgBox("DEBUG: We're fully connected. Joining channel.")
                    End If
                    IRCInstance.Send("JOIN #" + twitchUsername)
                ElseIf seperated_message(1) = "366" Then
                    If DebugMode Then
                        MsgBox("DEBUG: We've joined the channel.")
                    End If
                    UpdateTheBox(ControlChars.CrLf + ControlChars.CrLf + "Twitch IRC Overlay v1.0 Operational." + ControlChars.CrLf)
                ElseIf seperated_message(1) = "PRIVMSG" Then
                    If seperated_message(2) = "#" + twitchUsername Then
                        Dim seperated_nick As String() = seperated_message(0).Split("!")
                        Dim source_nick As String = seperated_nick(0)
                        Dim message_text As String = ""
                        For i As Int32 = 3 To (seperated_message.GetLength(0) - 1)
                            message_text = message_text + seperated_message(i)
                            If Not i = (seperated_message.GetLength(0) - 1) Then
                                message_text = message_text + " "
                            End If
                        Next i
                        source_nick = source_nick.Remove(0, 1)
                        message_text = message_text.Remove(0, 1)
                        Dim ascii As Encoding = Encoding.ASCII
                        Dim CTCPCheckArray As Byte() = ascii.GetBytes(message_text)
                        If CTCPCheckArray(0) = 1 Then
                            If DebugMode Then
                                MsgBox("DEBUG: Received CTCP from " + source_nick)
                            End If
                            'We have a CTCP message
                            message_text = message_text.Replace(Chr(1), "")
                            Dim ctcp_split As String() = message_text.Split()
                            Dim ctcp_type As String = ctcp_split(0)
                            DropFirstElement(ctcp_split)
                            Dim ctcp_message As String = Join(ctcp_split)
                            Select Case ctcp_type
                                Case "ACTION"
                                    UpdateTheBox("* " + source_nick + " " + ctcp_message + ControlChars.CrLf)
                                Case "VERSION"
                                    'Twitch doesn't support the NOTICE command, so we can't actually reply to CTCPs, this is what we would do if that wasn't the case.
                                    '   IRCInstance.Send("NOTICE " + source_nick + " " + Chr(1) + "VERSION Insidious's Twitch IRC Overlay Client v0.3" + Chr(1))
                                Case "PING"
                                    'Twitch doesn't support the NOTICE command, so we can't actually reply to CTCPs, this is what we would do if that wasn't the case.
                                    '   IRCInstance.Send("NOTICE " + source_nick + " " + Chr(1) + "PING " + ctcp_message + Chr(1))
                                Case Else
                                    UpdateTheBox(source_nick + " performed an unimplemented CTCP " + ctcp_type + " with contents " + ctcp_message + ControlChars.CrLf)
                            End Select
                        Else
                            If DebugMode Then
                                MsgBox("DEBUG: Received channel message from " + source_nick)
                            End If
                            UpdateTheBox("<" + source_nick + "> " + message_text)
                        End If

                    End If
                Else
                    If DebugMode Then
                        UpdateTheBox(received_message)
                    End If
                End If
            Else
                If DebugMode Then
                    UpdateTheBox(received_message)
                End If
            End If
        End If
    End Sub

    Public Sub UpdateTheBox(ByVal message As String)
        Me.TextBox1.Text = Me.TextBox1.Text + message
        Dim Temp1 As Int32
        Dim Temp2 As Int32
        Temp1 = TextBox1.SelectionStart
        Temp2 = TextBox1.SelectionLength
        TextBox1.SelectionStart = Len(TextBox1.Text)
        TextBox1.SelectionLength = 0
        TextBox1.ScrollToCaret()
        TextBox1.Refresh()
        TextBox1.SelectionStart = Temp1
        TextBox1.SelectionLength = Temp2
    End Sub

    Private Sub TextBox1_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyUp
        HideCaret(TextBox1.Handle)
    End Sub

    Private Sub TextBox1_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles TextBox1.MouseDown
        HideCaret(TextBox1.Handle)
    End Sub

    Private Sub IRCOverlayMainWindow_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles MyBase.Paint
        EditBox.Location = New Point(Me.Location.X, Me.Location.Y + Me.requestedHeight + 7)
        EditBox.WindowState = Me.WindowState
    End Sub

    Private Sub IRCOverlayMainWindow_Move(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Move
        EditBox.Location = New Point(Me.Location.X, Me.Location.Y + Me.requestedHeight + 7)
        EditBox.WindowState = Me.WindowState
    End Sub

    Private Sub IRCOverlayMainWindow_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If Not IRCInstance Is Nothing Then
            IRCInstance.Quit()
        End If
    End Sub
End Class

Public Class IRC
    Private s As Socket = Nothing

    Public Sub New(ByVal server As String, ByVal port As Integer)
        Dim hostEntry As IPHostEntry = Nothing
        Dim hostName As String = server
        hostEntry = Dns.GetHostEntry(hostName)
        Dim address As IPAddress
        For Each address In hostEntry.AddressList
            Dim endPoint As New IPEndPoint(address, port)
            Dim tempSocket As New Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)

            tempSocket.Connect(endPoint)

            If tempSocket.Connected Then
                s = tempSocket
                s.Blocking = False
                Exit For
            End If
        Next address
    End Sub

    Public Sub Quit()
        Send("QUIT :Insidious's Twitch IRC Overlay v1.0 Says Goodbye!")
        s.Disconnect(False)
    End Sub

    Public Function IsConnected() As Boolean
        If s Is Nothing Then
            Return False
        Else
            If s.Connected Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Public Function Send(ByVal command As String) As Boolean
        If Not Me.IsConnected() Then
            Return False
        End If
        Dim ascii As Encoding = Encoding.ASCII
        Dim request As String = command + ControlChars.CrLf
        Dim bytesSent As Byte() = ascii.GetBytes(request)
        If (s.Send(bytesSent, bytesSent.Length, 0) = bytesSent.Length) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ReceiveString() As String
        If Not Me.IsConnected() Then
            Return "ERR_NO_CONNECT"
        End If
        Dim tinyBuffer(1) As Byte
        Dim amountReceived As Int16
        Dim result As String = ""
        Dim doneTime As Boolean = False
        Dim ascii As Encoding = Encoding.ASCII
        Do
            Try
                amountReceived = s.Receive(tinyBuffer, 1, 0)
            Catch ex As SocketException
                If ex.ErrorCode = 10035 Then
                    amountReceived = 0
                Else
                    Throw ex
                End If
            End Try
            If amountReceived = 0 Then
                doneTime = True
            Else
                result = result + ascii.GetString(tinyBuffer, 0, 1)
                If (ascii.GetString(tinyBuffer, 0, 1) = ControlChars.Lf) Then
                    doneTime = True
                End If
            End If
        Loop While doneTime = False
        Return result
    End Function

End Class