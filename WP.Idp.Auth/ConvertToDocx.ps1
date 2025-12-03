# PowerShell script to convert Markdown to DOCX using Word COM
# Requires Microsoft Word to be installed

param(
    [string]$InputFile = "TECHNICAL_DOCUMENTATION.md",
    [string]$OutputFile = "TECHNICAL_DOCUMENTATION.docx"
)

Write-Host "Converting $InputFile to $OutputFile..." -ForegroundColor Green

try {
    # Read the markdown file
    $content = Get-Content -Path $InputFile -Raw -Encoding UTF8
    
    # Create Word application
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    
    # Create a new document
    $doc = $word.Documents.Add()
    
    # Convert markdown to plain text (basic conversion)
    # Replace markdown headers
    $content = $content -replace '^# (.+)$', '$1' -replace '^## (.+)$', '$1' -replace '^### (.+)$', '$1'
    # Replace code blocks
    $content = $content -replace '```[\s\S]*?```', '[Code Block]'
    # Replace inline code
    $content = $content -replace '`([^`]+)`', '$1'
    # Replace bold
    $content = $content -replace '\*\*([^\*]+)\*\*', '$1'
    
    # Split into paragraphs
    $paragraphs = $content -split "`n`n"
    
    foreach ($para in $paragraphs) {
        if ($para.Trim() -ne '') {
            $doc.Content.InsertAfter($para.Trim() + "`n`n")
        }
    }
    
    # Save as DOCX
    $fullPath = Join-Path (Get-Location) $OutputFile
    $doc.SaveAs([ref]$fullPath, [ref]16) # 16 = wdFormatDocumentDefault (DOCX)
    
    # Close and quit
    $doc.Close()
    $word.Quit()
    
    # Release COM objects
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($doc) | Out-Null
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    
    Write-Host "Successfully created $OutputFile" -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "`nNote: This script requires Microsoft Word to be installed." -ForegroundColor Yellow
    Write-Host "Alternative: Open TECHNICAL_DOCUMENTATION.html in Word and save as DOCX" -ForegroundColor Yellow
}

