# PCHAT

## What is Pchat?
Pchat is a simple file based I/O chat solution that allows users to talk to each other and share their screen for collaboration.

## Setting Up Pchat
1. Get permissions to access the file share
2. Launch a powershell window
3. Navigate to the pchat directory, then to the `chat` subdirectory
4. Run `generateShortcut.ps1` in the powershell window to generate the shortcut for pchat
5. Click on the `pChat.lnk` shortcut to start pchat
6. Follow the prompt to join the right channel

## Other Commands
Enter these commands into the chat window
| Command(s) | Function |
| ----------- |  ----------- |
| quit<br/>exit<br/>stop<br/>leave | Exits the pchat program |
| share | Start sharing screen |
| help | See help text |

## Manually Launching Pchat (Advanced)
Command: `chat.ps1 -ChatChannelName <ChannelName> -Name <UserName> -ShowOldPost <True/False> -HomeShare <Server/Directory Location>`
### Startup Options for chat.ps1
| Option | Type | Description | Optional/Mandatory | 
| ----------- | ----------- | ----------- | ----------- |
| ChatChannelName | string | Name of the channel to join  | Mandatory |
| Name | string | UserName | Optional |
| ShowOldPosts | bool | Whether chat history will be shown | Optional |
| HomeShare | string | Server file share | Optional |

