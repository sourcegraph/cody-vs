# Setting up a Windows VM on Apple Silicon Mac

This guide provides detailed instructions on how to set up a Windows Virtual Machine (VM) on an Apple Silicon Mac.

## Prerequisites

Before you begin, you'll need to download and install two essential pieces of software:

1. **UTM** (Universal Turing Machine)

   - Visit the [UTM installation page](https://docs.getutm.app/installation/macos/)
   - Choose the "Install from GitHub" option

2. **Windows 11 ISO**
   - Install [CrystalFetch](https://docs.getutm.app/guides/windows/#crystalfetch) from GitHub
   - Open `CrystalFetch` and download the `Windows 11` ISO

## Creating a Windows VM in UTM

Follow these steps to create your Windows VM:

1. Open UTM
2. Click "Create a New Virtual Machine"
3. Select "Virtualize"
4. Choose "Windows"
5. Click "Browse" and select the `Windows 11` ISO you downloaded with `CrystalFetch`
6. Click "Continue"
7. Proceed through the setup, adjusting resources as needed
8. Once created, click the Play button to start the VM

> **Important**: When you see "Press any key to boot from CD or DVD", press any key promptly. If you miss this, you'll enter an interactive shell. In that case, restart the VM to try again.

> **Note**: After Windows 11 is installed, you won't need to respond to the "Press any key" prompt in future boots.

## Setting up Windows 11 Pro in UTM

1. During the Windows Setup, select `Windows 11 Pro`
2. Follow the on-screen instructions to complete the setup
3. Once setup is complete, your `Windows 11` VM should be up and running

## Next Steps

After successfully setting up your `Windows 11` VM, you can proceed with installing necessary software and configuring the environment for your specific needs.

For more information on using and troubleshooting your Windows VM, refer to the [UTM troubleshooting guide](https://docs.getutm.app/guides/windows/#troubleshooting).
