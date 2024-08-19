# 设置 Redmine 服务器配置
$redmineUrl = "http://your-redmine-instance.com"
$apiKey = "your_api_key"

# CSV 文件路径
$csvFile = "C:\path\to\time_entries.csv"

# 读取 CSV 文件
$entries = Import-Csv -Path $csvFile

foreach ($entry in $entries) {
    $payload = @{
        "time_entry" = @{
            "issue_id" = $entry.issue_id
            "hours" = $entry.hours
            "activity_id" = $entry.activity_id
            "spent_on" = $entry.spent_on
            "comments" = $entry.comments
        }
    }

    $jsonPayload = $payload | ConvertTo-Json

    $headers = @{
        "X-Redmine-API-Key" = $apiKey
        "Content-Type" = "application/json"
    }

    # 发起 HTTP POST 请求
    $response = Invoke-RestMethod -Uri "$redmineUrl/time_entries.json" -Method Post -Headers $headers -Body $jsonPayload

    if ($response -eq $null) {
        Write-Host "Failed to import time entry for issue $($entry.issue_id)"
    } else {
        Write-Host "Successfully imported time entry for issue $($entry.issue_id)"
    }
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Redmine Time Entry Importer</title>
</head>
<body>
    <h1>Import Time Entries to Redmine</h1>
    <input type="file" id="csvFileInput" accept=".csv" />
    <button onclick="importTimeEntries()">Import</button>

    <script src="importer.js"></script>
</body>
</html>
function importTimeEntries() {
    const fileInput = document.getElementById('csvFileInput');
    if (!fileInput.files.length) {
        alert('Please select a CSV file.');
        return;
    }

    const file = fileInput.files[0];
    const reader = new FileReader();

    reader.onload = function(event) {
        const csvData = event.target.result;
        const rows = csvData.split('\n');

        const headers = rows[0].split(',');

        for (let i = 1; i < rows.length; i++) {
            const row = rows[i].split(',');
            const timeEntry = {};

            for (let j = 0; j < headers.length; j++) {
                timeEntry[headers[j].trim()] = row[j].trim();
            }

            sendTimeEntryToRedmine(timeEntry);
        }
    };

    reader.readAsText(file);
}

function sendTimeEntryToRedmine(timeEntry) {
    const apiUrl = 'http://your-redmine-instance.com/time_entries.json';
    const apiKey = 'your_api_key';

    const payload = {
        time_entry: {
            issue_id: timeEntry.issue_id,
            hours: timeEntry.hours,
            activity_id: timeEntry.activity_id,
            spent_on: timeEntry.spent_on,
            comments: timeEntry.comments,
        }
    };

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Redmine-API-Key': apiKey
        },
        body: JSON.stringify(payload)
    })
    .then(response => {
        if (response.ok) {
            console.log(`Successfully imported time entry for issue ${timeEntry.issue_id}`);
        } else {
            response.text().then(text => {
                console.error(`Failed to import time entry for issue ${timeEntry.issue_id}: ${text}`);
            });
        }
    })
    .catch(error => {
        console.error(`Error importing time entry for issue ${timeEntry.issue_id}:`, error);
    });
}


