# POS Backend

This is the backend service for the Point of Sale (POS) system. It provides APIs for managing products, orders, and customers.

## Project Structure

```
/pos-backend
├── src
│   ├── controllers
│   ├── models
│   ├── routes
│   └── services
├── tests
├── config
├── scripts
└── README.md
```

- **src/controllers**: Contains the logic for handling requests and responses.
- **src/models**: Defines the data models and schemas.
- **src/routes**: Manages the API endpoints and routes.
- **src/services**: Implements the business logic and interactions with the database.
- **tests**: Contains unit and integration tests.
- **config**: Configuration files for different environments.
- **scripts**: Utility scripts for database migrations, seeding, etc.

## Getting Started

### Prerequisites

- Node.js
- npm or yarn
- MongoDB

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/pos-backend.git
    ```
2. Navigate to the project directory:
    ```sh
    cd pos-backend
    ```
3. Install dependencies:
    ```sh
    npm install
    ```
    or
    ```sh
    yarn install
    ```

### Running the Application

#### Using .NET Core

1. Clean the project:
   ```
   dotnet clean PosBackend.csproj
   ```

2. Build the project:
   ```
   dotnet build PosBackend.csproj
   ```

3. Run the project:
   ```
   dotnet run --project PosBackend.csproj dev
   ```

#### Using the Scripts

##### Windows
1. Double-click on `run-backend.bat` or run it from the command line:
   ```
   run-backend.bat
   ```

##### Unix/Linux/Mac
1. Make the script executable:
   ```
   chmod +x run-backend.sh
   ```
2. Run the script:
   ```
   ./run-backend.sh
   ```

### Running Tests

```sh
npm test
```
or
```sh
yarn test
```

## API Documentation

The API documentation is available at `/api-docs` when the server is running.

## Contributing

Contributions are welcome! Please read the [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Additional Information

### Running in Development Mode

To run the application in development mode with hot-reloading:

```sh
npm run dev
```
or
```sh
yarn dev
```

### Debugging

To debug the application, you can use the following command:

```sh
npm run debug
```
or
```sh
yarn debug
```

### Tools and Technologies

This project includes the following tools and technologies:

- **Express.js**: A web application framework for Node.js.
- **Mongoose**: An ODM (Object Data Modeling) library for MongoDB and Node.js.
- **Jest**: A JavaScript testing framework.
- **Supertest**: A library for testing Node.js HTTP servers.
- **dotenv**: A module to load environment variables from a `.env` file.
- **ESLint**: A tool for identifying and fixing linting issues in JavaScript code.
- **Prettier**: A code formatter to ensure consistent code style.

### Environment Variables and Configuration

The following environment variables need to be configured in the `.env` file:

- `PORT`: The port on which the server will run.
- `MONGODB_URI`: The URI for connecting to the MongoDB database.
- `JWT_SECRET`: The secret key for signing JSON Web Tokens.

#### Sensitive Data Logging

By default, sensitive data logging is disabled in both development and production environments. This prevents potentially sensitive information from being logged, which is important for security.

If you need to enable sensitive data logging for debugging purposes in development, you can set the `EnableSensitiveDataLogging` setting to `true` in your `appsettings.Development.json` file:

```json
{
  "EnableSensitiveDataLogging": true
}
```

Note that this setting should always be `false` in production environments.

### Database Seeding

The application automatically seeds the database with initial data when it starts up if the database is empty.

### Utility Scripts

The backend includes several utility scripts for database maintenance:

- `UpdateCustomPackage.cs`: Updates the Custom package pricing in the database
- `UpdateCustomPackagePrice.cs`: Similar functionality to update Custom package pricing
- `UpdatePriceConsole/Program.cs`: Console application to update pricing

These scripts are not meant to be used as entry points for the application. They are utility scripts that can be called programmatically when needed.

### Updating the Custom Package Price

To update the Custom package price in the database, you can use one of the following methods:

#### Method 1: Using the C# Script
1. Run the following command from the backend directory:
   ```
   dotnet run --project PosBackend.csproj
   ```

   This will automatically update the Custom package price during application startup.

   Alternatively, you can use the utility methods directly in code:
   ```csharp
   // Call the utility method
   await UpdateCustomPackage.UpdatePackage();
   ```

#### Method 2: Using Direct SQL
Run the following SQL script using your preferred PostgreSQL client or tool:

```sql
-- Update the Custom package price directly
UPDATE "PricingPackages"
SET "Price" = 49.99,
    "MultiCurrencyPrices" = '{"ZAR": 899.99, "EUR": 45.99, "GBP": 39.99}'
WHERE "Type" = 'custom';

-- If the Custom package doesn't exist, insert it
INSERT INTO "PricingPackages" ("Title", "Description", "Icon", "ExtraDescription", "Price", "TestPeriodDays", "Type", "Currency", "MultiCurrencyPrices")
SELECT 'Custom', 'Build your own package;Select only what you need;Flexible pricing;Scalable solution;Pay for what you use', 'MUI:SettingsIcon', 'Create a custom solution that fits your exact needs', 49.99, 14, 'custom', 'USD', '{"ZAR": 899.99, "EUR": 45.99, "GBP": 39.99}'
WHERE NOT EXISTS (SELECT 1 FROM "PricingPackages" WHERE "Type" = 'custom');
```

This SQL script is also available in the file `update-custom-package-direct.sql`.

### Linting and Formatting

To check for linting issues, run:

```sh
npm run lint
```
or
```sh
yarn lint
```

To format the code, run:

```sh
npm run format
```
or
```sh
yarn format
```