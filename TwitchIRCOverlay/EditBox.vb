Public Class EditBox

    Private Sub TextBox1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If (e.KeyCode = Keys.Enter) Then
            My.Forms.IRCOverlayMainWindow.IRCInstance.Send("PRIVMSG #" + My.Forms.IRCOverlayMainWindow.twitchUsername + " :" + TextBox1.Text)
            My.Forms.IRCOverlayMainWindow.UpdateTheBox("<" + My.Forms.IRCOverlayMainWindow.twitchUsername + "> " + TextBox1.Text + ControlChars.CrLf)
            TextBox1.Clear()
            e.SuppressKeyPress = True
        End If
    End Sub
End Class