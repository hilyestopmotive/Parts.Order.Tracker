# Parts.Order.Tracker.UI

The `UI` project is the browser-based frontend of the Parts Order Tracker
application. It lets users browse and filter parts, manage favorites and a
shopping cart, place authenticated orders, and receive live order-status updates.

The application is implemented as a standalone Blazor WebAssembly client and
communicates with `PartsOrderTracker.API` over HTTPS.

## Table of contents

- [Technology overview](#technology-overview)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running locally](#running-locally)
- [Authentication](#authentication)
- [Routes and pages](#routes-and-pages)
- [Components](#components)
- [Application services](#application-services)
- [Browser storage](#browser-storage)
- [API integration](#api-integration)
- [Real-time order updates](#real-time-order-updates)
- [Data models](#data-models)
- [Architecture and data flow](#architecture-and-data-flow)
- [Error handling and known behavior](#error-handling-and-known-behavior)
- [Deployment considerations](#deployment-considerations)
- [Development commands](#development-commands)

## Technology overview

| Area | Technology |
| --- | --- |
| UI framework | .NET 10 Blazor WebAssembly |
| Authentication | Microsoft Entra ID through MSAL |
| API communication | `HttpClient` with an authorization message handler |
| Real-time messaging | ASP.NET Core SignalR client with Azure SignalR Service |
| Styling | Bootstrap 5, Bootstrap Icons, and component-scoped CSS |
| Client-side persistence | Browser `localStorage` |
| Application language | English |

Important project files and directories:

```text
UI/
├── Components/             reusable presentation components
├── DataType/               client-side data models
├── Layout/                 main layout and navigation
├── Pages/                  routable pages
├── Services/               cart, favorites, and API interaction
├── wwwroot/
│   ├── css/                global styles
│   ├── data/               bundled sample assets and data
│   ├── appsettings.json    browser-visible runtime configuration
│   └── index.html          application host page
├── App.razor               routing and authorization shell
├── Program.cs              dependency injection and MSAL setup
└── UI.csproj               project and package configuration
```

## Prerequisites

- .NET 10 SDK
- a running `PartsOrderTracker.API` instance
- Azure SignalR Service configured for the API
- a Microsoft Entra ID single-page application registration
- the API application registration and its delegated `access_as_user` scope
- a status-processing worker that publishes order notifications for live updates
- a browser supported by Blazor WebAssembly

Check the installed SDKs with:

```powershell
dotnet --list-sdks
```

For the default local setup, the API must be available at:

```text
https://localhost:7146/
```

## Configuration

Runtime configuration is stored in `wwwroot/appsettings.json`. Because this file
is downloaded by the browser, every value in it is public and must be safe to
expose to users.

```json
{
  "ApiBaseUrl": "https://localhost:7146/",
  "ApiScope": "api://<api-client-id>/access_as_user",
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/<tenant-id>",
    "ClientId": "<spa-client-id>",
    "ValidateAuthority": true
  }
}
```

| Key | Required | Description |
| --- | --- | --- |
| `ApiBaseUrl` | yes | Base URL of `PartsOrderTracker.API`. Keep the trailing slash. |
| `ApiScope` | yes | Delegated API permission requested for the signed-in user. |
| `AzureAd:Authority` | yes | Tenant-specific Microsoft identity platform authority. |
| `AzureAd:ClientId` | yes | Client ID of the frontend SPA application registration. |
| `AzureAd:ValidateAuthority` | yes | Enables validation of the configured authority. |

The values in this file are identifiers and public client settings, not secrets.
Never add client secrets, passwords, private keys, or database connection strings
to a Blazor WebAssembly application.

### Microsoft Entra ID setup

The SPA application registration must include the local redirect URIs used by
the application, including the authentication callback path. For the default
HTTPS launch profile, configure at least:

```text
https://localhost:7053/authentication/login-callback
https://localhost:7053/authentication/logout-callback
```

The SPA must also have delegated permission to the API's `access_as_user` scope.
The scope configured in `ApiScope` must match the scope exposed by the API
application registration.

## Running locally

### 1. Start the API

From the repository root:

```powershell
dotnet run --project ".\Web API\PartsOrderTracker.Api\PartsOrderTracker.Api\PartsOrderTracker.Api.csproj" --launch-profile https
```

The API should be available at `https://localhost:7146`.

### 2. Start the UI

In a second terminal, from the repository root:

```powershell
dotnet run --project .\UI\UI.csproj --launch-profile https
```

Default development URLs:

| Resource | URL |
| --- | --- |
| UI over HTTPS | `https://localhost:7053` |
| UI over HTTP | `http://localhost:5031` |
| API over HTTPS | `https://localhost:7146` |

Trust the local development certificate if required:

```powershell
dotnet dev-certs https --trust
```

Use the HTTPS URL during normal development. The API's current CORS policy
allows the default UI origin `https://localhost:7053`, including credentials
required by the SignalR browser connection.

## Authentication

Authentication is provided by
`Microsoft.Authentication.WebAssembly.Msal`. The application uses redirect-based
login:

```text
UI → Microsoft Entra ID → /authentication/login-callback → UI
```

The named `PartsApi` HTTP client uses an `AuthorizationMessageHandler`. For
requests sent to `ApiBaseUrl`, the handler obtains a token for `ApiScope` and
adds it to the request:

```http
Authorization: Bearer <access-token>
```

Authentication-related routes are handled by `RemoteAuthenticatorView` at:

```text
/authentication/{action}
```

The navigation menu displays the current user name after sign-in and provides
login or logout actions as appropriate.

The current SignalR connection is created separately from the authorized
`PartsApi` HTTP client and does not attach an API access token. After connecting,
the UI obtains the user's object ID from the local authentication state and sends
it to the hub's `Subscribe` method. See [Real-time order updates](#real-time-order-updates)
for the complete flow and its security considerations.

Browsing the application shell does not itself require an `[Authorize]`
attribute. However, the parts and order data come from secured API endpoints.
When a token is unavailable during part loading, the application redirects the
user to the login flow. Adding items to the cart and placing an order are also
disabled or rejected for unauthenticated users.

## Routes and pages

| Route | Page | Description |
| --- | --- | --- |
| `/` | `Home.razor` | Displays the parts catalog through the `Parts` component. |
| `/cart` | `Cart.razor` | Displays cart contents, quantities, totals, and the order action. |
| `/orders` | `Orders.razor` | Displays the authenticated user's order history. |
| `/authentication/{action}` | `Authentication.razor` | Handles MSAL login, logout, and callback actions. |
| `/not-found` | `NotFound.razor` | Displays the fallback page for unknown routes. |

### Parts catalog

The home page loads active parts from `GET /api/parts` and provides:

- live text search by name, manufacturer, article number, or description;
- category filtering through a select field and filter chips;
- sorting by price or name;
- stock indication;
- favorite toggling;
- adding in-stock products to the cart for authenticated users.

The default `Most popular` option does not use popularity metrics. With the
current active-only API response, it effectively preserves the order returned
by the API.

### Shopping cart

The cart page provides:

- product name, manufacturer, article number, image, and unit price;
- increment and decrement controls;
- item removal;
- product count, total item count, and order total;
- order placement through `POST /api/orders`;
- success and failure messages after an order attempt.

The cart is cleared only after the API successfully creates the order.

### Orders

The orders page loads up to the latest 100 orders provided by the API and starts
the live order-notification connection. When a status notification arrives, the
page refreshes automatically. Each order can be expanded to show:

- the generated display number in the form `ORD-XXXXXX`;
- creation time and current status;
- expected arrival and tracking ID;
- the `Received`, `Processing`, and `Dispatched` timeline;
- ordered products, quantities, unit prices, and totals.

## Components

| Component | Responsibility |
| --- | --- |
| `PartCard` | Displays a part, favorite state, price, stock status, and add-to-cart action. |
| `StockBadge` | Displays whether a part is in or out of stock. |
| `CartBadge` | Displays the current total cart quantity in the navigation menu. |
| `OrderCard` | Displays a collapsible order summary, timeline, items, and totals. |
| `LoginDisplay` | Displays login or logout controls and the authenticated user name. |
| `RedirectToLogin` | Starts the MSAL login redirect for unauthenticated access. |

Components that subscribe to service events implement `IDisposable` and remove
their event handlers when they are disposed.

## Application services

### `CartService`

`CartService` is a scoped client-side state container responsible for both the
shopping cart and order history.

Its main responsibilities are:

- loading and saving cart data in browser `localStorage`;
- adding, updating, and removing cart items;
- calculating quantities and totals;
- creating orders through `POST /api/orders`;
- preventing concurrent order submissions from the same service instance;
- loading orders through `GET /api/orders/my`;
- refreshing orders after a SignalR notification;
- enriching order items with current part descriptions, images, and prices from
  `GET /api/parts`;
- notifying interested components through the `Changed` event.

Initialization is cached in `initTask`, so each scoped service instance only
performs its initial load once.

### `FavoritesService`

`FavoritesService` stores a set of favorite part IDs. It provides initialization,
lookup, and toggle operations, persists the set in `localStorage`, and publishes
changes through the `Changed` event.

Favorites are local to the current browser profile and are not synchronized with
the API or tied to a Microsoft Entra user account.

### `OrderNotificationService`

`OrderNotificationService` owns the SignalR `HubConnection` used by the orders
page. It:

- builds the hub URL from `ApiBaseUrl` and `/hubs/orders`;
- starts a connection with automatic reconnect enabled;
- resolves the signed-in user ID from `oid`, the Microsoft object-identifier
  claim, `nameidentifier`, or `sub`;
- invokes `Subscribe(userId)` after connecting;
- listens for `OrderStatusChanged` messages;
- calls `CartService.RefreshOrdersAsync()` when a notification arrives;
- disposes the hub connection when the scoped service is disposed.

`StartAsync()` is idempotent for the lifetime of the service: if a connection
object already exists, it does not create a second one.

## Browser storage

The application uses two `localStorage` entries:

| Key | Content | Lifetime |
| --- | --- | --- |
| `cart` | Serialized array of `CartItem` objects. | Until cleared by the user, browser, application, or a successful order. |
| `favorites` | Serialized set of favorite part IDs. | Until cleared by the user, browser, or application. |

This data is available to scripts running under the same origin. Do not store
access tokens, secrets, or sensitive personal information in these entries.

Clearing site data removes the cart and favorites. Because these entries are not
associated with a user account, different users of the same browser profile also
share the stored cart and favorite state.

## API integration

The UI uses the named `PartsApi` client for all backend requests.

| UI operation | Method and endpoint | Used by |
| --- | --- | --- |
| Load the catalog | `GET /api/parts` | `Parts` page |
| Load current user's orders | `GET /api/orders/my` | `CartService` |
| Refresh part details for order items | `GET /api/parts` | `CartService` |
| Place an order | `POST /api/orders` | `CartService` |
| Negotiate and connect for live updates | `/hubs/orders` | `OrderNotificationService` |

Order creation request:

```json
{
  "items": [
    {
      "partId": "6908a93ecdcce35769335bb8",
      "quantity": 2
    }
  ]
}
```

The API base URL must use an origin allowed by the backend's CORS policy. The
API's Entra configuration must accept tokens issued for the scope requested by
the UI.

## Real-time order updates

Live updates are activated when the user opens the `/orders` page.

### Connection and subscription

`OrderNotificationService` connects to:

```text
{ApiBaseUrl}/hubs/orders
```

It reads the signed-in user's object ID from the authentication claims and calls:

```text
Subscribe(userId)
```

The API places the connection in the Azure SignalR group named after that user
ID. The external status-processing worker must send the event to the same group.

### Notification contract

Message name:

```text
OrderStatusChanged
```

Expected payload:

```json
{
  "orderId": "6908b05acdcce35769335bc1",
  "status": "Processing"
}
```

The current callback treats the payload as an invalidation notification. It does
not patch a single order in memory. Instead, it invokes
`CartService.RefreshOrdersAsync()`, which reloads both:

```http
GET /api/orders/my
GET /api/parts
```

This guarantees that the page displays the server's current order state and
re-enriches each order item with the latest catalog image, description, and
price.

### Real-time message flow

```text
External status worker
        │  OrderStatusChanged → group: user oid
        ▼
Azure SignalR Service
        │
        ▼
OrderNotificationService
        │
        └── CartService.RefreshOrdersAsync()
                │
                ├── GET /api/orders/my
                └── GET /api/parts
                        │
                        └── Changed event → Orders page re-render
```

The status-processing worker is not part of this UI repository. Without that
publisher, the orders page still works through its initial REST API load but does
not receive live status changes.

## Data models

### `Part`

Represents a catalog item returned by the API.

| Property | Type | Purpose |
| --- | --- | --- |
| `Id` | string | MongoDB part identifier. |
| `ArticleNumber` | string | Displayed article or part number. |
| `Name` | string | Part name. |
| `Manufacturer` | string | Part manufacturer. |
| `Description` | string | Part description and searchable text. |
| `Category` | string | Category used by the catalog filters. |
| `Price` | decimal | Price in minor currency units; displayed as EUR after division by 100. |
| `ImageUrl` | string | Product image URL. |
| `InStock` | boolean | Controls stock display and cart availability. |
| `IsActive` | boolean | Client-side property retained for compatibility; the API response currently omits it. |

### `CartItem`

Contains the catalog snapshot stored in the browser cart: part ID, name, brand,
description, part number, unit price, image URL, and quantity. `Total` is computed
as `UnitPrice × Quantity`.

`PartId` uses `FlexibleStringConverter`, allowing older local data containing a
numeric ID to be read and normalized into a string.

### `Order`

Represents an order returned by the API. It contains order items, status and
timeline timestamps, user information, expected arrival, and tracking data.

Computed display properties include:

- `OrderNumber`: `ORD-` followed by the final six characters of the order ID;
- `ProductCount`: number of distinct order rows;
- `ItemCount`: sum of all ordered quantities;
- `Total`: sum of the enriched order-item totals.

### Money formatting

`MoneyFormatting.ToEuro()` treats stored price values as minor units (cents),
divides them by 100, formats the result with two decimal places using invariant
culture, and appends `EUR`. The catalog, cart, and order components use this
helper consistently.

### `OrderStatus`

The UI expects string enum values from the API:

```text
Received → Processing → Dispatched
```

## Architecture and data flow

```text
Browser
│
├── Blazor pages and components
│       │
│       ├── FavoritesService ─────► localStorage: favorites
│       │
│       └── CartService ──────────► localStorage: cart
│                    │
│                    └── named HttpClient: PartsApi
│                              │
│                              ├── AuthorizationMessageHandler
│                              │       └── MSAL / Microsoft Entra ID
│                              │
│                              └── PartsOrderTracker.API
│                                      └── MongoDB
│
├── OrderNotificationService
│       │
│       └── SignalR HubConnection
│               └── API /hubs/orders
│                       └── Azure SignalR Service
│                               └── external status worker
│
└── Component-scoped and global CSS
```

Typical order flow:

1. The catalog loads parts from the API.
2. The user adds in-stock parts to the locally persisted cart.
3. The cart creates an `items` request from its current state.
4. The authorization handler attaches an access token.
5. The API validates and creates the order.
6. The UI clears the cart and refreshes the order history.
7. When the worker later changes the order status, it publishes an
   `OrderStatusChanged` message to the user's SignalR group.
8. The UI receives the message and refreshes the order history from the API.

## Error handling and known behavior

- If part loading cannot acquire an access token, the UI starts the login flow.
- `CartService` ignores an `HttpRequestException` during its initial order load,
  allowing the rest of the client to initialize without order data.
- Order placement displays the message from an `HttpRequestException` in an
  alert. The cart remains intact after failure.
- The UI does not impose the API's maximum quantity of 100. Quantities above 100
  are rejected by the API when the user places the order.
- Cart and favorite persistence is fire-and-forget. A browser storage failure is
  not surfaced in the interface.
- Order item descriptions, images, and prices are enriched from the current part
  catalog. If a part is no longer returned by the API, these values are empty or
  zero in the order display.
- Automatic SignalR reconnect is enabled. The current implementation subscribes
  only after the initial connection; because group membership is tied to a
  connection, a reconnected client may need to invoke `Subscribe` again before
  receiving further group messages.
- `StartAsync()` leaves a failed connection object assigned. A later call does
  not retry startup unless the service is recreated.
- The current hub subscription trusts a user ID sent by the browser and the UI
  does not attach an access token to the SignalR connection. Production systems
  should authenticate the hub and derive the group from server-side claims.
- The UI interprets API price values as euro cents and divides them by 100 for
  display. The API model itself does not declare either a currency or a minor-unit
  convention, so both applications must keep this contract synchronized.
- The project currently has no automated test project.

## Deployment considerations

A published Blazor WebAssembly application consists of static files and can be
hosted by a static web server, CDN, or ASP.NET Core host.

For each deployed environment:

1. Set the correct `ApiBaseUrl`, `ApiScope`, Entra authority, and SPA client ID.
2. Register the deployed authentication callback URLs in Microsoft Entra ID.
3. Configure the API's Azure SignalR connection string and deploy the compatible
   status-notification publisher.
4. Add the deployed UI origin to the API's CORS policy and allow credentials.
5. Serve the application and API over HTTPS.
6. Configure the host to fall back to `index.html` for client-side routes.
7. If the application is hosted below a subpath, update the `<base href="/">`
   value in `wwwroot/index.html` accordingly.

Bootstrap Icons are loaded from jsDelivr at runtime. A deployment with a strict
Content Security Policy or no internet access must allow that source or host the
icon assets locally.

Publish a release build with:

```powershell
dotnet publish .\UI\UI.csproj -c Release
```

## Development commands

Restore dependencies:

```powershell
dotnet restore .\UI\UI.csproj
```

Build the UI:

```powershell
dotnet build .\UI\UI.csproj
```

Run with the HTTPS development profile:

```powershell
dotnet run --project .\UI\UI.csproj --launch-profile https
```

Verify formatting:

```powershell
dotnet format .\UI\UI.csproj --verify-no-changes
```
