# Enhanced Telemetry Client Application: `acRemoteServerUDP_Example`

## Overview

This project is an enhanced version of the acRemoteServerUDP_Example, which is a telemetry client application designed to connect to UDP and WebSocket servers to send and receive data. The goal of this project is to modify the existing C# application to act as a remote telemetry client for the Assetto Corsa Dedicated server. This application connects to the server via UDP, receives telemetry data, and forwards it as JSON over a WebSocket. Additionally, the application includes a user interface (UI) for configuring the IPs and ports for both connections, making it user-friendly and configurable.

## Features

* **Windows Forms GUI:** A user-friendly interface for inputting UDP and WebSocket server details.

* **Cross-Platform Support:** The application can be run on both Windows and Linux (WSL) environments with X11 forwarding enabled.

* **WebSocket and UDP Communication:** The client can establish connections with both UDP and WebSocket servers for data exchange.

## Setup and Configuration

### Prerequisites

* **.NET SDK:** Ensure that you have the .NET SDK installed. This project was tested with .NET SDK 8.0.400.

* **Mono:** If running on Linux/WSL, make sure Mono is installed (sudo apt-get install mono-complete).

* **Xming/XLaunch:** For running Windows Forms applications in WSL, you need an X Server like Xming/XLaunch installed on your Windows machine.

* **Python 3:** To run the UDP and WebSocket server scripts.

* **wscat:** A WebSocket client used for testing WebSocket connections.

### Cloning the Repository

Clone the repository and navigate to the project directory:

```bash
   git clone https://github.com/your-repo/acRemoteServerUDP_Example.git
   cd acRemoteServerUDP_Example
```

### Building the Application

1. Modify the Project Files:

   * Update the `.csproj` file to include necessary references for Windows Forms, WebSocket, and UDP communication.
  
   * Add `System.Drawing.Common` for color management.

2. Build the Project application:

```bash
   dotnet build acRemoteServerUDP_Example.csproj
```

### Running the Application on Linux/WSL

1. **Install Xming/XLaunch:**

   * Download and install Xming or XLaunch on your Windows machine.
   * Launch XLaunch and configure it for `X11 forwarding`.

2. **Configure WSL for X11 Forwarding:**

   Export the DISPLAY environment variable in your WSL session:

   ```bash
      export DISPLAY=$(cat /etc/resolv.conf | grep nameserver | awk '{print $2}'):0.0
   ```

3. **Run the Application:**

   ```bash
      cd bin/Debug
      mono acRemoteServerUDP_Example.exe
   ```

4. Testing the Application:

   Use wscat to connect to the WebSocket server:

   ```bash
      wscat -c ws://127.0.0.1:12000
   ```

### Running the Application on Windows

1. **Install .NET SDK:** Ensure that the .NET SDK is installed on your machine.

2. **Run the Application:**

   Simply navigate to the bin/Debug directory and execute the `.exe` file:

   ```bash
      cd bin/Debug
      ./acRemoteServerUDP_Example.exe
   ```

### UDP and WebSocket Server Setup

For testing purposes, simple UDP and WebSocket servers were set up using Python.

```bash
   python3 udp_server.py
```

```bash
   python3 ws_server.py
```

### Troubleshooting

* Unable to Connect to Remote Server:
   * Ensure that the UDP and WebSocket servers are running and accessible at the specified IP addresses and ports.
  
   * Check that no other application is using the specified ports (use `sudo lsof -i :<PORT>` to check).

* Mono X11 Error:
   * If running on Linux/WSL, ensure that X11 forwarding is correctly set up with `Xming` or` XLaunch`.
  
   * Confirm that the `DISPLAY` environment variable is correctly configured.
  
