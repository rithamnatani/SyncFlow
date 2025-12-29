# SyncFlow

A real-time synchronization and chat application built with Azure Static Web Apps, utilizing a microservices architecture with .NET and Python Azure Functions.

## Architecture

- **Client**: Angular application (hosted via Azure Static Web Apps).
- **API**: Azure Functions (.NET 10 Isolated) handling SignalR negotiation and message triggers.
- **AI Worker**: Azure Functions (Python 3.14) processing background tasks via Azure Service Bus.
- **Infrastructure**:
  - Azure Service Bus (Message Broker)
  - Azure SignalR Service (Real-time broadcasting)
  - Azure Storage (State management)

## Prerequisites

- **.NET 10 SDK** (for API)
- **Python 3.11+** (for AI Worker)
- **Node.js 18+** & **Angular CLI** (for Client)
- **Azure Functions Core Tools v4** (`bun install -g azure-functions-core-tools@4`)
- **Azure Static Web Apps CLI** (`bun install -g @azure/static-web-apps-cli`)
- **Azure CLI**

## Setup

1.  **Clone the repository**
2.  **Infrastructure Setup**:
    - Create a Service Bus Namespace & Queue (named `orders`).
    - Create a SignalR Service (Serverless mode).
    - Create a Storage Account.
3.  **Local Configuration**:
    - Create `api/local.settings.json`:
      ```json
      {
        "IsEncrypted": false,
        "Values": {
          "AzureWebJobsStorage": "<YOUR_STORAGE_CONNECTION_STRING>",
          "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
          "AzureSignalRConnectionString": "<YOUR_SIGNALR_CONNECTION_STRING>",
          "ServiceBusConnection": "<YOUR_SERVICE_BUS_CONNECTION_STRING>"
        }
      }
      ```
    - Create `ai-worker/local.settings.json`:
      ```json
      {
        "IsEncrypted": false,
        "Values": {
          "AzureWebJobsStorage": "<YOUR_STORAGE_CONNECTION_STRING>",
          "FUNCTIONS_WORKER_RUNTIME": "python",
          "AzureSignalRConnectionString": "<YOUR_SIGNALR_CONNECTION_STRING>",
          "ServiceBusConnection": "<YOUR_SERVICE_BUS_CONNECTION_STRING>"
        }
      }
      ```
4.  **CORS**:
    - Add `http://localhost:4200` and `http://localhost:4280` to your Azure SignalR Service CORS settings in the Azure Portal.

## Running Locally

To run the full solution, open three dedicated terminals:

**1. AI Worker (Service Bus Consumer)**
```bash
cd ai-worker
# Create venv if first time: python -m venv .venv
# Activate venv: .venv\Scripts\activate
# Install deps: pip install -r requirements.txt
func start
```

**2. API (SignalR & Producer)**
```bash
cd api
func start
```

**3. Client (Frontend)**
1. Install dependencies:
   ```bash
   cd client
   bun install
   ```
2. Run from **Root Directory**:
   ```bash
   cd ..
   swa start
   ```
*(Access app at `http://localhost:4200`)*

## Testing the Flow

You can trigger a test message through the API to verify the full loop:

```bash
curl -X POST http://localhost:7071/api/SendMessage -d "Hello SyncFlow!"
```

**Expected Result:**
1.  API sends message to Service Bus `orders` queue.
2.  AI Worker picks up message.
3.  AI Worker broadcasts message via SignalR.
4.  Client receives and displays: `New Message received: Hello SyncFlow!`
