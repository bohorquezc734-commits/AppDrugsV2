$header = @{ "alg" = "HS256"; "typ" = "JWT" } | ConvertTo-Json -Compress
$headerBytes = [System.Text.Encoding]::UTF8.GetBytes($header)
$headerBase64 = [System.Convert]::ToBase64String($headerBytes).TrimEnd('=').Replace('+','-').Replace('/','_')

$payload = @{ "nameid" = "1"; "email" = "admin@app.com"; "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" = "Admin"; "exp" = [DateTimeOffset]::UtcNow.AddHours(1).ToUnixTimeSeconds(); "iss" = "AppDrugsV2"; "aud" = "AppDrugsV2Client" } | ConvertTo-Json -Compress
$payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
$payloadBase64 = [System.Convert]::ToBase64String($payloadBytes).TrimEnd('=').Replace('+','-').Replace('/','_')

$key = [System.Text.Encoding]::UTF8.GetBytes("TuSuperSecretKeyDeAlMenos32CaracteresLargos1234567")
$hmac = [System.Security.Cryptography.HMACSHA256]::new($key)
$signatureBytes = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("$headerBase64.$payloadBase64"))
$signatureBase64 = [System.Convert]::ToBase64String($signatureBytes).TrimEnd('=').Replace('+','-').Replace('/','_')

$token = "$headerBase64.$payloadBase64.$signatureBase64"

try {
    $response = Invoke-WebRequest -Uri "http://localhost:46321/api/Reports/appointments/pdf?status=EnProceso" -Headers @{ Authorization = "Bearer $token" } -UseBasicParsing
    Write-Host "Success! Status Code: $($response.StatusCode)"
}
catch {
    Write-Host "Error!"
    $ex = $_.Exception.Response
    if ($ex) {
        $stream = $ex.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $msg = $reader.ReadToEnd()
        Write-Host "Message: $msg"
    } else {
        Write-Host $_.Exception.Message
    }
}
