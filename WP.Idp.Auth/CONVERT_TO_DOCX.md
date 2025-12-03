# How to Convert Technical Documentation to DOCX

## Option 1: Using Microsoft Word (Recommended)

1. **Open the HTML file**:
   - Navigate to `WP.Idp.Auth/TECHNICAL_DOCUMENTATION.html`
   - Double-click to open in your default browser
   - Or right-click → Open with → Microsoft Word

2. **Save as DOCX**:
   - In Word: File → Save As
   - Choose "Word Document (*.docx)" format
   - Save as `TECHNICAL_DOCUMENTATION.docx`

## Option 2: Using PowerShell Script (Requires Word)

1. **Run the conversion script**:
   ```powershell
   .\ConvertToDocx.ps1
   ```

2. **Or specify custom files**:
   ```powershell
   .\ConvertToDocx.ps1 -InputFile "TECHNICAL_DOCUMENTATION.md" -OutputFile "MyDocument.docx"
   ```

**Note**: This script requires Microsoft Word to be installed.

## Option 3: Using Online Converters

1. Open `TECHNICAL_DOCUMENTATION.html` in a browser
2. Copy the content
3. Use an online HTML to DOCX converter:
   - https://www.zamzar.com/convert/html-to-docx/
   - https://convertio.co/html-docx/
   - https://www.online-convert.com/

## Option 4: Using Pandoc (If Installed)

If you have Pandoc installed:

```bash
pandoc TECHNICAL_DOCUMENTATION.md -o TECHNICAL_DOCUMENTATION.docx
```

## Option 5: Using Markdown Editors

Many markdown editors can export to DOCX:

- **Typora**: File → Export → Word (.docx)
- **Mark Text**: File → Export → Word Document
- **VS Code with Markdown PDF extension**: Can export to DOCX

## Recommended Approach

**For best results**, use **Option 1** (Microsoft Word):
- Open `TECHNICAL_DOCUMENTATION.html` in Word
- Word will preserve formatting, tables, and code blocks
- You can further customize the document in Word
- Save as DOCX

The HTML file is specifically formatted to look good when opened in Word.

