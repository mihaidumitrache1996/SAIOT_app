Module Module1
    Public url As String = "https://localhost:7144/"
    Public routes As New RoutesClass
    Public Class RoutesClass
        Public weatherForecast As String = "WeatherForecast"
        Public accountFollowers As String = "Followers"
    End Class

    Public Function getRequest(route As String)
        Dim webClient As New System.Net.WebClient()
        Dim response As String = ""
        Try
            webClient.Headers.Add("Content-Type", "application/json")
        Catch
        End Try
        Try
            response = webClient.DownloadString(url & route)
            webClient = Nothing
            If response = Nothing Then response = ""
        Catch ex As Exception
        End Try
        Return response
    End Function

    Public Function postRequest(route As String, data As String)
        Dim webClient As New System.Net.WebClient()
        Dim response As String = ""
        Try
            webClient.Headers.Add("Content-Type", "application/json")
        Catch
        End Try
        Try
            Dim bytArguments As Byte() = System.Text.Encoding.UTF8.GetBytes((data))
            Dim bytRetData As Byte() = webClient.UploadData(url & route, "POST", bytArguments)
            response = (System.Text.Encoding.UTF8.GetString(bytRetData))
            webClient = Nothing
            If response = Nothing Then response = ""
        Catch ex As Exception

        End Try
        Return response
    End Function
End Module
