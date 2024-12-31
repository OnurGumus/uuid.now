# uuid.now

A simple, single-page website for generating all types of GUIDs quickly and easily. Zero fuss, zero clutter—just your GUIDs, right when you need them.

A live version is available at https://uuid.now or https://guid.now.

## Overview

**uuid.now** is designed to make GUID (or UUID) generation straightforward. Generate a Version 4 (v4) GUID for secure randomness, a time-based GUID for easy database indexing, or even a zero GUID placeholder—all in your browser.

### Why uuid.now?
- **Easy to Remember:** No complicated URLs—just `uuid.now`.
- **Fast & Simple:** Instantly create and copy GUIDs on one page.
- **Multiple Formats:** v4, time-based, and zero GUIDs all in one place.
- **Browser-Based:** Uses the Crypto API for secure, random v4 GUIDs.

## Features

1. **V4 GUID Generator**  
   Utilizes your browser’s Crypto API for secure and random GUID creation.  
2. **Time-Based GUIDs**  
   Embed timestamps for sorting or indexing by creation time.  
3. **Zero GUID**  
   Quickly copy an all-zero GUID for placeholders or reset scenarios.  
4. **One-Click Copy**  
   Click the “Copy” button or highlight and copy directly from the screen.  

## Requirements

- **.NET SDK 9**  
- **Node.js v18** or higher  

## Build Steps

1. **Restore .NET tools**  
   ```bash
   dotnet tool restore
   ```
2. **Restore packages with Paket**  
   ```bash
   dotnet paket restore
   ```
3. **Build the .NET project**  
   ```bash
   dotnet build
   ```
4. **Navigate to the client folder**  
   ```bash
   cd src/Client
   ```
5. **Start the development server**  
   ```bash
   npm i
   ```
6. **Start the development server**  
   ```bash
   npm run start
   ```
   This opens a local development server. Any changes you make will hot-reload in your browser.
7. **Build for production**  
   ```bash
   npm run build
   ```
   Bundles and optimizes the app for production deployment.

## Contributing

Contributions, issues, and feature requests are always welcome. Feel free to open a pull request or submit an issue to help make **uuid.now** even better!

## License

This project is provided under the [MIT License](LICENSE). You’re free to use, modify, and distribute it as you see fit.

