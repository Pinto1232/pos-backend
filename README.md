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

1. Start the MongoDB server.
2. Configure environment variables in the `.env` file.
3. Run the application:
    ```sh
    npm start
    ```
    or
    ```sh
    yarn start
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

### Environment Variables

The following environment variables need to be configured in the `.env` file:

- `PORT`: The port on which the server will run.
- `MONGODB_URI`: The URI for connecting to the MongoDB database.
- `JWT_SECRET`: The secret key for signing JSON Web Tokens.

### Database Seeding

To seed the database with initial data, run the following command:

```sh
npm run seed
```
or
```sh
yarn seed
```

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