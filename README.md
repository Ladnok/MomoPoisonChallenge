 # MomoPoisonChallenge
Livesplit component for the game Momodora:RUTM. Currently supporting game version v1.07

## License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/Ladnok/MomoPoisonChallenge/blob/v1.0/LICENSE) file for details.

This project uses LiveSplit, which is licensed under the MIT License.

## Installation
First, make sure that you have a LiveSplit installation http://livesplit.org/downloads/.

1. Download the latest version of MomoPoisonChallenge.dll from the [Releases](https://github.com/Ladnok/MomoPoisonChallenge/releases) page.
2. Navigate to your LiveSplit installation folder, then place MomoPoisonChallenge.dll inside: LiveSplit/Components/
3. Launch LiveSplit, go to Edit Layout, and add the "Momodora RUtM Poison Challange" component from the "Other" category.

![image](https://github.com/user-attachments/assets/85145167-5a1b-4a15-9c0d-4a15156b75bf)

## How to use
Once you have added the component to the layout:
1. Open the game and start playing.
2. That's it.
- The component automatically activates as long as both LiveSplit and Momodora: RUtM are running.
- No need to start the timer or begin a new save file!

## Updates
- Livesplit has a built-in auto-updater.<br>
- Whenever you open LiveSplit, it will check for updates and prompt you to download the latest version if necessary.<br>
- No manual updates needed!

## Building from Source
If you want to modify the project or compile your own version, follow these steps.

### Prerequisites
Make sure you have the following installed:
- **[Visual Studio](https://visualstudio.microsoft.com/)** (Recommended: Community Edition)
- **.NET Framework 4.8.1 Development Kit**
- **Git** (Optional, but recommended for version control)

### Cloning the Repository
You can clone the repository using either the command line or Visual Studio.

#### Using the Console
1. Open a terminal or command prompt in the directory where you want to store the project.
2. Run the following commands:
   ```sh
   git clone https://github.com/Ladnok/MomoPoisonChallenge.git
   cd MomoPoisonChallenge
   ```
   > This will create a folder named `MomoPoisonChallenge` in your current directory.

#### Using Visual Studio
1. Open **Visual Studio**.
2. Click on **Clone a Repository** from the start window.
3. In the **Repository Location** field, enter:
  ```https://github.com/Ladnok/MomoPoisonChallenge.git```
4. Choose a local folder where you want to store the project.
5. Click **Clone**.

Once cloned, you can open the solution file (`MomoPoisonChallenge.sln`) and start working on the project.

### Compiling the Project
1. Open `MomoPoisonChallenge.sln` in **Visual Studio**.
2. Make sure you have **.NET Framework 4.8.1** installed.
3. Set the build configuration to **Release**.
4. Click **Build → Build Solution (Ctrl+Shift+B)**.
5. The compiled `.dll` file will be generated in the `bin\Release` folder.

### Using Your Compiled Version
Once compiled, the new `MomoPoisonChallenge.dll` will be located in the root directory, inside a folder named `Components`.<br>
After that just follow the **Installation** steps above, replacing `MomoPoisonChallenge.dll` with your newly built version.
