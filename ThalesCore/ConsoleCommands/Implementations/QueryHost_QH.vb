﻿''
'' This program is free software; you can redistribute it and/or modify
'' it under the terms of the GNU General Public License as published by
'' the Free Software Foundation; either version 2 of the License, or
'' (at your option) any later version.
''
'' This program is distributed in the hope that it will be useful,
'' but WITHOUT ANY WARRANTY; without even the implied warranty of
'' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'' GNU General Public License for more details.
''
'' You should have received a copy of the GNU General Public License
'' along with this program; if not, write to the Free Software
'' Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
'' 

Imports ThalesSim.Core
Imports ThalesSim.Core.Resources

Namespace ConsoleCommands

    ''' <summary>
    ''' Query host command.
    ''' </summary>
    ''' <remarks></remarks>
    <ThalesConsoleCommandCode("QH", "Displays information about the host port configuration.")> _
    Public Class QueryHost_QH
        Inherits AConsoleCommand

        ''' <summary>
        ''' No stack, since this is an immediate command.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub InitializeStack()
        End Sub

        ''' <summary>
        ''' Return host connection info (TCP only).
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ProcessMessage() As String
            Return "Message header length: 04" + vbCrLf + _
                   "Protocol: Ethernet" + vbCrLf + _
                   "Character format: ASCII" + vbCrLf + _
                   "IP address: " + System.Net.Dns.GetHostAddresses(My.Computer.Name)(0).ToString + vbCrLf + _
                   "Well-Known-Port address: " + CType(Resources.GetResource(Resources.WELL_KNOWN_PORT), Integer).ToString
        End Function

    End Class

End Namespace