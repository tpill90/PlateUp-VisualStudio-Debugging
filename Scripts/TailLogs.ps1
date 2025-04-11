# Getting PlateUp install dir
[xml]$parsedProps = Get-Content Environment.props
$plateupInstallDir = ([string]$parsedProps.Project.PropertyGroup.PlateUpInstallDir).Trim()
$logFile = "$plateupInstallDir\PlateUp\BepInEx\LogOutput.log"

#TODO review this and cleanup the generated code
# Variables for tracking log state
$currentColor = $null
$currentEntry = @()  # Buffer to store multi-line log entries
$shouldFilter = $false  # Flag to determine if an entry should be filtered

Get-Content -Path $logFile -Wait | ForEach-Object {
    $line = $_

    # Check if this line is the start of a new log entry
    if ($line -match "^\[.*?\]")
    {
        # Process the previous log entry before starting a new one
        if (-not $shouldFilter)
        {
            foreach ($entry in $currentEntry)
            {
                if ($currentColor)
                {
                    Write-Host $entry -ForegroundColor $currentColor
                }
                else
                {
                    Write-Host $entry
                }
            }
        }

        # Reset state for new log entry
        $currentEntry = @($line)
        $shouldFilter = $false
        $currentColor = $null

        # Determine log level and whether it should be filtered
        if ($line -match "\[APPLICATION\]|\[HUEY-CORE\]|Odin Serializer")
        {
            $shouldFilter = $true
        }
        elseif ($line -match "Warning")
        {
            $currentColor = "Yellow"
        }
        elseif ($line -match "Error")
        {
            $currentColor = "Red"
        }
        elseif ($line -match "Info")
        {
            $currentColor = "DarkGray"
        }
        elseif ($line -match "Message")
        {
            $currentColor = "White"
        }
    }
    else
    {
        # Append multi-line log continuation
        $currentEntry += $line
    }
}
