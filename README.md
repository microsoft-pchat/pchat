# PCHAT

## What is Pchat?
Pchat is a simple file based I/O chat solution that allows users to talk to each other and share their screen for collaboration.

## Setting Up Pchat
1. Get permissions to access the file share
2. Launch a powershell window
3. Navigate to the pchat directory, then to the `chat` subdirectory
4. Run `Launch_pChat.ps1` in the powershell window to start pChat

## Startup Options
| Option | Type | Description | Optional/Mandatory | 
| ----------- | ----------- | ----------- | ----------- |
| ChatChannelName | string | Name of the channel to join  | Mandatory |
| Name | string | UserName | Optional |
| ShowOldPosts | bool | Whether chat history will be shown | Optional |
| HomeShare | string | Server file share | Optional |
